/// <summary>
/// Represents an incoming HTTP request. It holds all the parsed information from the client's request.
/// </summary>
public class HttpRequest
{
    public string Method { get; }
    public string Path { get; }
    public string HttpVersion { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public string? Body { get; }

    public HttpRequest(string method, string path, string httpVersion, IReadOnlyDictionary<string, string> headers, string? body)
    {
        Method = method;
        Path = path;
        HttpVersion = httpVersion;
        Headers = headers;
        Body = body;
    }
}