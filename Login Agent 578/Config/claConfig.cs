using System;
using Login_Agent_578.ClientSession;

namespace Login_Agent_578.Config
{
    internal delegate void EventMsgPrintStr(String msg);
    internal delegate void EventMsgPrintUint(UInt32 msg);
    internal delegate void EventMsg(zaSession session);
    internal delegate void EventMsgClt(cltSession session);
    internal delegate void EventMsgNewPacket(String action, Byte[] buffer, String ip, ClientVer ver);

    internal enum ClientVer
    {
        undefined = 0x00,
        v219 = 0x38,
        v562 = 0x34,
        v578 = 0x40
    }

    /// <summary>
    /// generate a UID for la.
    /// </summary>
    internal class LAUID
    {
        private uint start;
        private uint uid;

        internal LAUID(uint StartValue)
        {
            this.start = StartValue;
            this.uid = this.start;
        }

        internal uint Uid
        {
            get
            {
                try
                {
                    return checked(++this.uid);
                }
                catch
                {
                    this.uid = this.start;
                    return this.uid;
                }
            }
        }
    }
    /// <summary>
    /// generate a UID for za.
    /// </summary>
    internal class ZAUID
    {
        private uint start;
        private uint uid;

        internal uint dspUid
        {
            get
            {
                return this.uid;
            }
        }

        internal uint Uid
        {
            get
            {
                try
                {
                    return checked(++this.uid);
                }
                catch
                {
                    this.uid = this.start;
                    return this.uid;
                }
            }
            set
            {
                this.uid = this.uid < value ? value : this.uid;
            }
        }

        internal uint StartValue
        {
            set
            {
                this.start = this.MinValue < value ? value : 345856;
                this.uid = this.start;
            }
        }

        internal uint MinValue
        {
            get
            {
                return 65535;
            }
        }
    }

    internal class ACCDB
    {
        internal ACCDB()
        {
            Column = new string[4];
        }
        internal string ConnInfo { get; set; }
        internal string Table { get; set; }
        internal string[] Column { get; set; }
    }

    internal class LoginUser
    {
        internal UInt32 PCID { get; set; }
        internal Int16 ServerID { get; set; }

        internal LoginUser(UInt32 uid, Int16 sid)
        {
            this.PCID = uid;
            this.ServerID = sid;
        }
    }

    internal class WrongTry
    {
        internal int Count { get; set; }
        internal DateTime Time { get; set; }

        internal WrongTry()
        {
            this.Count = 1;
            this.Time = DateTime.Now;
        }
    }

    internal class AccountInfo
    {
        internal AccountInfo()
        {
            id = string.Empty;
            pwd = string.Empty;
            status = string.Empty;
            expDate = DateTime.Now;
        }
        internal string id { get; set; }
        internal string pwd { get; set; }
        internal string status { get; set; }
        internal DateTime expDate { get; set; }
    }
}
