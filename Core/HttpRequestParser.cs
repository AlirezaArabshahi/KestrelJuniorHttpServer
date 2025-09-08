namespace KestrelJuniorHttpServer.Core;

public class HttpRequestParser
{
    public static (string Method, string Path, string HttpVersion) ParseRequestLine(string requestLine)
    {
        var parts = requestLine.Split(' ');
        if (parts.Length != 3)
        {
            throw new ArgumentException("Invalid request line format.");
        }
        return (parts[0], parts[1], parts[2]);
    }
}