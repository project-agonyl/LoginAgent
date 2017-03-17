using System;
using System.Net.Sockets;
using System.Threading;
using Login_Agent_578.Config;

namespace Login_Agent_578.ClientSession
{
    internal class zaSession : IDisposable
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
                    ZoneAgentChecker.Dispose();
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

        ~zaSession()
        {
            Dispose(false);
        }

        internal zaSession(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            Buffer = new byte[8092];
            AgentID = 0;
            ServerID = 0;
            Port = -1;
            IPadress = string.Empty;
            AliveTime = DateTime.Now;
            ZoneAgentChecker = new Timer(CheckSessionAlive, null, 1000, 1000);
        }

        //이벤트: 디스커넥트
        internal event EventMsg SessionClosed;
        //이벤트: 존에이전트 현황 업데이트
        internal event EventMsg StatusUpdate;

        private Timer ZoneAgentChecker;
        internal TcpClient TcpClient { get; private set; }
        internal Byte[] Buffer { get; private set; }
        internal NetworkStream Stream
        {
            get { return TcpClient.GetStream(); }
        }
        internal Byte AgentID { get; set; }
        internal Byte ServerID { get; set; }
        internal Int32 Port { get; set; }
        internal String IPadress { get; set; }
        internal DateTime AliveTime { get; set; }

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
        /// Connection check: Zone Agent
        /// </summary>
        /// <param name="sender"></param>
        private void CheckSessionAlive(Object state)
        {
            StatusUpdate(this);
            if (!isConnected(this.TcpClient.Client) || this.AliveTime.AddSeconds(10) < DateTime.Now)
            {
                SessionClosed(this);
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
