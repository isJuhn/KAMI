using System;
using static KAMI.PCSX2IPC;

namespace KAMI
{
    public static class IPCUtils
    {
        public static IPCStatus Error;
        public static float ReadFloat(IntPtr ipc, uint address)
        {
            uint value = (uint)Read(ipc, address, IPCCommand.MsgRead32, false);
            Error = GetError(ipc);
            byte[] floatBytes = BitConverter.GetBytes(value);
            return BitConverter.ToSingle(floatBytes, 0);
        }

        public static void WriteFloat(IntPtr ipc, uint address, float value)
        {
            byte[] floatBytes = BitConverter.GetBytes(value);
            uint uintValue = BitConverter.ToUInt32(floatBytes);
            Write(ipc, address, uintValue, IPCCommand.MsgWrite32, false);
        }
    }
}
