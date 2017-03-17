using Login_Agent_578.ClientSession;
using Login_Agent_578.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Login_Agent_578
{
    internal class LoginServer
    {
        //이벤트: 로그 파일 기록
        internal event EventMsgPrintStr WriteLogs;
        //이벤트: 로그창 출력
        internal event EventMsgPrintStr PrintLogs;
        //이벤트: PCID현황 출력
        internal event EventMsgPrintUint PrintPCID;
        //이벤트: za상태 업데이트
        internal event EventMsg UpdateStatus;
        //이벤트: New Packet 저장
        internal event EventMsgNewPacket WriteNewPacket;

        private TcpListener login_Server;
        private gConfig g_Config;
        private byte[] zRestBytes;

        internal LoginServer()
        {
            this.zRestBytes = null;
            g_Config = gConfig.getInstance();
            login_Server = new TcpListener(IPAddress.Parse("127.0.0.1"), g_Config.ZAPort);
        }

        internal void Close()
        {
            login_Server.Stop();
        }

        internal bool Start()
        {
            try
            {
                login_Server.Start();
                login_Server.BeginAcceptTcpClient(SessionHandler, null);
                PrintLogs(string.Format("127.0.0.1:{0}:Listen OK", g_Config.ZAPort));
                return true;
            }
            catch
            {
                PrintLogs(string.Format("127.0.0.1:{0}:Listen ERROR", g_Config.ZAPort));
                return false;
            }
        }

        /// <summary>
        /// 접속자 수, 연결된 za 수 업데이트
        /// </summary>
        /// <param name="session"></param>
        private void zaStatusUpdate(zaSession session)
        {
            UpdateStatus(session);
        }

        /// <summary>
        /// za 종료
        /// </summary>
        private void zaSessionClosed(zaSession session)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                //219ZA는 종료전 모든 유저의 디스커넥트 패킷을 보내온다.
                //이후 사용자들이 개발한 za는 그렇제 못하므로 za가 디스커넥트되면 로그인 유저목록을 클리어 해준다.
                for (int i = g_Config.LoggedInList.Count - 1; i >= 0; i--)
                {
                    if (g_Config.LoggedInList.ElementAt(i).Value.ServerID == session.ServerID)
                    {
                        sb.Clear();
                        PrintLogs(sb.AppendFormat("loginuser.destroy {0}", g_Config.LoggedInList.ElementAt(i).Key).ToString());
                        g_Config.LoggedInList.Remove(g_Config.LoggedInList.ElementAt(i).Key);
                    }
                }

                sb.Clear();
                PrintLogs(sb.AppendFormat("<ZA>AgentID={0} Disconnected", session.AgentID).ToString());
                g_Config.ZoneAgentList.Remove(session.ServerID);
                session.Dispose();
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.zaSessionClosed:{0}{1}", Environment.NewLine, ex));
            }
        }
    
        /// <summary>
        /// Session Handler
        /// </summary>
        /// <param name="asyncResult"></param>
        private void SessionHandler(IAsyncResult asyncResult)
        {
            try
            {
                TcpClient session = login_Server.EndAcceptTcpClient(asyncResult);
                zaSession zAgent = new zaSession(session);
                zAgent.Stream.BeginRead(zAgent.Buffer, 0, zAgent.Buffer.Length, new AsyncCallback(RequestReceived), zAgent);
                login_Server.BeginAcceptTcpClient(SessionHandler, null);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.ZoneAgentHandler:{0}{1}", Environment.NewLine, ex));
            }
        }

        /// <summary>
        /// 메시지 받음
        /// </summary>
        /// <param name="asyncResult"></param>
        private void RequestReceived(IAsyncResult asyncResult)
        {
            zaSession zAgent = (zaSession)asyncResult.AsyncState;
            try
            {
                if (zAgent == null) return;
                int length = zAgent.Stream.EndRead(asyncResult);
                if (length == 0) return;

                List<Byte[]> buffers;
                this.zRestBytes = Packet.SplitPackets(zAgent.Buffer, length, out buffers, this.zRestBytes);
                zAgent.Stream.BeginRead(zAgent.Buffer, 0, zAgent.Buffer.Length, new AsyncCallback(RequestReceived), zAgent);

                ExecuteCommand(zAgent, buffers);
            }
            catch (IOException) { }
            catch (InvalidOperationException) { }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.RequestReceived:{0}{1}", Environment.NewLine, ex));
            }
        }

        /// <summary>
        /// 메시지 한개씩 루핑
        /// </summary>
        /// <param name="buffers"></param>
        private void ExecuteCommand(zaSession zAgent, List<Byte[]> buffers)
        {
            foreach (Byte[] buffer in buffers)
            {
                MsgAnalysis(zAgent, buffer);
            }
        }

        /// <summary>
        /// 메시지 분석
        /// </summary>
        /// <param name="buffer"></param>
        private void MsgAnalysis(zaSession zAgent, Byte[] buffer)
        {
            try
            {
                if (buffer[9] == 0xE0 && buffer[8] == 0x02)
                {
                    NewZoneAgentConnected(zAgent, buffer);
                }
                else if (buffer[9] == 0xE1 && buffer[8] == 0x02)
                {
                    //ZA로부터 도착하는 지속적인 확인 패킷: 답장 필요없음, 4:접속자수, 1:za_count, 1:za_count
                    //해당 za의 패킷 도착시간을 지속적으로 업데이트해서 디스커넥트 확인에 사용함.
                    zAgent.AliveTime = DateTime.Now;
                }
                else if (buffer[9] == 0xE2 && buffer[8] == 0x02)
                {
                    PlayerLogout(zAgent, buffer);
                }
                else if (buffer[9] == 0xE3 && buffer[8] == 0x02)
                {
                    PreparedUser2EnterGame(zAgent, buffer);
                }
                else if (buffer[9] == 0xE4 && buffer[8] == 0x02)
                {
                    LoginUserRecoverMsg(zAgent, buffer);
                }
                else
                {
                    WriteNewPacket("ZA->LS", buffer, "localhost", ClientVer.undefined);
                }
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.MsgAnalysis:{0}{1}{0}Buffer:{0}{2}", Environment.NewLine, ex, BitConverter.ToString(buffer).Replace("-", " ")));
            }
        }

        /// <summary>
        /// 새 존 에이전트 접속
        /// </summary>
        /// <param name="zAgent"></param>
        /// <param name="buffer"></param>
        private void NewZoneAgentConnected(zaSession zAgent, Byte[] buffer)
        {
            try
            {
                MSG_ZA2LS_CONNECT new_za = new MSG_ZA2LS_CONNECT();
                new_za.SetBuffer(buffer);
                if (!g_Config.ZoneAgentList.ContainsKey(new_za.byServerID))
                {
                    //zone agent 정보 저장
                    zAgent.ServerID = new_za.byServerID;
                    zAgent.AgentID = new_za.byAgentID;
                    zAgent.IPadress = new_za.szIPAdress;
                    zAgent.Port = new_za.dwPort;
                    g_Config.ZoneAgentList.Add(new_za.byServerID, zAgent);
                    PrintLogs(string.Format("<ZA>Receive SID={0} AgentID={1} {2}:{3}", new_za.byServerID, new_za.byAgentID, new_za.szIPAdress, new_za.dwPort));
                    //정상 등록되면 이벤트 등록
                    zAgent.StatusUpdate += zaStatusUpdate;
                    zAgent.SessionClosed += zaSessionClosed;
                }
                else
                {
                    //중복 ServerID의 에이전트는 등록 거부
                    PrintLogs(string.Format("<ZA>Receive AgentID={0} Duplicate ServerID={1}", new_za.byAgentID, new_za.byServerID));
                    PrintLogs(string.Format("<ZA>AgentID={0} Connection Rejected", new_za.byAgentID));
                    zAgent.Dispose();
                }
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.NewZoneAgentConnected:{0}{1}{0}Buffer:{0}{2}", Environment.NewLine, ex, BitConverter.ToString(buffer).Replace("-", " ")));
            }
        }

        /// <summary>
        /// 플레이어 로그아웃 처리
        /// </summary>
        /// <param name="zAgent"></param>
        /// <param name="buffer"></param>
        private void PlayerLogout(zaSession zAgent, Byte[] buffer)
        {
            try
            {
                //로그아웃 패킷: 확인 후 로그인 사용자 목록에서 삭제
                MSG_ZA2LS_ACC_LOGOUT pLogout = new MSG_ZA2LS_ACC_LOGOUT();
                pLogout.SetBuffer(buffer);
                PrintLogs(string.Format("<ZA>Receive User Logout={0} {1}", pLogout.MsgHeader.dwPCID, pLogout.szAccount));
                if (g_Config.LoggedInList.ContainsKey(pLogout.szAccount))
                {
                    g_Config.LoggedInList.Remove(pLogout.szAccount);
                    PrintLogs(string.Format("loginuser.destroy {0}", pLogout.szAccount));
                }
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.PlayerLogout:{0}{1}{0}Buffer:{0}{2}", Environment.NewLine, ex, BitConverter.ToString(buffer).Replace("-", " ")));
            }
        }

        /// <summary>
        /// Prepared유저가 za에 접속 성공했음
        /// </summary>
        /// <param name="zAgent"></param>
        /// <param name="buffer"></param>
        private void PreparedUser2EnterGame(zaSession zAgent, Byte[] buffer)
        {
            try
            {
                //유저가 za에 정상 접속했음을 알리는 패킷 : 로그인 목록에서 유저의 UID 변경
                MSG_ZA2LS_PREPARED_ACC_LOGIN pLogin = new MSG_ZA2LS_PREPARED_ACC_LOGIN();
                pLogin.SetBuffer(buffer);
                g_Config.LoggedInList[pLogin.szAccount].PCID = pLogin.MsgHeader.dwPCID;
                PrintLogs(string.Format("<ZA>Receive User Login={0} {1}", pLogin.MsgHeader.dwPCID, pLogin.szAccount));
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.PreparedUser2EnterGame:{0}{1}{0}Buffer:{0}{2}", Environment.NewLine, ex, BitConverter.ToString(buffer).Replace("-", " ")));
            }
        }

        /// <summary>
        /// 로그인 서버가 종료 후 재접속 했을경우 za가 보내주는 접속 유저 정보
        /// </summary>
        /// <param name="zAgent"></param>
        /// <param name="buffer"></param>
        private void LoginUserRecoverMsg(zaSession zAgent, Byte[] buffer)
        {
            try
            {
                //ZS Login user Recover packet
                MSG_ZA2LS_LOGIN_USER_LIST pRecover = new MSG_ZA2LS_LOGIN_USER_LIST();
                pRecover.SetBuffer(buffer);
                g_Config.za_Uid.Uid = pRecover.MsgHeader.dwPCID;
                g_Config.LoggedInList.Add(pRecover.szUserAccount, new LoginUser(pRecover.MsgHeader.dwPCID, zAgent.ServerID));
                PrintLogs(string.Format("<ZA>Recover User UID={0} ID={1}", pRecover.MsgHeader.dwPCID, pRecover.szUserAccount));
                PrintPCID(g_Config.za_Uid.dspUid);
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.LoginUserRecoverMsg:{0}{1}{0}Buffer:{0}{2}", Environment.NewLine, ex, BitConverter.ToString(buffer).Replace("-", " ")));
            }
        }
    }
}