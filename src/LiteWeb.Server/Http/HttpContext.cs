/// <summary>
/// Encapsulates all HTTP-specific information about an individual HTTP request and response.
/// </summary>
public class HttpContext
{
    public HttpRequest Request { get; }
    public HttpResponse Response { get; }

    public HttpContext(HttpRequest request, HttpResponse response)
    {
        Request = request;
        Response = response;
    }
}