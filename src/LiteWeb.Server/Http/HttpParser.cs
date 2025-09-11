namespace LiteWeb.Server.Http;

public class HttpParser
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

    public static async Task<Dictionary<string, string>> ParseHeadersAsync(StreamReader reader)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? line;
        while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync().ConfigureAwait(false)))
        {
            if (string.IsNullOrWhiteSpace(line)) continue; // Skip any empty lines
            
            var idx = line.IndexOf(':');
            if (idx <= 0) continue; // Skip malformed header lines
            
            var name = line[..idx].Trim();
            var value = line[(idx + 1)..].Trim();
            headers[name] = value;
        }
        return headers;

        // alternative way
        // while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
        // {
        //     var parts = line.Split(':', 2);
        //     if (parts.Length == 2)
        //     {
        //         headers[parts[0].Trim()] = parts[1].Trim();
        //     }
        // }
        // return headers;
    }

    public static async Task<string> ParseBodyAsync(StreamReader reader, int contentLength)
    {
        if (contentLength <= 0) return string.Empty;

        var bodyBuffer = new char[contentLength];
        int totalRead = 0;
        while (totalRead < contentLength)
        {
            int read = await reader.ReadAsync(bodyBuffer, totalRead, contentLength - totalRead);
            if (read == 0) break; // End of stream
            totalRead += read;
        }
        return new string(bodyBuffer, 0, totalRead);

        // alternative way
        // var buffer = new char[contentLength];
        // await reader.ReadBlockAsync(buffer, 0, contentLength);
        // return new string(buffer);
    }
}