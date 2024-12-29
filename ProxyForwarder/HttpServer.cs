using CommunityToolkit.Mvvm.Messaging;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Net;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Models;

namespace ProxyForwarder
{
    public class HttpServer
    {
        private bool _isRunning = false;
        private ProxyServer? _proxyServer;
        private MsmhAgnosticServer? _socksServer;
        private ExternalProxyType _proxyType;

        public HttpServer()
        {
            WeakReferenceMessenger.Default.Register<ServerRunningStateChangedMessage>(this, (r, m) =>
            {
                _isRunning = m.Value;
            });
        }

        public void Start(int localPort, string host, int port, string username, string password, ExternalProxyType proxyType)
        {
            if (_isRunning)
            {
                Stop();
            }

            _proxyType = proxyType;

            if (ExternalProxyType.Socks5 == proxyType)
            {
                _socksServer = new();

                AgnosticSettings settings = new()
                {
                    Working_Mode = AgnosticSettings.WorkingMode.DnsAndProxy,
                    ListenerPort = localPort,
                    UpstreamProxyScheme = $"socks5://{host}:{port}",
                    UpstreamProxyPass = password,
                    UpstreamProxyUser = username,
                    BootstrapIpAddress = IPAddress.Parse("127.0.0.1")
                };

                _socksServer.Start(settings);
            }
            else
            {
                _proxyServer = new ProxyServer(false, false, false);

                var explicitEndpoint = new ExplicitProxyEndPoint(IPAddress.Parse("127.0.0.1"), localPort, false);

                _proxyServer.AddEndPoint(explicitEndpoint);

                var proxy = new ExternalProxy(host, port, username, password) { ProxyType = proxyType };


                _proxyServer.UpStreamHttpProxy = proxy;
                _proxyServer.UpStreamHttpsProxy = proxy;

                _proxyServer.StartAsync();
            }

            WeakReferenceMessenger.Default.Send(new ServerRunningStateChangedMessage(true));

        }

        public void Stop()
        {
            WeakReferenceMessenger.Default.Send(new ServerRunningStateChangedMessage(false));
            
            if (_proxyType == ExternalProxyType.Socks5)
            {
                _socksServer?.Stop();
                _socksServer = null;
            }
            else
            {
                _proxyServer?.Stop();
                _proxyServer?.Dispose();
                _proxyServer = null;
            }

        }
    }
}