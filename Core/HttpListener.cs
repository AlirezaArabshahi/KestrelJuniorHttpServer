using System.Net;
using System.Net.Sockets;

namespace KestrelJrHttpListener.Core;

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

    private async Task AcceptClientsAsync(CancellationToken token)
    {
        if (_tcpListener == null) return;
        
        while (!token.IsCancellationRequested)
        {
            var client = await _tcpListener.AcceptTcpClientAsync(token);
            // Handle client connection here (e.g., in a new Task)
            // Temporarily close the connection as request handling is not yet implemented
            // This prevents the client from hanging indefinitely
            client.Close();
            Console.WriteLine("Client connected!");
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