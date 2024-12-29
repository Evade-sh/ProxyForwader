using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyForwarder
{
    public class ServerRunningStateChangedMessage(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}