using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KestrelJuniorHttpServer.Core;

public class HttpListener
{
    private TcpListener? _tcpListener;
    private readonly string _prefix;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoop;

    public HttpListener(string prefix)
    {
        _prefix = prefix;
    }
    
    public void Start()
    {
        if (_tcpListener != null) throw new InvalidOperationException("Already started");
        if (string.IsNullOrWhiteSpace(_prefix)) throw new InvalidOperationException("Prefix is empty");
        var uri = new Uri(_prefix);
        var ipAddress = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) switch
        {
            true => IPAddress.Loopback,
            _ => IPAddress.Parse(uri.Host),
        };
        _tcpListener = new TcpListener(ipAddress, uri.Port);
        _tcpListener.Start();
        Console.WriteLine($"Listening on {uri}");
        _cts = new CancellationTokenSource();
        _acceptLoop = AcceptClientsAsync(_cts.Token);
    }

    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        if (_tcpListener == null) return;
        
        while (!ct.IsCancellationRequested)
        {
            var client = await _tcpListener.AcceptTcpClientAsync(ct);
            _ = HandleClientAsync(client, ct); // Handle client connection in a new Task
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        Console.WriteLine("Client connected!");
        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
            var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var requestLine = request.Split('\n')[0].Trim();
            if (!string.IsNullOrEmpty(requestLine))
            {
                var (method, path, httpVersion) = HttpRequestParser.ParseRequestLine(requestLine);
                Console.WriteLine($"Method: {method}, Path: {path}, HTTP Version: {httpVersion}");
            }

            // For now, just close the connection
            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error handling client: {e.Message}");
        }
    }

    public async Task StopAsync()
    {
        if (_tcpListener == null || _cts == null) throw new InvalidOperationException("Not started");
        _cts.Cancel();
        _tcpListener.Stop(); // Stop the listener first to unblock AcceptTcpClientAsync
        if (_acceptLoop != null)
        {
            await _acceptLoop.ConfigureAwait(false);
        }
        _cts.Dispose();
        _tcpListener = null;
        Console.WriteLine($"Stopped listening on {_prefix}");
    }
}