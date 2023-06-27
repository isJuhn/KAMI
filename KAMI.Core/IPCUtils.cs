using System;
using static KAMI.Core.PineIPC;

namespace KAMI.Core
{
    public static class IPCUtils
    {
        public static IPCStatus Error;
        public static float ReadFloat(IntPtr ipc, uint address)
        {
            uint value = (uint)Read(ipc, address, IPCCommand.MsgRead32);
            Error = GetError(ipc);
            byte[] floatBytes = BitConverter.GetBytes(value);
            return BitConverter.ToSingle(floatBytes, 0);
        }

        public static void WriteFloat(IntPtr ipc, uint address, float value)
        {
            byte[] floatBytes = BitConverter.GetBytes(value);
            uint uintValue = BitConverter.ToUInt32(floatBytes);
            Write(ipc, address, uintValue, IPCCommand.MsgWrite32);
            Error = GetError(ipc);
        }

        public static uint ReadU32(IntPtr ipc, uint address)
        {
            uint value = (uint)Read(ipc, address, IPCCommand.MsgRead32);
            Error = GetError(ipc);
            return value;
        }

        public static void WriteU32(IntPtr ipc, uint address, uint value)
        {
            Write(ipc, address, value, IPCCommand.MsgWrite32);
            Error = GetError(ipc);
        }

        public static ushort ReadU16(IntPtr ipc, uint address)
        {
            ushort value = (ushort)Read(ipc, address, IPCCommand.MsgRead16);
            Error = GetError(ipc);
            return value;
        }

        public static void WriteU16(IntPtr ipc, uint address, ushort value)
        {
            Write(ipc, address, value, IPCCommand.MsgWrite16);
            Error = GetError(ipc);
        }
    }
}
