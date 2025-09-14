using System.Net.Sockets;
using System.Text;

// This test simulates packetfragmentation - a common edge case where data is split 
// across multiple TCP packets during transmission.
var tcpClient = new TcpClient();
await tcpClient.ConnectAsync("localhost", 11231); // Connect to your server
Console.WriteLine("Connected to server...");

using var stream = tcpClient.GetStream();

// First part of the body that will be sent with headers
string firstPartOfBody = "{\"name\":\"part1";

// Second part of the body that will be sent with a delay
string secondPartOfBody = "\",\"value\":\"part2\"}";

// Calculate total content length
int totalContentLength = firstPartOfBody.Length + secondPartOfBody.Length;

// Create headers string
string headers =
    $"POST /test HTTP/1.1\r\n" +
    $"Host: localhost:8080\r\n" +
    $"Content-Type: application/json\r\n" +
    $"Content-Length: {totalContentLength}\r\n" +
    $"\r\n";

// Combine headers with the first part of the body
string initialPayload = headers + firstPartOfBody;
byte[] initialPayloadBytes = Encoding.UTF8.GetBytes(initialPayload);

// 1. Send first part (headers + first part of body)
await stream.WriteAsync(initialPayloadBytes);
Console.WriteLine("Sent headers and first part of the body.");

// 2. Create a intentional delay to separate network packets
Console.WriteLine("Waiting for 500ms...");
await Task.Delay(500);

// 3. Send second part of the body
byte[] secondPartOfBodyBytes = Encoding.UTF8.GetBytes(secondPartOfBody);
await stream.WriteAsync(secondPartOfBodyBytes);
Console.WriteLine("Sent second part of the body.");

Console.WriteLine("Request sent completely. Press enter to exit.");
Console.ReadLine();