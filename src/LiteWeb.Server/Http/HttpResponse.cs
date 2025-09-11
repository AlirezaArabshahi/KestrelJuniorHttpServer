using System.Net.Sockets;

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
}