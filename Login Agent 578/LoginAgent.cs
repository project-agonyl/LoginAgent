using Login_Agent_578.ClientSession;
using Login_Agent_578.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Login_Agent_578
{
    internal class LoginAgent
    {
        //이벤트: 로그 파일 기록
        internal event EventMsgPrintStr WriteLogs;
        //이벤트: 로그창 출력
        internal event EventMsgPrintStr PrintLogs;
        //이벤트: PCID현황 출력
        internal event EventMsgPrintUint PrintPCID;
        //이벤트: New Packet 저장
        internal event EventMsgNewPacket WriteNewPacket;

        private TcpListener login_Agent;
        private Timer BlacklistChecker;
        private Timer ExpiredChecker;
        private Dictionary<string, DateTime> Blacklist;
        private Dictionary<string, WrongTry> FailedAttempt;
        private LAUID la_PCID;
        private gConfig g_Config;
        private SqlClient Acc_DB;


        internal void Close()
        {
            BlacklistChecker.Dispose();
            ExpiredChecker.Dispose();
            login_Agent.Stop();
        }

        internal LoginAgent()
        {
            g_Config = gConfig.getInstance();
            Acc_DB = new SqlClient();

            //Create a UID to use in LA.(start value)
            //LoginAgent에서 사용할 uid 시작값 지정해서 생성
            la_PCID = new LAUID(2047);
            login_Agent = new TcpListener(IPAddress.Any, g_Config.LAPort);
            FailedAttempt = new Dictionary<string, WrongTry>();
            Blacklist = new Dictionary<string, DateTime>();
            BlacklistChecker = new Timer(ReleaseBlacklist, null, Timeout.Infinite, Timeout.Infinite);
            ExpiredChecker = new Timer(CheckExpiredAccount, null, Timeout.Infinite, Timeout.Infinite);
        }

        internal bool Start()
        {
            try
            {
                login_Agent.Start();
                login_Agent.BeginAcceptTcpClient(SessionHandler, null);
                WriteLogs(string.Format("0.0.0.0:{0}:Listen OK", g_Config.LAPort));
                BlacklistChecker.Change(0, 60000);
                if (g_Config.isExpiration)
                    ExpiredChecker.Change(0, 600000);
                return true;
            }
            catch
            {
                WriteLogs(string.Format("0.0.0.0:{0}:Listen ERROR", g_Config.LAPort));
                return false;
            }
        }

        /// <summary>
        /// New client.
        /// 클라이언트 핸들러
        /// </summary>
        /// <param name="asyncResult"></param>
        private void SessionHandler(IAsyncResult asyncResult)
        {
            try
            {
                TcpClient session = login_Agent.EndAcceptTcpClient(asyncResult);
                cltSession client = new cltSession(session);
                client.cltSessionClosed += SessionClosed;
                g_Config.ClientsList.Add(client);
                client.Stream.BeginRead(client.Buffer, 0, client.Buffer.Length, new AsyncCallback(RequestReceived), client);
                login_Agent.BeginAcceptTcpClient(SessionHandler, null);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.ZoneAgentHandler:{0}{1}", Environment.NewLine, ex));
            }
        }

        /// <summary>
        /// Client session termination event.
        /// 클라이언트 세션 종료
        /// </summary>
        /// <param name="client"></param>
        private void SessionClosed(cltSession client)
        {
            if (g_Config.LoggedInList.Values.Any(x => x.PCID == client.PCID))
            {
                var User = g_Config.LoggedInList.First(x => x.Value.PCID == client.PCID);
                g_Config.LoggedInList.Remove(User.Key);
                PrintLogs(string.Format("loginuser.destroy {0}", User.Key));
            }
#if DEBUG
            PrintLogs(string.Format("Auto.destroy: {0}@{1}:{2}", client.PCID, client.IPadress, client.Port));
#endif
            if (client.Step == "ZA")
                g_Config.inZoneAgent--;
            g_Config.ClientsList.Remove(client);
            client.Dispose();
        }

        /// <summary>
        /// Message arrives from client.
        /// 클라이언트로부터 메시지 도착
        /// </summary>
        /// <param name="asyncResult"></param>
        private void RequestReceived(IAsyncResult asyncResult)
        {
            cltSession client = (cltSession)asyncResult.AsyncState;
            try
            {
                if (client == null) return;
                int length = client.Stream.EndRead(asyncResult);
                if (length < 10)
                {
                    client.TcpClient.Client.Disconnect(false);
                    return;
                }
                ExecuteCommand(client, client.Buffer);
                client.Stream.BeginRead(client.Buffer, 0, client.Buffer.Length, new AsyncCallback(RequestReceived), client);
            }
            catch (IOException) { }
            catch (InvalidOperationException) { }
            catch (Exception ex)
            {
                WriteLogs(string.Format("LoginServer.RequestReceived:{0}{1}", Environment.NewLine, ex));
            }
        }

        /// <summary>
        /// 블랙리스트, 밴IP 확인
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool isBlacklistOrBanned(cltSession client)
        {
            //비밀번호 무한 대입 사용자 처리
            if (Blacklist.ContainsKey(client.IPadress))
            {
#if DEBUG
                PrintLogs(string.Format("BlacklistIP: {0}", client.IPadress));
#endif
                Blacklist[client.IPadress] = DateTime.Now;
                client.Send(Packet.SayMessage(g_Config.Msg_Blacklist, 0, 0, client.Ver));
                RemoveClient(client, true, false);
                return true;
            }
            else
            {
                //banned ip, 점검중 체크
                string sMsg = BanOrMaintainance(client.IPadress);
                if (!string.IsNullOrEmpty(sMsg))
                {
#if DEBUG
                    PrintLogs("BanOrMaintainance: RemoveClient");
#endif
                    client.Send(Packet.SayMessage(sMsg, 0, 0, client.Ver));
                    RemoveClient(client, true, true);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 메시지 처리
        /// </summary>
        /// <param name="buffers"></param>
        private void ExecuteCommand(cltSession client, Byte[] buffer)
        {
            IPEndPoint clientEp = (IPEndPoint)client.TcpClient.Client.RemoteEndPoint;
            client.IPadress = clientEp.Address.ToString();
            client.Port = clientEp.Port;

            //클라이언트 버전에따라 패킷 자르고 아이피 검사(블랙리스트, 밴, 공사중...)
            if (CheckClientVersion(client, ref buffer) && !isBlacklistOrBanned(client))
            {
                //새로운 요청이면 uid발급
                if (client.PCID == 0)
                {
                    client.PCID = la_PCID.Uid;
                    PrintLogs(string.Format("NewClient: {0}:{1} {2}", client.IPadress, client.Port, client.PCID));
                }
                MsgAnalysis(client, buffer);
            }
        }

        /// <summary>
        /// 메시지 분석
        /// </summary>
        /// <param name="buffer"></param>
        private void MsgAnalysis(cltSession client, Byte[] buffer)
        {
            if (buffer[9] == 0xE0 && buffer[8] == 0x01)
            {
                //로그인 요청
                ReqUserLogin(client, buffer);
            }
            else if (buffer[9] == 0xE1 && buffer[8] == 0x01)
            {
                //서버 선택 패킷: 로그인 인정된 사용자의 패킷만 처리
                if (g_Config.LoggedInList.Values.Any(x => x.PCID == client.PCID))
                    SelectedServer(client, buffer);
                else
                    client.TcpClient.Client.Disconnect(false);
            }
            else
            {
                WriteNewPacket("CL->LA", buffer, client.IPadress, client.Ver);
            }
        }

        /// <summary>
        /// Handling login packets.
        /// 로그인 요청 패킷 처리
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        private void ReqUserLogin(cltSession client, Byte[] buffer)
        {
            string str_id = Packet.Bytes2String(buffer, 0x0A);
            string str_pwd = Packet.Bytes2String(buffer, 0x1F);

            //check blank
            if (string.IsNullOrEmpty(str_id) || string.IsNullOrEmpty(str_pwd))
            {
                client.Send(Packet.SayMessage(g_Config.Msg_Incorrect, client.PCID, 0, client.Ver));
                RemoveClient(client, true, true);
#if DEBUG
                PrintLogs(string.Format("BlankID/PWD: RemoveClient({0})", client.PCID));
#endif
            }
            else
            {
                //get db info by account
                AccountInfo AccInfo = Acc_DB.getAccountInfo(str_id);
                if (string.IsNullOrEmpty(AccInfo.id))
                {
                    client.Send(Packet.SayMessage(g_Config.Msg_Incorrect, client.PCID, 0, client.Ver));
                    RemoveClient(client, true, true);
#if DEBUG
                    PrintLogs(string.Format("Wrong ID: RemoveClient({0})", client.PCID));
#endif
                }
                else
                {
                    //check pwd
                    if (!Hasher.Verify(str_pwd, AccInfo.pwd, (HashType)g_Config.hashType))
                    {
                        client.Send(Packet.SayMessage(g_Config.Msg_Incorrect, client.PCID, 0, client.Ver));
                        RemoveClient(client, true, true);
#if DEBUG
                        PrintLogs(string.Format("Wrong PWD: RemoveClient({0})", client.PCID));
#endif
                    }
                    else
                    {
                        //check status
                        if (g_Config.RejectDB.ContainsKey(AccInfo.status))
                        {
                            client.Send(Packet.SayMessage(g_Config.RejectDB[AccInfo.status], client.PCID, 3, client.Ver));
                            RemoveClient(client, true, false);
#if DEBUG
                            PrintLogs(string.Format("StatusReject: RemoveClient({0})", client.PCID));
#endif
                        }
                        else
                        {
#if DEBUG
                            PrintLogs(string.Format("{0} ExpireDate: {1}", AccInfo.id, AccInfo.expDate));
#endif
                            //check expire
                            if (g_Config.isExpiration && DateTime.Now >= AccInfo.expDate)
                            {
                                client.Send(Packet.SayMessage(g_Config.Msg_Expired, client.PCID, 3, client.Ver));
                                RemoveClient(client, true, false);
                            }
                            else
                            {
                                //log-in succeed: check duplicate connections
                                if (!g_Config.LoggedInList.ContainsKey(str_id))
                                {
                                    string msg = string.Empty;
                                    //아직 서버 선택전이므로 로그인 유저 정보의 Sid = -1로 설정함
                                    g_Config.LoggedInList.Add(str_id, new LoginUser(client.PCID, -1));
                                    if (g_Config.isExpiration)
                                    {
                                        TimeSpan ts = AccInfo.expDate - DateTime.Now;
                                        msg = string.Format(g_Config.Msg_ExpireInfo, ts.Days, ts.Hours, ts.Minutes);
                                    }
                                    client.Send(Packet.CreateSvrList(client.PCID, client.Ver, g_Config.ZoneAgentList, g_Config.ServerList, msg));
                                    PrintLogs(string.Format("SuccLogin: {0}:{1} {2}", client.IPadress, client.Port, str_id));
#if DEBUG
                                    PrintLogs(string.Format("RegLoginUser: {0}", str_id));
#endif
                                }
                                else
                                {
                                    //duplication act: in LA OR in ZA - uid가 ZaUID의 최소값(65535)이상이면 이미 za로 넘어간 유저
                                    PrintLogs(string.Format("Duplicate login detected {0} {1}", g_Config.LoggedInList[str_id].PCID, str_id));
                                    //za 유저인경우 za에게 디스커넥트 요청을 해야함: 완료시 30패킷을 되돌려받음
                                    if (g_Config.LoggedInList[str_id].PCID > g_Config.za_Uid.MinValue)
                                    {
                                        zaSession zAgent = g_Config.ZoneAgentList[g_Config.LoggedInList[str_id].ServerID];
                                        zAgent.Send(Packet.reqDisconnectAcc(g_Config.LoggedInList[str_id].PCID, str_id));
                                        PrintLogs(string.Format("Request UserDrop {0} {1} to ZoneAgent", g_Config.LoggedInList[str_id].PCID, str_id));
                                    }
                                    g_Config.LoggedInList.Remove(str_id);
                                    PrintLogs(string.Format("Previous LoggedInUser.destroy {0}", str_id));

                                    client.Send(Packet.SayMessage(g_Config.Msg_Duplication, client.PCID, 2, client.Ver));
                                    RemoveClient(client, true, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Server selection packet processing.
        /// 서버 선택 패킷
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        private void SelectedServer(cltSession client, Byte[] buffer)
        {
            //22패킷(za 정보: ip, port, new uid) 넘겨주고 접속자 정보의 기존uid를 새 uid로 변경하기.
            short ServerID = buffer[10];
            if (g_Config.ZoneAgentList.ContainsKey(ServerID))
            {
                PrintLogs(string.Format("<CT>ServerID={0}", ServerID));

                var User = g_Config.LoggedInList.First(x => x.Value.PCID == client.PCID);
                zaSession zAgent = g_Config.ZoneAgentList[ServerID];
                //ZS용 UID발급
                uint zs_PCID = g_Config.za_Uid.Uid;
                PrintPCID(g_Config.za_Uid.dspUid);
                //유저가 몇번 서버로 이동하는지 기록함. 응답 패킷에는 서버 번호가 없으므로 여기서 미리 변경
                User.Value.ServerID = ServerID;
                //za에게 유저 정보 넘기기
                zAgent.Send(Packet.PreparedAccount(zs_PCID, User.Key));
                //클라에게 za정보 넘기기
                client.Send(Packet.ZoneAgentInfo(client.PCID, zs_PCID, zAgent.IPadress, zAgent.Port, client.Ver));
                //za에 유저가 정상 접속한다면 za에서 답패킷(1f,02,e3)이 날아옴

                g_Config.inZoneAgent++;
                client.Step = "ZA";
            }
            else
            {
                PrintLogs(string.Format("<CT>ServerID={0} but ZoneAgent is Down", ServerID));
            }
        }

        /// <summary>
        /// Client version checking and packet trimming.
        /// 클라이언트 버전 확인및 패킷 자르기
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        private bool CheckClientVersion(cltSession client, ref Byte[] buffer)
        {
            if (client.Ver == ClientVer.undefined)
            {
                int length = BitConverter.ToInt32(buffer, 0);
                if ((ClientVer.v578 == (ClientVer)length && (g_Config.WorkMode == 0 || g_Config.WorkMode == 3)) ||
                    (ClientVer.v562 == (ClientVer)length && (g_Config.WorkMode == 0 || g_Config.WorkMode == 2)))
                {
                    client.Ver = (ClientVer)length;
                    Packet.TrimPacket(ref buffer, 0);
                }
                else if (ClientVer.v219 == (ClientVer)BitConverter.ToInt32(buffer, 0x0A) && (g_Config.WorkMode == 0 || g_Config.WorkMode == 1))
                {
                    client.Ver = ClientVer.v219;
                    Packet.TrimPacket(ref buffer, 0x0A);
                    int cVer = BitConverter.ToInt32(buffer, 0x34);
                    //v219에는 클라 버전 체크 기능이 있음. 버전 확인 후 아래 패킷을 보내면 클라 강제 종료됨.
                    if (g_Config.cLowVer > cVer || g_Config.cHighVer < cVer)
                    {
                        client.Send(Packet.InvalidClientVersion(client.Ver));
                        return false;
                    }
                }
                else
                {
                    client.TcpClient.Client.Disconnect(false);
                    return false;
                }
            }
            else if (client.Ver == ClientVer.v219)
            {
                Packet.TrimPacket(ref buffer, 0x0A);
            }
            else
            {
                Packet.TrimPacket(ref buffer, 0);
            }
            return true;
        }

        /// <summary>
        /// Ban IP or Maintainance or Admin: return msg
        /// IP를 기반으로 Ban된 사용자인지 혹은 점검중(허용 IP는 점검중에도 통과)인지 체크해서 메시지 돌려줌
        /// </summary>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        private string BanOrMaintainance(string clientIP)
        {
            string msg = string.Empty;
            bool isBan = false;
            bool isAllow = false;
            bool isAdmin = false;
            for (int i = 0; i < g_Config.BannedIP.Count; i++)
            {
                if (isBan = IsInRange(clientIP, g_Config.BannedIP[i]))
                    break;
            }
            for (int i = 0; i < g_Config.AllowedIP.Count; i++)
            {
                if (isAllow = IsInRange(clientIP, g_Config.AllowedIP[i]))
                    break;
            }
            for (int i = 0; i < g_Config.AdminIP.Count; i++)
            {
                if (isAdmin = IsInRange(clientIP, g_Config.AdminIP[i]))
                    return msg;
            }

            if (isBan && !isAllow)
                msg = g_Config.Msg_Banned;
            else if (g_Config.isMaintainance && !isAdmin)
                msg = g_Config.Msg_Maintainance;

            return msg;
        }

        /// <summary>
        /// IP comparison. Corresponds to CIDR expression
        /// IP비교: IP주소및 CIDR표현식에 대응
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="cidr"></param>
        /// <returns></returns>
        private bool IsInRange(string ipAddress, string cidr)
        {
            try
            {
                string[] parts = cidr.Split('/').Select(s => s.Trim()).Where(s => s != string.Empty).ToArray();
                if (parts.Length != 2)
                    return parts[0] == ipAddress;
                int baseAddr = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
                int ipAddr = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);
                int mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));
                return ((baseAddr & mask) == (ipAddr & mask));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Invalid attempt processing: More than 5 times, Blacklist registration
        /// 무한 입력 카운트해서 잘못된입력 5회 초과시 블랙리스트 등록
        /// </summary>
        /// <param name="ip"></param>
        private void CountFailedAttempts(string ip)
        {
            if (!FailedAttempt.ContainsKey(ip))
            {
                FailedAttempt.Add(ip, new WrongTry());
#if DEBUG
                PrintLogs("Failed.count: 1");
#endif
            }
            else if (FailedAttempt[ip].Time.AddSeconds(10) > DateTime.Now)
            {
                FailedAttempt[ip].Time = DateTime.Now;
                FailedAttempt[ip].Count++;
#if DEBUG
                PrintLogs(string.Format("Failed.count: {0}", FailedAttempt[ip].Count));
#endif
                if (FailedAttempt[ip].Count > 5)
                {
                    if (!Blacklist.ContainsKey(ip))
                        Blacklist.Add(ip, DateTime.Now);
                    Blacklist[ip] = DateTime.Now;
                    FailedAttempt.Remove(ip);
                    PrintLogs(string.Format("Blacklist.Added:{0}", ip));
                    WriteLogs(string.Format("Blacklist.Added:{0}", ip));
                }
            }
            else if (FailedAttempt[ip].Time.AddSeconds(10) < DateTime.Now)
            {
                FailedAttempt[ip].Time = DateTime.Now;
                FailedAttempt[ip].Count = 1;
#if DEBUG
                PrintLogs("Failed.count: Reset");
#endif
            }
        }

        /// <summary>
        /// Released from the blacklist.
        /// 1분마다 체크해서 블랙리스트 목록에서 10분 지난 사용자 제외 시키기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReleaseBlacklist(object sender)
        {
            //블랙리스트중 10분 경과한 ip 삭제
            for (int i = Blacklist.Count - 1; i >= 0; i--)
            {
                if (Blacklist.ElementAt(i).Value.AddMinutes(10) < DateTime.Now)
                {
                    PrintLogs(string.Format("Blacklist.Remove:{0}", Blacklist.ElementAt(i).Key));
                    Blacklist.Remove(Blacklist.ElementAt(i).Key);
                }
            }
            //실패한 로그인 카운트중 3분이상 경과된 항목은 삭제
            for (int i = FailedAttempt.Count - 1; i >= 0; i--)
            {
                if (FailedAttempt.ElementAt(i).Value.Time.AddMinutes(3) < DateTime.Now)
                {
                    FailedAttempt.Remove(FailedAttempt.ElementAt(i).Key);
                }
            }
        }

        /// <summary>
        /// Invalid request count processing.
        /// 대부분 소켓 연결은 자동으로 끊어지므로 특별한경우 아니면 disconnect처리 필요없음
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reUseSocket"></param>
        /// <param name="countFail"></param>
        private void RemoveClient(cltSession client, bool reUseSocket, bool countFail)
        {
            if (countFail)
                Task.Factory.StartNew(() => CountFailedAttempts(client.IPadress));
            if (!reUseSocket)
                client.TcpClient.Client.Disconnect(reUseSocket);
        }

        /// <summary>
        /// In-game user account validity check: Expiration User enforcement logout.
        /// 게임 접속상태에서 계정 유효기한이 지난 사용자를 로그아웃 처리
        /// </summary>
        private void CheckExpiredAccount(object sender)
        {
            if (g_Config.isExpiration)
            {
                for (int i = g_Config.LoggedInList.Count - 1; i >= 0; i--)
                {
                    string strID = g_Config.LoggedInList.ElementAt(i).Key;
                    if (DateTime.Now >= Acc_DB.getExpireDate(strID))
                    {
                        if (g_Config.LoggedInList[strID].PCID > g_Config.za_Uid.MinValue)
                        {
                            zaSession zAgent = g_Config.ZoneAgentList[g_Config.LoggedInList[strID].ServerID];
                            zAgent.Send(Packet.reqDisconnectAcc(g_Config.LoggedInList[strID].PCID, strID));
                            PrintLogs(string.Format("request userdrop {0} {1} to zoneagent", g_Config.LoggedInList[strID].PCID, strID));
                        }
                        PrintLogs(string.Format("ExpiredAccount.destroy {0}", strID));
                        g_Config.LoggedInList.Remove(strID);
                    }
                    Thread.Sleep(200);
                }
            }
        }
    }
}