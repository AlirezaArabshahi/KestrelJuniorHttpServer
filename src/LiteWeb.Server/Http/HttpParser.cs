using System.Net.Sockets;
using System.Text;

public static class HttpParser
{
    public static async Task<HttpRequest?> ParseRequestAsync(NetworkStream stream, CancellationToken ct)
    {
        // 1. Read and parse the request line
        var requestLineString = await ReadLineAsync(stream, ct);
        if (string.IsNullOrEmpty(requestLineString)) return null;
        
        var requestLineParts = requestLineString.Split(' ', 3);
        if (requestLineParts.Length != 3) return null;

        var method = requestLineParts[0];
        var path = requestLineParts[1];
        var httpVersion = requestLineParts[2];

        // 2. Read and parse the headers
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? headerLine;
        while (!string.IsNullOrEmpty(headerLine = await ReadLineAsync(stream, ct)))
        {
            var headerParts = headerLine.Split(':', 2);
            if (headerParts.Length == 2)
            {
                headers[headerParts[0].Trim()] = headerParts[1].Trim();
            }
        }

        // 3. Read the request body as raw bytes
        ReadOnlyMemory<byte>? body = null;
        if (headers.TryGetValue("Content-Length", out var contentLengthString) &&
            int.TryParse(contentLengthString, out var contentLength) && contentLength > 0)
        {
            // we read the body as raw bytes
            var bodyBuffer = new byte[contentLength];
            int totalBytesRead = 0;
            while (totalBytesRead < contentLength)
            {
                int bytesRead = await stream.ReadAsync(bodyBuffer, totalBytesRead, contentLength - totalBytesRead, ct);
                if (bytesRead == 0) break; // connection closed
                totalBytesRead += bytesRead;
            }
            // body = new ReadOnlyMemory<byte>(bodyBuffer);
            body = new ReadOnlyMemory<byte>(bodyBuffer, 0, totalBytesRead); 

        }
        return new HttpRequest(method, path, httpVersion, headers, body);
    }
    // Read a line from the stream
    private static async Task<string> ReadLineAsync(NetworkStream stream, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        while (true)
        {
            int readByte = stream.ReadByte();
            if (readByte == -1 || ct.IsCancellationRequested) 
            {
                // end of stream or request canceled
                break;
            }

            if (readByte == '\n') 
            {
                // we found the end of the line
                break;
            }

            if (readByte != '\r')
            {
                memoryStream.WriteByte((byte)readByte);
            }
        }
        // we use ASCII encoding because HTTP protocol lines must be ASCII
        return Encoding.ASCII.GetString(memoryStream.ToArray());
    }
}
