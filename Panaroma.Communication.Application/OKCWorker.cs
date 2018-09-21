namespace Panaroma.Communication.Application
{
    public class OKCWorker : Worker, IWorker
    {
        public OKCWorker(TcpCommand tcpCommand) : base(tcpCommand)
        {
        }

        public void DoWork()
        {
            if(!(TcpCommand.OKCModel == "Verifone"))
                return;
            new OKCVerifone(TcpCommand).DoWork();
        }
    }
}