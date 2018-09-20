using Fleck;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Panaroma.Communication.Application
{
    public class WebSocketServer
    {
        public List<IWebSocketConnection> _webSocketConnections = new List<IWebSocketConnection>();

        public event MessageChanged OnMessageChanged;

        public WebSocketServer()
        {
            new Fleck.WebSocketServer(string.Format("ws://0.0.0.0:{0}",
                ConfigurationManager.AppSettings["WebSocketPort"]))
            {
                RestartAfterListenError = true
            }.Start(socket =>
            {
                socket.OnOpen = () => _webSocketConnections.Add(socket);
                socket.OnClose = () => _webSocketConnections.Remove(socket);
                socket.OnMessage = message => PanaromaWebSocketServer_OnMessageChanged(new WebSocketEventArgs()
                {
                    Message = message,
                    Sender = this
                });
            });
        }

        public void SendMessage(string message)
        {
            IWebSocketConnection webSocketConnection = _webSocketConnections.LastOrDefault();
            if(webSocketConnection == null)
            {
                return;
            }

            webSocketConnection.Send(message);
        }

        private void PanaromaWebSocketServer_OnMessageChanged(WebSocketEventArgs e)
        {
            MessageChanged onMessageChanged = OnMessageChanged;
            if(onMessageChanged == null)
                return;
            onMessageChanged(e);
        }

        public delegate void MessageChanged(WebSocketEventArgs e);
    }
}