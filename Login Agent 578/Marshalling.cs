using System;
using System.Runtime.InteropServices;

namespace Login_Agent_578
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class Marshalling
    {
        /// <summary>
        /// 바이트 배열 -> 구조체
        /// </summary>
        /// <param name="pbyte"></param>
        internal void SetBuffer(byte[] pbyte)
        {
            unsafe	// c#에서는 포인터연산을 통한 메모리복사를 안전하지 않는 코드로 판단, unsafe블록안에 감싸줘야함
            {
                fixed (byte* fixed_buffer = pbyte)
                {
                    Marshal.PtrToStructure((IntPtr)fixed_buffer, this);
                }
            }
        }
        /// <summary>
        /// 구조체 -> 바이트 배열
        /// </summary>
        /// <returns></returns>
        internal byte[] GetBuffer()
        {
            byte[] rvalue = new byte[GetSize()];
            PutBuffer(rvalue);
            return rvalue;
        }
        // 보낼패킷 구조체를 byte에 복사함
        private void PutBuffer(byte[] pbyte)
        {
            int mycount = Marshal.SizeOf(this);
            unsafe
            {
                fixed (byte* fixed_buffer = pbyte)
                {
                    Marshal.StructureToPtr(this, (IntPtr)fixed_buffer, true);
                    for (int i = 0; i < mycount; i++)
                    {
                        pbyte[i] = fixed_buffer[i];
                    }
                }
            }
        }
        /// <summary>
        /// 구조체의 사이즈 리턴
        /// </summary>
        /// <returns></returns>
        internal uint GetSize()
        {
            return (uint)Marshal.SizeOf(this);
        }
    }
}