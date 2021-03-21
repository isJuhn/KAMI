using System;
using System.Runtime.InteropServices;

namespace KAMI
{
    public static class PCSX2IPC
    {
        private const string libipc = "pcsx2_ipc_c.dll";

        [DllImport(libipc)]
        static extern IntPtr pcsx2ipc_new();

        [DllImport(libipc)]
        static extern void pcsx2ipc_initialize_batch(IntPtr v);

        [DllImport(libipc)]
        static extern Int32 pcsx2ipc_finalize_batch(IntPtr v);

        [DllImport(libipc)]
        static extern void pcsx2ipc_send_command(IntPtr v, Int32 cmd);

        [DllImport(libipc)]
        static extern UInt64 pcsx2ipc_read(IntPtr v, UInt32 address, IPCCommand msg, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pcsx2ipc_version(IntPtr v, bool batch);

        [DllImport(libipc)]
        static extern EmuStatus pcsx2ipc_status(IntPtr v, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pcsx2ipc_getgametitle(IntPtr v, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pcsx2ipc_getgameid(IntPtr v, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pcsx2ipc_getgameuuid(IntPtr v, bool batch);

        [DllImport(libipc)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        static extern string pcsx2ipc_getgameversion(IntPtr v, bool batch);

        [DllImport(libipc)]
        static extern void pcsx2ipc_write(IntPtr v, UInt32 address, UInt64 val, IPCCommand msg, bool batch);

        [DllImport(libipc)]
        static extern void pcsx2ipc_delete(IntPtr v);

        [DllImport(libipc)]
        static extern void pcsx2ipc_free_batch_command(Int32 cmd);

        [DllImport(libipc)]
        static extern IPCStatus pcsx2ipc_get_error(IntPtr v);


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
            MsgVersion = 8,         /**< Returns PCSX2 version. */
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

        public static IntPtr New()
        {
            return pcsx2ipc_new();
        }

        public static void InitializeBatch(IntPtr v)
        {
            pcsx2ipc_initialize_batch(v);
        }

        public static int FinalizeBatch(IntPtr v)
        {
            return pcsx2ipc_finalize_batch(v);
        }

        public static void SendCommand(IntPtr v, int cmd)
        {
            pcsx2ipc_send_command(v, cmd);
        }

        public static ulong Read(IntPtr v, uint address, IPCCommand msg, bool batch = false)
        {
            return pcsx2ipc_read(v, address, msg, batch);
        }

        public static string Version(IntPtr v, bool batch = false)
        {
            return pcsx2ipc_version(v, batch);
        }

        public static EmuStatus Status(IntPtr v, bool batch = false)
        {
            return pcsx2ipc_status(v, batch);
        }

        public static string GetGameTitle(IntPtr v, bool batch = false)
        {
            return pcsx2ipc_getgametitle(v, batch);
        }

        public static string GetGameID(IntPtr v, bool batch = false)
        {
            return pcsx2ipc_getgameid(v, batch);
        }

        public static string GetGameUUID(IntPtr v, bool batch = false)
        {
            return pcsx2ipc_getgameuuid(v, batch);
        }

        public static string GetGameVersion(IntPtr v, bool batch = false)
        {
            return pcsx2ipc_getgameversion(v, batch);
        }

        public static void Write(IntPtr v, uint address, ulong val, IPCCommand msg, bool batch = false)
        {
            pcsx2ipc_write(v, address, val, msg, batch);
        }

        public static void Delete(IntPtr v)
        {
            pcsx2ipc_delete(v);
        }

        public static void FreeBatchCommand(int cmd)
        {
            pcsx2ipc_free_batch_command(cmd);
        }

        public static IPCStatus GetError(IntPtr v)
        {
            return pcsx2ipc_get_error(v);
        }
    }
}
