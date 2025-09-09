using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KestrelJuniorHttpServer.Core;

public class HttpListener
{
    private TcpListener? _tcpListener;
    private readonly string _prefix;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoop;

    public HttpListener(string prefix)
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
        _acceptLoop = AcceptClientsAsync(_cts.Token);
    }

    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        if (_tcpListener == null) return;
        
        while (!ct.IsCancellationRequested)
        {
            var client = await _tcpListener.AcceptTcpClientAsync(ct);
            _ = HandleClientAsync(client, ct); // Handle client connection in a new Task
        }
    }

    // EDUCATIONAL-NOTE:
    // This method demonstrates manual buffer handling for parsing an HTTP request.
    // It is intentionally written for learning purposes to show the low-level details of processing raw TCP streams.
    // The current implementation with `MemoryStream.ToArray()` inside the loop is intentionally inefficient
    // to highlight common pitfalls in low-level network programming.
    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        Console.WriteLine("Client connected!");
        using var stream = client.GetStream();
        // Manual buffer management for reading request line and headers
        var buffer = new byte[4096]; // A reasonable size for request line and headers
        int bytesRead;
        using var ms = new MemoryStream();
        
        // Read bytes until we find the end of the header block (\r\n\r\n)
        // This is a simplified approach. A more robust solution would handle partial reads
        // and potentially very large headers more efficiently.
        while (true)
        {
            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
            if (bytesRead == 0) break; // End of stream or connection closed

            ms.Write(buffer, 0, bytesRead);

            // At this point, 'ms' (MemoryStream) contains the raw bytes read from the network stream so far.
            // For example, if the request was "GET / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n",
            // 'ms' would internally hold the byte sequence corresponding to this string.
            // In hexadecimal representation, these bytes would look like:
            // 47 45 54 20 2F 20 48 54 54 50 2F 31 2E 31 0D 0A 48 6F ...
            // ------------------

            // Calling ms.ToArray() converts these accumulated bytes into a byte array.
            // e.g., this will produce a byte array like [71, 69, ..., 13, 10]
            // which is the byte representation of the request string 
            // "GET / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n"
            var accumulatedBytes = ms.ToArray(); // Inefficient for large streams, but okay for headers
            
            // Check for end of headers marker (\r\n\r\n)
            // This is a naive search. For performance, a more optimized search (e.g., KMP)
            // or a dedicated HTTP parser library would be used.
            if (accumulatedBytes.Length >= 4)
            {
                int headerSectionEndPosition = -1;
                for (int i = 0; i < accumulatedBytes.Length - 3; i++)
                {
                    if (accumulatedBytes[i] == '\r' && accumulatedBytes[i+1] == '\n' &&
                        accumulatedBytes[i+2] == '\r' && accumulatedBytes[i+3] == '\n')
                    {
                        headerSectionEndPosition = i + 4; // +4 to include \r\n\r\n
                        break;
                    }
                }
                if (headerSectionEndPosition != -1)
                {
                    // If end of headers is found, process headers and potentially body
                    var headerBytes = new byte[headerSectionEndPosition];
                    Array.Copy(accumulatedBytes, 0, headerBytes, 0, headerSectionEndPosition);
                    
                    // Convert the accumulated bytes to string
                    // [71, 69, ..., 13, 10] -> "GET / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n"
                    var headersString = Encoding.UTF8.GetString(headerBytes);

                    // Split the full headers string into headerLines
                    // e.g., headerLines -> ["GET / HTTP/1.1", "Host: localhost:8080", "", ""]

                    var headerLines = headersString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    // Extract Request Line
                    // e.g., requestLine -> "GET / HTTP/1.1"

                    var requestLine = headerLines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
                    if (string.IsNullOrEmpty(requestLine))
                    {
                        client.Close();
                        return;
                    }
                    // e.g., method -> "GET", path -> "/", httpVersion -> "HTTP/1.1"
                    var (method, path, httpVersion) = HttpRequestParser.ParseRequestLine(requestLine);
                    Console.WriteLine($"Method: {method}, Path: {path}, HTTP Version: {httpVersion}");
                    // Extract Headers
                    var rawHeaderLines = new List<string>();
                    bool inHeadersSection = false;
                    foreach (var line in headerLines)
                    {
                        if (line == requestLine) // Skip the request line itself
                        {
                            inHeadersSection = true;
                            continue;
                        }

                        if (inHeadersSection)
                        {
                            if (string.IsNullOrWhiteSpace(line)) // Empty line signifies end of headers
                            {
                                break;
                            }
                            rawHeaderLines.Add(line);
                        }
                    }
                    // e.g., rawHeaderLines -> ["Host: localhost:8080"]
                    var headers = HttpRequestParser.ParseHeaders(rawHeaderLines);
                    // e.g., headers -> { "Host": "localhost:8080" }
                    Console.WriteLine("Headers:");
                    foreach (var header in headers)
                    {
                        Console.WriteLine($"  {header.Key}: {header.Value}");
                    }

                    // Handle request body if Content-Length header is present
                    if (headers.TryGetValue("Content-Length", out var contentLengthString) &&
                        int.TryParse(contentLengthString, out var contentLength))
                    {
                        Console.WriteLine($"Content-Length: {contentLength}");

                        // Calculate how much of the body has already been read with the headers unintentionally
                        int bodyBytesReadWithHeaders = accumulatedBytes.Length - headerSectionEndPosition;
                        
                        int remainingBodyBytesToRead = contentLength - bodyBytesReadWithHeaders;

                        // If there's more body to read than what's already in ms
                        if (bodyBytesReadWithHeaders < contentLength)
                        {
          
                            var bodyBuffer = new byte[remainingBodyBytesToRead];
                            int totalBodyBytesRead = bodyBytesReadWithHeaders;

                            // Read the remaining body bytes
                            while (totalBodyBytesRead < contentLength)
                            {
                                // calculate the position in the buffer where we should write the next bytes
                                var currentBufferPosition = totalBodyBytesRead - bodyBytesReadWithHeaders;

                                // calculate the number of bytes we should read in this pass
                                var bytesToReadInThisPass = remainingBodyBytesToRead - currentBufferPosition;

                                bytesRead = await stream.ReadAsync(bodyBuffer, currentBufferPosition, bytesToReadInThisPass, ct);
                               
                                if (bytesRead == 0) break; // Connection closed prematurely
                                totalBodyBytesRead += bytesRead;
                            }
                            // Now 'bodyBuffer' contains the full request body
                            string requestBody = Encoding.UTF8.GetString(bodyBuffer, 0, totalBodyBytesRead - bodyBytesReadWithHeaders);
                            Console.WriteLine($"Request Body:\n{requestBody}");
                        } else if (bodyBytesReadWithHeaders >= contentLength) {
                            // The entire body (or more) was read along with the headers
                            string requestBody = Encoding.UTF8.GetString(accumulatedBytes, headerSectionEndPosition, contentLength);
                            Console.WriteLine($"Request Body:\n{requestBody}");
                        }
                    }

                    break; // Exit the while loop after processing headers and body
                }
            }
        }

        // For now, just close the connection
        client.Close();
    }

    public async Task StopAsync()
    {
        if (_tcpListener == null || _cts == null) throw new InvalidOperationException("Not started");
        _cts.Cancel();
        _tcpListener.Stop(); // Stop the listener first to unblock AcceptTcpClientAsync
        if (_acceptLoop != null)
        {
            await _acceptLoop.ConfigureAwait(false);
        }
        _cts.Dispose();
        _tcpListener = null;
        Console.WriteLine($"Stopped listening on {_prefix}");
    }
}