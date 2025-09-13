using System.Net.Sockets;
using System.Text;

/// <summary>
/// Represents an outgoing HTTP response. It provides methods to build and send a response back to the client.
/// </summary>
public class HttpResponse
{
    private readonly NetworkStream _stream;
    public int StatusCode { get; set; } = 200;
    public string StatusMessage { get; set; } = "OK";
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HttpResponse(NetworkStream stream)
    {
        _stream = stream;
    }
    
    /// <summary>
    /// Sends the complete HTTP response to the client.
    /// </summary>
    public async Task SendAsync(string body, int statusCode, string statusMessage, string contentType = "text/html; charset=utf-8")
    {
        StatusCode = statusCode;
        StatusMessage = statusMessage;
        
        var bodyBytes = Encoding.UTF8.GetBytes(body);

        Headers["Content-Type"] = contentType;
        Headers["Content-Length"] = bodyBytes.Length.ToString();
        Headers["Connection"] = "close"; // We will close the connection after the response

        var responseBuilder = new StringBuilder();
        responseBuilder.Append($"HTTP/1.1 {StatusCode} {StatusMessage}\r\n");
        foreach (var header in Headers)
        {
            responseBuilder.Append($"{header.Key}: {header.Value}\r\n");
        }
        responseBuilder.Append("\r\n"); // End of headers

        var headerBytes = Encoding.UTF8.GetBytes(responseBuilder.ToString());

        await _stream.WriteAsync(headerBytes);
        await _stream.WriteAsync(bodyBytes);
        await _stream.FlushAsync();
    }
}