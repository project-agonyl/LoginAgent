using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Login_Agent_578.ClientSession;
using Login_Agent_578.Config;

namespace Login_Agent_578
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_HEAD_NO_PROTOCOL : Marshalling
    {
        internal MSG_HEAD_NO_PROTOCOL() { }
        internal MSG_HEAD_NO_PROTOCOL(uint uid, byte ctrl, byte cmd)
        {
            dwPCID = uid;
            byCtrl = ctrl;
            byCmd = cmd;
        }
        internal UInt32 dwSize;
        internal UInt32 dwPCID;
        internal Byte byCtrl;
        internal Byte byCmd;
    }
    /// <summary>
    /// ZA->LS: ZA information packet
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_ZA2LS_CONNECT : Marshalling
    {
        internal MSG_ZA2LS_CONNECT()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x02;
            MsgHeader.byCmd = 0xE0;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        internal Byte byServerID;
        internal Byte byAgentID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x10)]
        internal String szIPAdress;
        internal Int32 dwPort;
    }
    /// <summary>
    /// ZA->LS: alive packet
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_ZA2LS_PLAYER_COUNT : Marshalling
    {
        internal MSG_ZA2LS_PLAYER_COUNT()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x02;
            MsgHeader.byCmd = 0xE1;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        internal Int32 dwPlayerCount;
        internal Byte byZsCount1;
        internal Byte byZsCount2;
    }
    /// <summary>
    /// ZA->LS: user logout
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_ZA2LS_ACC_LOGOUT : Marshalling
    {
        internal MSG_ZA2LS_ACC_LOGOUT()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x02;
            MsgHeader.byCmd = 0xE2;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        internal Byte byReason;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x15)]
        internal String szAccount;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x09)]
        internal String szDate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x07)]
        internal String szTime;
    }
    /// <summary>
    /// ZA->LS: prepared user login
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_ZA2LS_PREPARED_ACC_LOGIN : Marshalling
    {
        internal MSG_ZA2LS_PREPARED_ACC_LOGIN()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x02;
            MsgHeader.byCmd = 0xE3;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x15)]
        internal String szAccount;
    }
    /// <summary>
    /// ZA->LS: login user recover packet
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_ZA2LS_LOGIN_USER_LIST : Marshalling
    {
        internal MSG_ZA2LS_LOGIN_USER_LIST()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x02;
            MsgHeader.byCmd = 0xE4;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x15)]
        internal String szUserAccount;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x10)]
        internal String szUserIP;
        internal UInt32 dwUnknown;
    }

    /// <summary>
    /// encrypted header, only v219
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_HEADER_219 : Marshalling
    {
        internal MSG_HEADER_219(ref byte[] buffer, byte count)
        {
            byRand = (byte)(new Random()).Next(0x20, 0x2F);
            wLength = (ushort)(buffer.Length + 7);
            int a = wLength, b = wLength;
            a = ((((((a >> 6) & 0xf0) | (a & 0xc00f)) >> 2) | (a & 0x03c0)) >> 2);
            b = ((((b & 0x3c) << 2) | (b & 0x03)) << 8);
            wEncryptedLength = (ushort)((a | b) ^ 0x01);
            byCount = count;
            dwCrc32 = new Crc32().ComputeHash(buffer).Reverse().ToArray();
        }
        internal Byte byRand;
        internal UInt16 wEncryptedLength;
        internal UInt16 wLength;
        internal Byte byCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
        internal Byte[] dwCrc32;
    }

    /// <summary>
    /// LA->CL: say msg
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_LA2CL_SAY : Marshalling
    {
        internal MSG_LA2CL_SAY()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x01;
            MsgHeader.byCmd = 0xE0;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        internal Byte byType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x51)]
        internal String szWords;
    }

    /// <summary>
    /// struct: server info
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class SVR_INFO : Marshalling
    {
        internal void Clear()
        {
            bySvrID = 0;
            szSvrName = string.Empty;
            szSvrStatus = "OFFLINE";
        }
        internal SVR_INFO()
        {
            szSvrStatus = "OFFLINE";
        }
        internal Byte bySvrID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x11)]
        internal String szSvrName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x51)]
        internal String szSvrStatus;
    }

    /// <summary>
    /// LA->CL: ZA information
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_LA2CL_ZA_INFO : Marshalling
    {
        internal MSG_LA2CL_ZA_INFO()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x01;
            MsgHeader.byCmd = 0xE2;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        internal UInt32 dwPCID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x10)]
        internal String szZAIP;
        internal Int32 dwZAPort;
    }

    /// <summary>
    /// LA->CL: Invalid client version. only 219 Client
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_LA2CL_INVALID_VERSION : Marshalling
    {
        internal MSG_LA2CL_INVALID_VERSION()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x01;
            MsgHeader.byCmd = 0xE4;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        internal Int32 dwReqVersion;
    }

    /// <summary>
    /// LA->ZA: prepared Account
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_LA2ZA_PREPARED_ACC : Marshalling
    {
        internal MSG_LA2ZA_PREPARED_ACC()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x01;
            MsgHeader.byCmd = 0xE1;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x15)]
        internal String szAccount;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x09)]
        internal String szUnknown;
    }

    /// <summary>
    /// LA->ZA: request disconnect(duplicate login)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class MSG_LA2ZA_REQ_DISCONNECT : Marshalling
    {
        internal MSG_LA2ZA_REQ_DISCONNECT()
        {
            MsgHeader = new MSG_HEAD_NO_PROTOCOL();
            MsgHeader.dwSize = GetSize();
            MsgHeader.byCtrl = 0x01;
            MsgHeader.byCmd = 0xE3;
        }
        internal MSG_HEAD_NO_PROTOCOL MsgHeader;
        internal Byte byReason;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x15)]
        internal String szAccount;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x10)]
        internal String szUnknown;
    }


    internal static class Packet
    {
        //Combine ByteArray
        private static byte[] CombineBytes(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private static byte[] CombineBytes(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length, third.Length);
            return ret;
        }

        //String to ByteArray
        private static byte[] String2Bytes(string str)
        {
            str += '\0';
            return Encoding.Default.GetBytes(str);
        }

        private static byte[] String2Bytes(string str, int length)
        {
            byte[] bytearray = new byte[length];
            byte[] stringarray = Encoding.Default.GetBytes(str);
            if (stringarray.Length < length)
            {
                stringarray.CopyTo(bytearray, 0);
            }
            else
            {
                Array.Copy(stringarray, bytearray, length - 1);
            }
            return bytearray;
        }

        internal static string Bytes2String(byte[] packet, int offset, int count = 21)
        {
            string str = Encoding.Default.GetString(packet, offset, count).TrimEnd('\0');
            string[] strArray = str.Split('\0');
            return strArray[0];
        }

        internal static void TrimPacket(ref byte[] buffer, int offset)
        {
            if (offset != 0)
                Array.Copy(buffer, offset, buffer, 0, BitConverter.ToInt32(buffer, offset));
            Array.Resize(ref buffer, BitConverter.ToInt32(buffer, 0));
        }

        internal static byte[] SplitPackets(byte[] buffer, int length, out List<byte[]> packetList, byte[] front)
        {
            packetList = new List<byte[]>();
            byte[] packet;
            int offset = 0;
            int packetSize;
            byte[] rest = null;

                //처리되지않은 데이터는 rest로 넘기고 다음 작업때 front로 받아서 처리
            if (front != null && front.Length > 0)
            {
                Array.Resize(ref buffer, length);
                buffer = CombineBytes(front, buffer);
                length += front.Length;
            }

            while (length > offset)
            {
                //ToInt32 수행시 4바이트 미만일경우 ArgumentException
                if (length - offset < 4)
                {
                    rest = new byte[length - offset];
                    Array.Copy(buffer, offset, rest, 0, length - offset);
                    break;
                }
                packetSize = BitConverter.ToInt32(buffer, offset);
                if (offset + packetSize > length)
                {
                    rest = new byte[length - offset];
                    Array.Copy(buffer, offset, rest, 0, length - offset);
                    break;
                }
                if (packetSize < 10 || packetSize > 2048)
                {
                    break;
                }
                packet = new byte[packetSize];
                Array.Copy(buffer, offset, packet, 0, packetSize);
                packetList.Add(packet);
                offset += packetSize;
            }
            return rest;
        }

        /// <summary>
        /// 219클라이언트용 해더 추가
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="pCount"></param>
        private static void AddHeaderV219(ref byte[] buffer, byte pCount)
        {
            MSG_HEADER_219 Header_219 = new MSG_HEADER_219(ref buffer, 0);
            buffer = CombineBytes(Header_219.GetBuffer(), buffer);
        }

        /// <summary>
        /// (오류)메시지 생성, Type: 00-비번, 01-계정, 02-중복접속or공란입력, 03-기타(in AllowStatus.SDB)
        /// ASCII 0x0A: New Line
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="uid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static byte[] SayMessage(string msg, uint uid, byte type, ClientVer ver)
        {
            MSG_LA2CL_SAY say = new MSG_LA2CL_SAY();
            say.MsgHeader.dwPCID = uid;
            say.byType = type;
            say.szWords = Packet.bSubString(msg, 0x51);
            byte[] packet = say.GetBuffer();

            if (ver == ClientVer.v219)
                AddHeaderV219(ref packet, 0);
            return packet;
        }

        internal static byte[] CreateSvrList(uint uid, ClientVer ver, Dictionary<Int16, zaSession> zAgent, Dictionary<Byte, String> ServerList, string msg)
        {
            byte[] packet = new MSG_HEAD_NO_PROTOCOL(uid, 0x01, 0xE1).GetBuffer();
            if (ver == ClientVer.v578)
                packet = CombineBytes(packet, new byte[0x15]);
            packet = CombineBytes(packet, BitConverter.GetBytes((short)ServerList.Count));
            SVR_INFO sInfo = new SVR_INFO();
            for (int i = 0; i < ServerList.Count; i++)
            {
                sInfo.Clear();
                sInfo.bySvrID = ServerList.ElementAt(i).Key;
                sInfo.szSvrName = ServerList.ElementAt(i).Value;
                if (zAgent.ContainsKey(ServerList.ElementAt(i).Key))
                    sInfo.szSvrStatus = "ONLINE";
                packet = CombineBytes(packet, sInfo.GetBuffer());
            }
            BitConverter.GetBytes(packet.Length).CopyTo(packet, 0);
            if (ver == ClientVer.v219)
                AddHeaderV219(ref packet, 0);
            //메시지가 있는경우 메시지 패킷을 앞쪽에 추가한다 : 메시지와 서버 리스트를 따로 보내면 생각대로 작동 안함
            if (msg != string.Empty)
                packet = CombineBytes(SayMessage(msg, uid, 3, ver), packet);
            return packet;
        }

        internal static byte[] ZoneAgentInfo(uint uid, uint zaUid, string zaIp, int zaPort, ClientVer ver)
        {
            MSG_LA2CL_ZA_INFO za = new MSG_LA2CL_ZA_INFO();
            za.MsgHeader.dwPCID = uid;
            za.dwPCID = zaUid;
            za.szZAIP = zaIp;
            za.dwZAPort = zaPort;
            byte[] packet = za.GetBuffer();

            if (ver == ClientVer.v219)
                AddHeaderV219(ref packet, 1);
            return packet;
        }

        internal static byte[] InvalidClientVersion(ClientVer ver)
        {
            MSG_LA2CL_INVALID_VERSION mVer = new MSG_LA2CL_INVALID_VERSION();
            //mVer.dwReqVersion = Config.cHighVer;
            mVer.dwReqVersion = 0; //필요한 버전을 패킷에 실어보낸다. 단, 패킷을 보면 필요한 버전을 알수 있으므로 0으로 보내버린다.
            byte[] packet = mVer.GetBuffer();

            if (ver == ClientVer.v219)
                AddHeaderV219(ref packet, 0);
            return packet;
        }

        internal static byte[] PreparedAccount(uint zaUid, string account)
        {
            MSG_LA2ZA_PREPARED_ACC acc = new MSG_LA2ZA_PREPARED_ACC();
            acc.MsgHeader.dwPCID = zaUid;
            acc.szAccount = account;
            return acc.GetBuffer();
        }

        internal static byte[] reqDisconnectAcc(uint zaUid, string account)
        {
            MSG_LA2ZA_REQ_DISCONNECT req = new MSG_LA2ZA_REQ_DISCONNECT();
            req.MsgHeader.dwPCID = zaUid;
            req.byReason = 0x02; //duplication
            req.szAccount = account;
            return req.GetBuffer();
        }

        internal static string bSubString(string str, int bytes)
        {
            byte[] bytearray = new byte[bytes];
            byte[] stringarray = System.Text.Encoding.Default.GetBytes(str);
            if (stringarray.Length < bytes)
                stringarray.CopyTo(bytearray, 0);
            else
                Array.Copy(stringarray, bytearray, bytes - 1);
            return Encoding.Default.GetString(bytearray).TrimEnd('\0');
        }
    }
}