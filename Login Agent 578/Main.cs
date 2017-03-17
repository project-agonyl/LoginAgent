using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using Login_Agent_578.Config;
using Login_Agent_578.ClientSession;
using System.Linq;

namespace Login_Agent_578
{
    public partial class Main : Form
    {
        private static Main form = null;
        private LoginAgent login_Agent;
        private LoginServer login_Server;
        private gConfig g_Config;
        private Logger g_logger;

        public Main()
        {
            InitializeComponent();
            form = this;
            g_logger = Logger.getInstance();
            g_Config = gConfig.getInstance();
        }

        internal static void PrintLogMsg(string msg)
        {
            form.UpdateLogMsg(msg);
        }

        internal static void PrintUID(uint uid)
        {
            form.UpdateUidNumber(uid);
        }

        private void UpdateStatus(zaSession session)
        {
            form.UpdateZaStatus();
            form.UpdateCurrentUsers();
        }

        /// <summary>
        /// 로그 기록
        /// </summary>
        /// <param name="logs"></param>
        private void WriteLogs(String logs)
        {
            Task.Factory.StartNew(() => g_logger.WriteLog(logs));
        }

        private void WriteNewPacket(String action, Byte[] buffer, String ip, ClientVer ver)
        {
            Task.Factory.StartNew(() => g_logger.NewPacket(action, buffer, ip, ver));
        }

        private void CloseServer()
        {
            if (login_Server != null)
                login_Server = null;
            if (login_Agent != null)
                login_Agent = null;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //로그 디렉토리 없으면 생성
            if (!Directory.Exists("./log"))
                Directory.CreateDirectory("./log");

            if (!File.Exists("Svrinfo.ini"))
                MessageBox.Show("Svrinfo.ini not found!", "Login Agent", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (g_Config.Init(@".\SvrInfo.ini"))
            {
                try
                {
                    lbPortStatus.Text = g_Config.LAPort.ToString();

                    login_Agent = new LoginAgent();
                    login_Agent.PrintLogs += UpdateLogMsg;
                    login_Agent.WriteLogs += WriteLogs;
                    login_Agent.PrintPCID += UpdateUidNumber;
                    login_Agent.WriteNewPacket += WriteNewPacket;

                    login_Server = new LoginServer();
                    login_Server.PrintLogs += UpdateLogMsg;
                    login_Server.WriteLogs += WriteLogs;
                    login_Server.PrintPCID += UpdateUidNumber;
                    login_Server.UpdateStatus += UpdateStatus;

                    //if (new LoginAgent(this).Start() && new LoginServer(this).Start())
                    if (login_Server.Start() && login_Agent.Start())
                    {
                        //DB연결 확인: getdate()
                        SqlClient dbInfo = new SqlClient();
                        if (dbInfo.isConnectable())
                        {
                            UpdateLogMsg(string.Format("{0}:Connected", g_Config.DBinfo.ConnInfo));

                            if (dbInfo.isExistsTable(g_Config.DBinfo.Table))
                            {
                                bool isExists = true;
                                for (int i = 0; i < g_Config.DBinfo.Column.Length; i++)
                                {
                                    if (!dbInfo.isExistsColumn(g_Config.DBinfo.Column[i], g_Config.DBinfo.Table))
                                    {
                                        UpdateLogMsg(string.Format("There is no '{0}' column in the Table", g_Config.DBinfo.Column[i]));
                                        isExists = false;
                                    }
                                }

                                if(isExists)
                                {
                                    UpdateLogMsg("===== LOGIN AGENT START =====");
                                }
                                else
                                {
                                    UpdateLogMsg("===== LOGIN AGENT START ERROR =====");
                                    CloseServer();
                                }
                            }
                            else
                            {
                                UpdateLogMsg(string.Format("There is no '{0}' table in the DB", g_Config.DBinfo.Table));
                                UpdateLogMsg("===== LOGIN AGENT START ERROR =====");
                                CloseServer();
                            }
                        }
                        else
                        {
                            UpdateLogMsg(string.Format("{0}:Disconnected", g_Config.DBinfo.ConnInfo));
                            UpdateLogMsg("===== LOGIN AGENT START ERROR =====");
                            CloseServer();
                        }
                        dbInfo = null;
                    }
                    else
                    {
                        UpdateLogMsg("===== LOGIN AGENT START ERROR =====");
                        CloseServer();
                    }
                }
                catch (Exception ex)
                {
                    UpdateLogMsg("===== LOGIN AGENT START ERROR =====");
                    WriteLogs(string.Format("Agent Start error: {0}" + ex));
                }
            }
            else
            {
                UpdateLogMsg("=== Initialization Error ===");
            }
        }

        private void maintainanceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (maintainanceCheckBox.Checked)
                g_Config.isMaintainance = true;
            else
                g_Config.isMaintainance = false;
        }
        /// <summary>
        /// 로그 표시, 1000줄까지만 표현 하도록 제한
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateLogMsg(string msg)
        {
            try
            {
                this.Invoke(new Action(
                            delegate()
                            {
                                const int nLimitPrintLines = 1000;
                                if (LaMsgLog.Lines.Length >= nLimitPrintLines)
                                {
                                    List<string> tempLines = new List<string>(LaMsgLog.Lines);
                                    tempLines.RemoveRange(0, nLimitPrintLines / 2);
                                    LaMsgLog.Lines = tempLines.ToArray();
                                }
                                LaMsgLog.AppendText(string.Format("{0}{1}", msg, Environment.NewLine));
                                LaMsgLog.ScrollToCaret();
                            }));
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("FrmMain.UpdateLogMsg:{0}{1}", Environment.NewLine, ex));
            }
        }
        /// <summary>
        /// 연결중인 ZA수 표시
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateZaStatus()
        {
            try
            {
                this.Invoke(new Action(
                            delegate()
                            {
                                lbZaStatus.Text = g_Config.ZoneAgentList.Count.ToString();
                            }));
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("FrmMain.UpdateZaStatus:{0}{1}", Environment.NewLine, ex));
            }
        }
        /// <summary>
        /// 현재 접속자 표시, 캐릭터 선택 화면으로 넘어가야 유저수에 카운트됨
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateCurrentUsers()
        {
            try
            {
                this.Invoke(new Action(
                            delegate()
                            {
                                int ingameUsers = g_Config.LoggedInList.Count(x => x.Value.PCID > g_Config.za_Uid.MinValue);
                                int inlaUsers = g_Config.ClientsList.Count;
                                int inzaUsers = g_Config.inZoneAgent;
                                lbUserCount.Text = string.Format("{0}(L:{1}/Z:{2})", ingameUsers, inlaUsers - inzaUsers, inzaUsers);
                            }));
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("FrmMain.UpdateCurrentUsers:{0}{1}", Environment.NewLine, ex));
            }
        }
        /// <summary>
        /// 현재 UID표시
        /// </summary>
        /// <param name="uid"></param>
        private void UpdateUidNumber(uint uid)
        {
            try
            {
                this.Invoke(new Action(
                            delegate()
                            {
                                lbUidStatus.Text = uid.ToString();
                            }));
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("FrmMain.UpdateUidNumber:{0}{1}", Environment.NewLine, ex));
            }
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            g_Config.LoadAllowIP();
            g_Config.LoadBanIP();
            g_Config.LoadAdminIP();
            g_Config.LoadAllowSDB();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to close?", "LoginAgent", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                e.Cancel = true;
            CloseServer();
        }
    }
}