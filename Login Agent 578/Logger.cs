using System;
using System.IO;
using System.Text;
using Login_Agent_578.Config;

namespace Login_Agent_578
{
    internal class Logger
    {
        private static Logger g_logger;

        private Logger() { }

        internal static Logger getInstance()
        {
            if (g_logger == null)
            {
                g_logger = new Logger();
            }
            return g_logger;
        }

        internal void WriteLog(String log)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(string.Format("./log/LoginAgent_{0}.log", DateTime.Now.ToString("yyyyMMdd")), true))
                {
                    sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log));
                }
            }
            catch { }
        }

        internal void NewPacket(String act, Byte[] packet, String ip, ClientVer cVer)
        {
            try
            {
                if (!Directory.Exists("./NewPacket"))
                    Directory.CreateDirectory("./NewPacket");

                string sPacket = BitConverter.ToString(packet).Replace("-", " ");
                using (StreamWriter sw = new StreamWriter(string.Format("./NewPacket/uPacket_{0}.log", DateTime.Now.ToString("yyyy-MM-dd")), true))
                {
                    sw.WriteLine(string.Format("Action: {0}", act));
                    sw.WriteLine(string.Format("ClientVer: {0}", cVer.ToString()));
                    sw.WriteLine(string.Format("ClientIP: {0}", ip));
                    sw.WriteLine(string.Format("Text: {0}", Encoding.Default.GetString(packet)));
                    if (sPacket.Length >= 35)
                        sw.WriteLine(string.Format("Protocol: 0x{0}{1}", sPacket.Substring(33, 2), sPacket.Substring(30, 2)));
                    else
                        sw.WriteLine("Protocol: 0x----");
                    sw.WriteLine("00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
                    sw.WriteLine("-----------------------------------------------");
                    for (int i = 0; i < sPacket.Length; i++)
                    {
                        if (i > 0 && i % 48 == 0)
                            sw.Write("\r\n");
                        sw.Write(sPacket[i]);
                    }
                    sw.Write("\r\n");
                    sw.Write("\r\n");
                }
            }
            catch { }
        }
    }
}