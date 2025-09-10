namespace LiteWeb.Server.Http;

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

    public static Dictionary<string, string> ParseHeaders(IEnumerable<string> rawHeaderLines)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in rawHeaderLines)
        {
            var parsedHeader = ParseHeaderLine(line);
            if (parsedHeader.HasValue)
            {
                headers[parsedHeader.Value.Name] = parsedHeader.Value.Value;
            }
        }
        return headers;
    }

    private static (string Name, string Value)? ParseHeaderLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null; // Skip any empty lines
        var idx = line.IndexOf(':');
        if (idx <= 0)
        {
            return null; // Malformed header line
        }
        var name = line[..idx].Trim();
        var value = line[(idx + 1)..].Trim();
        return (name, value);
    }
}