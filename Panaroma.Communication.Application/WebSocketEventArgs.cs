using System;

namespace Panaroma.Communication.Application
{
    public class WebSocketEventArgs : EventArgs
    {
        public string Message { get; set; }
        public object Sender { get; set; }
    }
}