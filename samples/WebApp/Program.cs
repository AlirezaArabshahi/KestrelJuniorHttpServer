using LiteWeb.Server.Core;
var server = new LiteWebServer("http://localhost:11231");

server.Start();

Console.WriteLine("Server is running. Press any key to stop the server.");
Console.ReadKey();

await server.StopAsync();

Console.WriteLine("Server stopped successfully.");


