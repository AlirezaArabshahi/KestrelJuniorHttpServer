using LiteWeb.Framework; 
using LiteWeb.Server;    

// 1. Create a builder for the web application
var builder = WebAppExample.CreateBuilder(args);

// 2. Build the application
var app = builder.Build();

// 3. Define the routes
app.MapGet("/", (HttpContext context) => {
    context.Response.Body = "<h1>Hello from LiteWebFramework!</h1>";
    context.Response.ContentType = "text/html";
    return Task.CompletedTask;
});

app.MapGet("/about", (HttpContext context) => {
    context.Response.Body = "This is the about page.";
    return Task.CompletedTask;
});

// 4. Run the application
await app.RunAsync();
