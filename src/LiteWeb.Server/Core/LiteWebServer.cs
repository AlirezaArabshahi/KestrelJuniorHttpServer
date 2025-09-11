using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

using LiteWeb.Server.Http; 
namespace LiteWeb.Server.Core;

public class LiteWebServer
{
    private TcpListener? _tcpListener;
    private readonly string _prefix;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoop;

    // The channel acts as a queue for incoming TcpClient connections
    private Channel<TcpClient>? _requestChannel;

    // Task for the request processing loop (the consumer)
    private Task? _requestProcessorLoop;


    public LiteWebServer(string prefix)
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

        // Initialize and start the request queue and processor
        _requestChannel = Channel.CreateUnbounded<TcpClient>();
        _requestProcessorLoop = ProcessRequestsAsync(_cts.Token);


        _acceptLoop = AcceptClientsAsync(_cts.Token);
    }

    // This is the PRODUCER loop. Its only job is to accept connections
    // and quickly add them to the channel.
    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        if (_tcpListener == null || _requestChannel == null) return;
        Console.WriteLine("Connection acceptor loop started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var client = await _tcpListener.AcceptTcpClientAsync(ct);
                // Write the client to the channel. The consumer will pick it up.
                await _requestChannel.Writer.WriteAsync(client, ct);
            }
            catch (OperationCanceledException)
            {
                // This exception is expected when the listener is stopped.
            }
        }
        Console.WriteLine("Connection acceptor loop stopped.");

    }

    // The CONSUMER loop
    // This loop reads from the channel and dispatches the work to HandleClientAsync.
    private async Task ProcessRequestsAsync(CancellationToken ct)
    {
        if (_requestChannel == null) return;
        Console.WriteLine("Request processor loop started.");
        // Read from the channel until it's completed and empty.
        try
        {
            await foreach (var client in _requestChannel.Reader.ReadAllAsync(ct))
            {
                // Do not await this. We want the loop to immediately continue
                // and be ready to pick up the next request from the channel.
                _ = HandleClientAsync(client, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the server is stopping.
        }
        Console.WriteLine("Request processor loop stopped.");
    }

    // This method handles each client connection.
    // It now creates HttpRequest, HttpResponse, and HttpContext objects.
    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        Console.WriteLine("Client connected!");
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true, detectEncodingFromByteOrderMarks: true))
        {
            // Read the request line
            var requestLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(requestLine))
            {
                return; // Invalid request, close connection.
            }

            var (method, path, httpVersion) = HttpParser.ParseRequestLine(requestLine);

            var headers = await HttpParser.ParseHeadersAsync(reader);
            
            string? requestBody = null;
            // Handle request body if Content-Length header is present
            if (headers.TryGetValue("Content-Length", out var contentLengthString) &&
                int.TryParse(contentLengthString, out var contentLength) && contentLength > 0)
            {
                requestBody = await HttpParser.ParseBodyAsync(reader, contentLength);
            }
            
            // *** Create Core HTTP Abstractions ***
            var request = new HttpRequest(method, path, httpVersion, headers, requestBody);
            var response = new HttpResponse(stream);
            var context = new HttpContext(request, response);

            // Now that we have the context, we can process it.
            // In a real framework, you would pass this 'context' object to a request pipeline,
            // middleware, or a router to generate a meaningful response.
            
            // TODO: pass the context to the framework
        }
    }


    public async Task StopAsync()
    {
        if (_tcpListener == null || _cts == null || _requestChannel == null) throw new InvalidOperationException("Not started");

        // 1. Signal cancellation to all running tasks.
        _cts.Cancel();
        // 2. Stop the listener to prevent new connections from being accepted.
        _tcpListener.Stop();

        // 3. Mark the channel as complete, signaling that no more items will be added.
        // This allows the consumer loop (ProcessRequestsAsync) to finish gracefully.
        _requestChannel.Writer.Complete();

        // 4. Wait for both the acceptor and processor loops to finish their work.
        var tasksToWait = new List<Task>();
        if (_acceptLoop != null) tasksToWait.Add(_acceptLoop);
        if (_requestProcessorLoop != null) tasksToWait.Add(_requestProcessorLoop);


        try
        {
            await Task.WhenAll(tasksToWait).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log or handle exceptions that might occur during shutdown, but don't rethrow.
            Console.WriteLine($"An exception occurred during shutdown: {ex.Message}");
        }


        // 5. Clean up resources.
        _cts.Dispose();
        _tcpListener = null;
        Console.WriteLine($"Stopped listening on {_prefix}");
    }
}