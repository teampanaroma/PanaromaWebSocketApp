namespace Panaroma.Communication.Application
{
    public class Worker
    {
        public TcpCommand TcpCommand { get; }

        public Worker(TcpCommand tcpCommand)
        {
            TcpCommand = tcpCommand;
            InternalCommunication.GetInternalCommunication().Method = tcpCommand.Method;
        }
    }
}