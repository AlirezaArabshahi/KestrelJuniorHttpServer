using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// Represents an outgoing HTTP response. It provides methods to build and send a response back to the client.
/// </summary>
public class HttpResponse
{
    private readonly NetworkStream _stream;
    public int StatusCode { get; set; } = 200;
    public string StatusMessage { get; set; } = "OK";
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public ReadOnlyMemory<byte>? Body { get; set; }

    public HttpResponse(NetworkStream stream)
    {
        _stream = stream;
    }
    
    public async Task SendAsync(CancellationToken ct = default)
    {
        // Status line
        var statusLine = Encoding.UTF8.GetBytes($"HTTP/1.1 {StatusCode} {StatusMessage}\r\n");
        await _stream.WriteAsync(statusLine, ct);

        // Headers
        if (Body.HasValue && !Headers.ContainsKey("Content-Length"))
        {
            Headers["Content-Length"] = Body.Value.Length.ToString();
        }

        Headers["Connection"] = "close";


        foreach (var header in Headers)
        {
            var headerBytes = Encoding.UTF8.GetBytes($"{header.Key}: {header.Value}\r\n");
            await _stream.WriteAsync(headerBytes, ct);
        }

        // End of headers
        await _stream.WriteAsync(Encoding.UTF8.GetBytes("\r\n"), ct);

        // Body
        if (Body.HasValue)
        {
            await _stream.WriteAsync(Body.Value, ct);
        }
        await _stream.FlushAsync(ct);

    }
}