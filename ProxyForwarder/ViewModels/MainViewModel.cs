using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;

namespace ProxyForwarder.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private string _ip = string.Empty;
    private int? _localPort = 3000;
    private string _password = string.Empty;
    private int? _port;
    private string _proxyString = string.Empty;

    private string _username = string.Empty;

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register<ServerRunningStateChangedMessage>(this, (r, m) =>
        {
            ButtonText = m.Value ? "Stop Server" : "Start Server";
            IsServerRunning = m.Value;
        });
    }

    public static IEnumerable<ProxyMethod> ProxyMethodValues => Enum.GetValues<ProxyMethod>();

    [ObservableProperty]
    public partial string ButtonText { get; set; } = "Start Server";

    [ObservableProperty]
    public partial HttpServer HttpListenerService { get; set; } = new();

    public string Ip
    {
        get => _ip; set
        {
            SetProperty(ref _ip, value);
            CheckIfEnabled();
        }
    }

    [ObservableProperty]
    public partial bool IsButtonEnabled { get; set; } = false;

    [ObservableProperty]
    public partial bool IsServerRunning { get; set; } = false;

    public int? LocalPort
    {
        get => _localPort; set
        {
            SetProperty(ref _localPort, value);
            CheckIfEnabled();
        }
    }

    public string Password
    {
        get => _password; set
        {
            SetProperty(ref _password, value);
            CheckIfEnabled();
        }
    }

    public int? Port
    {
        get => _port; set
        {
            SetProperty(ref _port, value);
            CheckIfEnabled();
        }
    }

    [ObservableProperty]
    public partial ProxyMethod ProxyMethod { get; set; }

    public string ProxyString
    {
        get => _proxyString; set
        {
            if (!value.Contains("://"))
            {
                value = "http://" + value; // Default to HTTP if no protocol is present
                ProxyMethod = ProxyMethod.HTTP;
            }
            else
            {
                ProxyMethod = value.StartsWith("http://") ? ProxyMethod.HTTP : ProxyMethod.SOCKS5;
            }

            var uri = new Uri(value);

            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var userInfoParts = uri.UserInfo.Split(':');
                Username = userInfoParts[0];
                if (userInfoParts.Length > 1)
                {
                    Password = userInfoParts[1];
                }
            }

            Ip = uri.Host;
            Port = uri.Port;

            SetProperty(ref _proxyString, value);
        }
    }

    public string Username
    {
        get => _username; set
        {
            SetProperty(ref _username, value);
            CheckIfEnabled();
        }
    }

    [RelayCommand]
    public void ToggleServer()
    {
        try
        {
            if (IsServerRunning)
            {
                HttpListenerService.Stop();
            }
            else
            {
                HttpListenerService.Start(LocalPort!.Value, Ip, Port!.Value, Username, Password, ProxyMethod == ProxyMethod.HTTP ? Titanium.Web.Proxy.Models.ExternalProxyType.Http : Titanium.Web.Proxy.Models.ExternalProxyType.Socks5);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void CheckIfEnabled()
    {
        IsButtonEnabled = !string.IsNullOrEmpty(Ip) && Port.HasValue && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && LocalPort.HasValue;
    }
}