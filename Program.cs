using KestrelJuniorHttpServer.Core;

var server = new HttpListener("http://localhost:11231");
server.Start();
Console.WriteLine("Press any key to stop the server.");
Console.ReadKey();
await server.StopAsync();
Console.WriteLine("Server stopped.");