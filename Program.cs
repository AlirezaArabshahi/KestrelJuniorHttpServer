using KestrelJrHttpListener.Core;

var server = new HttpListener("http://localhost:11231");
server.Start();
Console.WriteLine("Press any key to stop the server.");
Console.ReadKey();
server.Stop();
Console.WriteLine("Server stopped.");