using System.Net;
using System.Net.Sockets;

namespace KestrelJrHttpListener.Core;

public class HttpListener
{
    private TcpListener? _tcpListener;
    private string _prefix;

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
    }

    public void Stop()
    {
        if (_tcpListener == null) throw new InvalidOperationException("Not started");
        _tcpListener?.Stop();
        _tcpListener = null;
        Console.WriteLine($"Stopped listening on {_prefix}");
    }
}