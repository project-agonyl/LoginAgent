using Login_Agent_578.Config;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Login_Agent_578.ClientSession
{
    internal class cltSession : IDisposable
    {
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ClientChecker.Dispose();
                    if (this.TcpClient.Connected)
                    {
                        this.TcpClient.GetStream().Close();
                        this.TcpClient.Close();
                    }
                }
                this.Buffer = null;

                disposed = true;
            }
        }

        ~cltSession()
        {
            Dispose(false);
        }

        internal cltSession(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            Buffer = new byte[4096];
            PCID = 0;
            Port = -1;
            IPadress = string.Empty;
            connTime = DateTime.Now;
            Ver = ClientVer.undefined;
            Step = "LA";
            ClientChecker = new Timer(CheckSessionAlive, null, 1000, 1000);
        }

        //이벤트: 디스커넥트
        internal event EventMsgClt cltSessionClosed;

        private Timer ClientChecker;
        internal TcpClient TcpClient { get; private set; }
        internal byte[] Buffer { get; private set; }
        internal NetworkStream Stream
        {
            get { return TcpClient.GetStream(); }
        }
        internal ClientVer Ver { get; set; }
        internal uint PCID { get; set; }
        internal int Port { get; set; }
        internal string IPadress { get; set; }
        internal DateTime connTime { get; set; }
        internal string Step { get; set; }

        /// <summary>
        /// 소켓 상태 리턴
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool isConnected(Socket s)
        {
            try { return !((s.Poll(1, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected); }
            catch { return false; }
        }

        /// <summary>
        /// Connection check: Client
        /// </summary>
        /// <param name="sender"></param>
        private void CheckSessionAlive(object sender)
        {
            if (!isConnected(this.TcpClient.Client) || this.connTime.AddMinutes(3) < DateTime.Now)
            {
                cltSessionClosed(this);
            }
        }

        /// <summary>
        /// Stream Write
        /// </summary>
        /// <param name="buffer"></param>
        internal void Send(Byte[] buffer)
        {
            try
            {
                if (this.Stream.CanWrite)
                    this.Stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(WriteCallback), this.Stream);
            }
            catch { }
        }

        /// <summary>
        /// Stream Write Callback
        /// </summary>
        /// <param name="result"></param>
        private void WriteCallback(IAsyncResult result)
        {
            NetworkStream Stream = (NetworkStream)result.AsyncState;
            if (Stream != null)
                Stream.EndWrite(result);
        }
    }
}
