using Login_Agent_578.ClientSession;
using Login_Agent_578.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Login_Agent_578
{
    internal class gConfig
    {
        #region ini file access
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        private static UInt32 GetIni2UInt32(string Section, string Key, string iniPath, UInt32 value = 0)
        {
            UInt32 numValue;
            if (UInt32.TryParse(GetIniValue(Section, Key, iniPath, value.ToString()), out numValue))
                return numValue;
            else
                return value;
        }

        private static Int32 GetIni2Int32(string Section, string Key, string iniPath, Int32 value = 0)
        {
            Int32 numValue;
            if (Int32.TryParse(GetIniValue(Section, Key, iniPath, value.ToString()), out numValue))
                return numValue;
            else
                return value;
        }

        private static Byte GetIni2Byte(string Section, string Key, string iniPath, Byte value = 0)
        {
            Byte numValue;
            if (Byte.TryParse(GetIniValue(Section, Key, iniPath, value.ToString()), out numValue))
                return numValue;
            else
                return value;
        }

        private static IPAddress GetIni2IP(string Section, string Key, string iniPath, String value = "127.0.0.1")
        {
            IPAddress ipAdress;
            if (IPAddress.TryParse(GetIniValue(Section, Key, iniPath, value.ToString()), out ipAdress))
                return ipAdress;
            else
                return IPAddress.Parse(value);
        }

        private static string GetIniValue(string Section, string Key, string iniPath, string defaultValue = "")
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);
            return (temp.ToString().Trim() != "") ? temp.ToString().Trim() : defaultValue;
        }
        #endregion

        private static gConfig g_Config;

        //공통으로 사용하는 변수
        internal List<cltSession> ClientsList;
        internal Dictionary<String, LoginUser> LoggedInList;
        internal Dictionary<Int16, zaSession> ZoneAgentList;
        internal Int32 inZoneAgent;

        //ini에서 읽어오는 설정값들...
        internal Boolean isExpiration;
        internal Boolean isMaintainance;
        internal Int32 hashType;
        internal List<String> AllowedIP;
        internal List<String> BannedIP;
        internal List<String> AdminIP;
        internal Dictionary<String, String> RejectDB;
        internal Dictionary<Byte, String> ServerList;
        internal Int32 ZAPort;
        internal Int32 LAPort;
        internal String[] QueryString;
        internal String Msg_Maintainance;
        internal String Msg_Banned;
        internal String Msg_Incorrect;
        internal String Msg_Duplication;
        internal String Msg_Blacklist;
        internal String Msg_Expired;
        internal String Msg_ExpireInfo;
        internal String SqlConn;
        internal Int32 cHighVer;
        internal Int32 cLowVer;
        internal Int32 WorkMode;
        internal ZAUID za_Uid;
        internal ACCDB DBinfo;

        internal static gConfig getInstance()
        {
            if (g_Config == null)
            {
                g_Config = new gConfig();
            }
            return g_Config;
        }

        internal bool Init(String SvrInfo)
        {
            try
            {
                LoggedInList = new Dictionary<string, LoginUser>();
                ZoneAgentList = new Dictionary<short, zaSession>();
                ClientsList = new List<cltSession>();
                inZoneAgent = 0;

                QueryString = new String[2];
                za_Uid = new ZAUID();
                RejectDB = new Dictionary<String, String>();
                AllowedIP = new List<String>();
                BannedIP = new List<String>();
                AdminIP = new List<String>();

                isMaintainance = false;
                isExpiration = (GetIni2Int32("STARTUP", "EXPIRATION", SvrInfo, 0) > 0) ? true : false;

                cHighVer = GetIni2Int32("VERSIONINFO", "HIGH", SvrInfo, 219);
                cLowVer = GetIni2Int32("VERSIONINFO", "LOW", SvrInfo, 217);
                WorkMode = GetIni2Int32("STARTUP", "MODE", SvrInfo, 0);
                hashType = GetIni2Int32("STARTUP", "HASHTYPE", SvrInfo, 0);
                hashType = 2 < hashType || 0 > hashType ? 2 : hashType;
                LAPort = GetIni2Int32("STARTUP", "PORT", SvrInfo, 3551);
                ZAPort = GetIni2Int32("STARTUP", "ZONEAGENTLISTENPORT", SvrInfo, 3200);
                za_Uid.StartValue = GetIni2UInt32("STARTUP", "STARTID", SvrInfo, 345856);
                Msg_Maintainance = GetIniValue("STARTUP", "MSG_MAINTAINANCE", SvrInfo, "Server is down for maintainance");
                Msg_Banned = GetIniValue("STARTUP", "MSG_BANNED", SvrInfo, "Your IP has been banned from this server");
                Msg_Incorrect = GetIniValue("STARTUP", "MSG_WRONGACCOUNT", SvrInfo, "Your login information was incorrect");
                Msg_Duplication = GetIniValue("STARTUP", "MSG_DUPLICATEUSER", SvrInfo, "Your account is already connected to another user");
                Msg_Blacklist = GetIniValue("STARTUP", "MSG_MANYFAILED", SvrInfo, "Your ip has too many failed attempts, Please try again in 10 minutes");
                Msg_Expired = GetIniValue("STARTUP", "MSG_EXPIRED", SvrInfo, "Your account is expired. Please renew your membership");
                Msg_ExpireInfo = GetIniValue("STARTUP", "MSG_EXPIREINFO", SvrInfo, "Your account expires after {0} days {1} hours {2} minutes");

                string bdip = GetIniValue("LOGIN_DB", "IP", SvrInfo, "127.0.0.1");
                string bdport = GetIniValue("LOGIN_DB", "PORT", SvrInfo, "1433");
                string bdname = GetIniValue("LOGIN_DB", "CATALOG", SvrInfo, "ASD");
                string bdid = GetIniValue("LOGIN_DB", "ID", SvrInfo, "");
                string bdpwd = GetIniValue("LOGIN_DB", "PWD", SvrInfo, "");
                SqlConn = string.Format("Data Source={0},{1};Initial Catalog={2};User ID={3};Password={4};Timeout=1", bdip, bdport, bdname, bdid, bdpwd);
                
                DBinfo = new ACCDB();
                DBinfo.ConnInfo = string.Format("{0}@{1}:{2}", bdname, bdip, bdport);
                DBinfo.Table = GetIniValue("LOGIN_DB", "TABLE", SvrInfo, "account");
                DBinfo.Column[0] = GetIniValue("LOGIN_DB", "COLUMN_ID", SvrInfo, "c_id");
                DBinfo.Column[1] = GetIniValue("LOGIN_DB", "COLUMN_PWD", SvrInfo, "c_headera");
                DBinfo.Column[2] = GetIniValue("LOGIN_DB", "COLUMN_STATUS", SvrInfo, "c_status");
                DBinfo.Column[3] = GetIniValue("LOGIN_DB", "COLUMN_EXPIRED", SvrInfo, "d_udate");

                QueryString[0] = string.Format("select {0}, {1}, {2}, {3} from {4} where convert(varbinary, {0}", DBinfo.Column[0], DBinfo.Column[1], DBinfo.Column[2], DBinfo.Column[3], DBinfo.Table) + ") = convert(varbinary, '{0}')";
                QueryString[1] = string.Format("select {0} from {1} where convert(varbinary, {2}", DBinfo.Column[3], DBinfo.Table, DBinfo.Column[0]) + ") = convert(varbinary, '{0}')";
                
                int listCount = GetIni2Int32("SERVER_GROUP", "COUNT", SvrInfo, 1);
                ServerList = new Dictionary<byte, string>();
                for (int i = 0; i < listCount; i++)
                {
                    try
                    {
                        //없는 번호의 서버리스트에는 -1을 할당해 일부러 오류 발생시켜서 넘어가기
                        byte sNum = byte.Parse(GetIniValue("SERVER_GROUP", "ID" + i, SvrInfo, "-1"));
                        string sName = GetIniValue("SERVER_GROUP", "NAME" + i, SvrInfo, "Server");
                        ServerList.Add(sNum, sName);
                    }
                    catch
                    {
                        continue;
                    }
                }
                Main.PrintLogMsg(string.Format("Loaded {0}", SvrInfo));
                Main.PrintUID(za_Uid.dspUid);
            }
            catch (Exception)
            {
                Main.PrintLogMsg("Load iniFile Error");
                return false;
            }

            LoadAllowIP();
            LoadBanIP();
            LoadAdminIP();
            return LoadAllowSDB();
        }

        /// <summary>
        /// 계정 status별 로그인 가/부 설정 파일 읽기
        /// </summary>
        /// <returns></returns>
        internal bool LoadAllowSDB()
        {
            try
            {
                RejectDB.Clear();
                using (StreamReader sr = new StreamReader("AllowStatus.SDB", Encoding.Default, true))
                {
                    string readLine;
                    while ((readLine = sr.ReadLine()) != null)
                    {
                        string[] status = readLine.Split(',');
                        if (status.Length == 4 && status[1] == "FALSE")
                        {
                            RejectDB.Add(status[0].Trim(), status[2].Trim());
                        }
                    }
                }
                Main.PrintLogMsg("Loaded AllowStatus.SDB");
                return true;
            }
            catch
            {
                Main.PrintLogMsg("Load AllowStatus.SDB:Error");
                return false;
            }
        }

        /// <summary>
        /// 허용 ip 목록 읽기: BanIP에 범위를 지정하고 그 중 허용할것이 있다면 이곳에 지정
        /// </summary>
        /// <returns></returns>
        internal bool LoadAllowIP()
        {
            try
            {
                AllowedIP.Clear();
                using (StreamReader sr = new StreamReader("AllowIP.txt", true))
                {
                    string readLine;
                    while ((readLine = sr.ReadLine()) != null)
                    {
                        if (readLine.Trim().Substring(0, 1) != "#")
                            AllowedIP.Add(readLine.Trim());
                    }
                }
                Main.PrintLogMsg("Loaded AllowIP.txt");
                return true;
            }
            catch
            {
                Main.PrintLogMsg("Load AllowIP.txt:Error");
                return false;
            }
        }

        /// <summary>
        /// 차단 ip 목록 읽기
        /// </summary>
        /// <returns></returns>
        internal bool LoadBanIP()
        {
            try
            {
                BannedIP.Clear();
                using (StreamReader sr = new StreamReader("BanIP.txt", true))
                {
                    string readLine;
                    while ((readLine = sr.ReadLine()) != null)
                    {
                        if (readLine.Trim().Substring(0, 1) != "#")
                            BannedIP.Add(readLine.Trim());
                    }
                }
                Main.PrintLogMsg("Loaded BanIP.txt");
                return true;
            }
            catch
            {
                Main.PrintLogMsg("Load BanIP.txt:Error");
                return false;
            }
        }

        /// <summary>
        /// 관리자 ip 읽기
        /// </summary>
        /// <returns></returns>
        internal bool LoadAdminIP()
        {
            try
            {
                AdminIP.Clear();
                using (StreamReader sr = new StreamReader("AdminIP.txt", true))
                {
                    string readLine;
                    while ((readLine = sr.ReadLine()) != null)
                    {
                        if (readLine.Trim().Substring(0, 1) != "#")
                            AdminIP.Add(readLine.Trim());
                    }
                }
                Main.PrintLogMsg("Loaded AdminIP.txt");
                return true;
            }
            catch
            {
                Main.PrintLogMsg("Load AdminIP.txt:Error");
                return false;
            }
        }
    }
}