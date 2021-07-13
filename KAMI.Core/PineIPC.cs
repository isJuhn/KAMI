using System;
using System.Runtime.InteropServices;

namespace KAMI.Core
{
    public static class PineIPC
    {
#if Windows
        private const string libipc = "pine_c.dll";
#elif Linux
        private const string libipc = "libpine_c.so";
#endif

        [DllImport(libipc)]
        static extern IntPtr pine_rpcs3_new();

        [DllImport(libipc)]
        static extern IntPtr pine_pcsx2_new();

        [DllImport(libipc)]
        static extern void pine_initialize_batch(IntPtr v);

        [DllImport(libipc)]
        static extern Int32 pine_finalize_batch(IntPtr v);

        [DllImport(libipc)]
        static extern void pine_send_command(IntPtr v, Int32 cmd);

        [DllImport(libipc)]
        static extern UInt64 pine_read(IntPtr v, UInt32 address, IPCCommand msg, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pine_version(IntPtr v, bool batch);

        [DllImport(libipc)]
        static extern EmuStatus pine_status(IntPtr v, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pine_getgametitle(IntPtr v, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pine_getgameid(IntPtr v, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pine_getgameuuid(IntPtr v, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pine_getgameversion(IntPtr v, bool batch);

        [DllImport(libipc)]
        static extern void pine_write(IntPtr v, UInt32 address, UInt64 val, IPCCommand msg, bool batch);

        [DllImport(libipc)]
        static extern void pine_rpcs3_delete(IntPtr v);

        [DllImport(libipc)]
        static extern void pine_pcsx2_delete(IntPtr v);

        [DllImport(libipc)]
        static extern void pine_free_batch_command(Int32 cmd);

        [DllImport(libipc)]
        static extern IPCStatus pine_get_error(IntPtr v);


        /// <summary>
        /// IPC Command messages opcodes.
        /// </summary>
        /// <remarks>
        /// A list of possible operations possible by the IPC.
        /// Each one of them is what we call an "opcode" and is the first
        /// byte sent by the IPC to differentiate between commands.
        /// </remarks>
        public enum IPCCommand : Byte
        {
            MsgRead8 = 0,           /**< Read 8 bit value to memory. */
            MsgRead16 = 1,          /**< Read 16 bit value to memory. */
            MsgRead32 = 2,          /**< Read 32 bit value to memory. */
            MsgRead64 = 3,          /**< Read 64 bit value to memory. */
            MsgWrite8 = 4,          /**< Write 8 bit value to memory. */
            MsgWrite16 = 5,         /**< Write 16 bit value to memory. */
            MsgWrite32 = 6,         /**< Write 32 bit value to memory. */
            MsgWrite64 = 7,         /**< Write 64 bit value to memory. */
            MsgVersion = 8,         /**< Returns Emulator version. */
            MsgSaveState = 9,       /**< Saves a savestate. */
            MsgLoadState = 0xA,     /**< Loads a savestate. */
            MsgTitle = 0xB,         /**< Returns the game title. */
            MsgID = 0xC,            /**< Returns the game ID. */
            MsgUUID = 0xD,          /**< Returns the game UUID. */
            MsgGameVersion = 0xE,   /**< Returns the game verion. */
            MsgStatus = 0xF,        /**< Returns the emulator status. */
            MsgUnimplemented = 0xFF /**< Unimplemented IPC message. */
        };

        /// <summary>
        /// Result code of the IPC operation.
        /// </summary>
        /// <remarks>
        /// A list of result codes that should be returned, or thrown, depending
        /// on the state of the result of an IPC command.
        /// </remarks>
        public enum IPCStatus : UInt32
        {
            Success = 0,       /**< IPC command successfully completed. */
            Fail = 1,          /**< IPC command failed to execute. */
            OutOfMemory = 2,   /**< IPC command too big to send. */
            NoConnection = 3,  /**< Cannot connect to the IPC socket. */
            Unimplemented = 4, /**< Unimplemented IPC command. */
            Unknown = 5        /**< Unknown status. */
        };

        /// <summary>
        /// Emulator status enum. @n
        /// </summary>
        /// <remarks>
        /// list of possible emulator statuses. @n
        /// </remarks>
        public enum EmuStatus : UInt32
        {
            Running = 0,            /**< Game is running */
            Paused = 1,             /**< Game is paused */
            Shutdown = 2,           /**< Game is shutdown */
        };

        public static IntPtr NewRpcs3()
        {
            return pine_rpcs3_new();
        }

        public static IntPtr NewPcsx2()
        {
            return pine_pcsx2_new();
        }

        public static void InitializeBatch(IntPtr v)
        {
            pine_initialize_batch(v);
        }

        public static int FinalizeBatch(IntPtr v)
        {
            return pine_finalize_batch(v);
        }

        public static void SendCommand(IntPtr v, int cmd)
        {
            pine_send_command(v, cmd);
        }

        public static ulong Read(IntPtr v, uint address, IPCCommand msg, bool batch = false)
        {
            return pine_read(v, address, msg, batch);
        }

        public static string Version(IntPtr v, bool batch = false)
        {
            return pine_version(v, batch);
        }

        public static EmuStatus Status(IntPtr v, bool batch = false)
        {
            return pine_status(v, batch);
        }

        public static string GetGameTitle(IntPtr v, bool batch = false)
        {
            return pine_getgametitle(v, batch);
        }

        public static string GetGameID(IntPtr v, bool batch = false)
        {
            return pine_getgameid(v, batch);
        }

        public static string GetGameUUID(IntPtr v, bool batch = false)
        {
            return pine_getgameuuid(v, batch);
        }

        public static string GetGameVersion(IntPtr v, bool batch = false)
        {
            return pine_getgameversion(v, batch);
        }

        public static void Write(IntPtr v, uint address, ulong val, IPCCommand msg, bool batch = false)
        {
            pine_write(v, address, val, msg, batch);
        }

        public static void DeleteRpcs3(IntPtr v)
        {
            pine_rpcs3_delete(v);
        }

        public static void DeletePcsx2(IntPtr v)
        {
            pine_pcsx2_delete(v);
        }

        public static void FreeBatchCommand(int cmd)
        {
            pine_free_batch_command(cmd);
        }

        public static IPCStatus GetError(IntPtr v)
        {
            return pine_get_error(v);
        }
    }
}
