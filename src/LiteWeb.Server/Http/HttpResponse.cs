using System.Net.Sockets;
using System.Text;
using System.IO; // Added for MemoryStream
using System.Threading;

/// <summary>
/// Represents an outgoing HTTP response. It now uses an internal buffer
/// to allow for incremental body construction.
/// </summary>
public class HttpResponse
{
    private readonly NetworkStream _stream;
    
    // Internal buffer for incrementally building the response body
    private readonly MemoryStream _bodyBuffer = new();

    public int StatusCode { get; set; } = 200;
    public string StatusMessage { get; set; } = "OK";
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    public HttpResponse(NetworkStream stream)
    {
        _stream = stream;
    }

    /// <summary>
    /// Writes raw bytes to the response body buffer.
    /// </summary>
    public void Write(ReadOnlySpan<byte> data)
    {
        _bodyBuffer.Write(data);
    }

    /// <summary>
    /// Writes a string to the response body buffer using UTF-8 encoding.
    /// </summary>
    public void Write(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        _bodyBuffer.Write(bytes, 0, bytes.Length);
    }
    

    public async Task SendAsync(CancellationToken ct = default)
    {
        // Status line
        var statusLine = Encoding.UTF8.GetBytes($"HTTP/1.1 {StatusCode} {StatusMessage}\r\n");
        await _stream.WriteAsync(statusLine, ct);

        // Set headers based on the final state of the buffer
        if (_bodyBuffer.Length > 0)
        {
            Headers["Content-Length"] = _bodyBuffer.Length.ToString();
        }

        Headers["Connection"] = "close";


        foreach (var header in Headers)
        {
            var headerBytes = Encoding.UTF8.GetBytes($"{header.Key}: {header.Value}\r\n");
            await _stream.WriteAsync(headerBytes, ct);
        }

        // End of headers
        await _stream.WriteAsync(Encoding.UTF8.GetBytes("\r\n"), ct);

        // Send the final content from the buffer
        if (_bodyBuffer.Length > 0)
        {
            // Send the accumulated content in the buffer as a single write
            await _stream.WriteAsync(_bodyBuffer.ToArray(), ct);
        }

        await _stream.FlushAsync(ct);
    }
}