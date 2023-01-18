using System;
using System.Runtime.InteropServices;

namespace KAMI.Core.Windows
{
    internal class RawInputMouseHandler : BaseMouseHandler
    {
        [DllImport("user32", SetLastError = true)]
        static extern uint GetRawInputDeviceList([Out] RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);

        [DllImport("user32.dll")]
        public static extern int GetRawInputData(IntPtr hRawInput, RawInputCommand uiCommand, out RAWINPUT pData, ref int pcbSize, int cbSizeHeader);

        [DllImport("user32.dll")]
        public static extern int GetRawInputData(IntPtr hRawInput, RawInputCommand uiCommand, IntPtr pData, ref int pcbSize, int cbSizeHeader);

        [DllImport("user32.dll")]
        public static extern bool RegisterRawInputDevices([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] RAWINPUTDEVICE[] pRawInputDevices, int uiNumDevices, int cbSize);

        int xDiff = 0;
        int yDiff = 0;

        internal RawInputMouseHandler(IntPtr hwnd, Action<HwndHook> addHookAction)
        {
            addHookAction(HwndHook);
            RAWINPUTDEVICE[] Rid = new RAWINPUTDEVICE[1];

            Rid[0].UsagePage = HIDUsagePageGeneric;
            Rid[0].Usage = HIDUsageMouse;
            Rid[0].Flags = RawInputDeviceFlags.InputSink;
            Rid[0].WindowHandle = hwnd;

            bool result = RegisterRawInputDevices(Rid, 1, 0x10);
            if (!result)
            {
                throw new Exception("Failed to register raw input");
            }
        }

        public override (int, int) GetCenterDiff()
        {
            var ret = (xDiff, yDiff);
            xDiff = yDiff = 0;
            return ret;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_INPUT = 0x00FF;
            switch (msg)
            {
                case WM_INPUT:
                    int size = 0x30;
                    GetRawInputData(lParam, RawInputCommand.RID_INPUT, out RAWINPUT input, ref size, 0x18);
                    xDiff += input.mouse.LastX;
                    yDiff += input.mouse.LastY;
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        [Flags()]
        public enum RawMouseFlags : ushort
        {
            /// <summary>Relative to the last position.</summary>
            MoveRelative = 0,
            /// <summary>Absolute positioning.</summary>
            MoveAbsolute = 1,
            /// <summary>Coordinate data is mapped to a virtual desktop.</summary>
            VirtualDesktop = 2,
            /// <summary>Attributes for the mouse have changed.</summary>
            AttributesChanged = 4
        }

        [Flags()]
        public enum RawMouseButtons : ushort
        {
            /// <summary>No button.</summary>
            None = 0,
            /// <summary>Left (button 1) down.</summary>
            LeftDown = 0x0001,
            /// <summary>Left (button 1) up.</summary>
            LeftUp = 0x0002,
            /// <summary>Right (button 2) down.</summary>
            RightDown = 0x0004,
            /// <summary>Right (button 2) up.</summary>
            RightUp = 0x0008,
            /// <summary>Middle (button 3) down.</summary>
            MiddleDown = 0x0010,
            /// <summary>Middle (button 3) up.</summary>
            MiddleUp = 0x0020,
            /// <summary>Button 4 down.</summary>
            Button4Down = 0x0040,
            /// <summary>Button 4 up.</summary>
            Button4Up = 0x0080,
            /// <summary>Button 5 down.</summary>
            Button5Down = 0x0100,
            /// <summary>Button 5 up.</summary>
            Button5Up = 0x0200,
            /// <summary>Mouse wheel moved.</summary>
            MouseWheel = 0x0400
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWMOUSE
        {
            /// <summary>
                    /// The mouse state.
                    /// </summary>
            [FieldOffset(0)]
            public RawMouseFlags Flags;
            /// <summary>
                    /// Flags for the event.
                    /// </summary>
            [FieldOffset(4)]
            public RawMouseButtons ButtonFlags;
            /// <summary>
                    /// If the mouse wheel is moved, this will contain the delta amount.
                    /// </summary>
            [FieldOffset(6)]
            public ushort ButtonData;
            /// <summary>
                    /// Raw button data.
                    /// </summary>
            [FieldOffset(8)]
            public uint RawButtons;
            /// <summary>
                    /// The motion in the X direction. This is signed relative motion or
                    /// absolute motion, depending on the value of usFlags.
                    /// </summary>
            [FieldOffset(12)]
            public int LastX;
            /// <summary>
                    /// The motion in the Y direction. This is signed relative motion or absolute motion,
                    /// depending on the value of usFlags.
                    /// </summary>
            [FieldOffset(16)]
            public int LastY;
            /// <summary>
                    /// The device-specific additional information for the event.
                    /// </summary>
            [FieldOffset(20)]
            public uint ExtraInformation;
        }

        public struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWMOUSE mouse;
        }

        public struct RAWINPUTHEADER
        {
            /// <summary>Type of device the input is coming from.</summary>
            public RawInputDeviceType Type;
            /// <summary>Size of the packet of data.</summary>
            public int Size;
            /// <summary>Handle to the device sending the data.</summary>
            public IntPtr Device;
            /// <summary>wParam from the window message.</summary>
            public IntPtr wParam;
        }

        public enum RawInputDeviceType : uint
        {
            MOUSE = 0,
            KEYBOARD = 1,
            HID = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public RawInputDeviceType Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort UsagePage;
            public ushort Usage;
            public RawInputDeviceFlags Flags;
            public IntPtr WindowHandle;
        }

        const ushort HIDUsagePageGeneric = 0x01;
        const ushort HIDUsageMouse = 0x02;

        [Flags()]
        public enum RawInputDeviceFlags
        {
            /// <summary>No flags.</summary>
            None = 0,
            /// <summary>If set, this removes the top level collection from the inclusion list. This tells the operating system to stop reading from a device which matches the top level collection.</summary>
            Remove = 0x00000001,
            /// <summary>If set, this specifies the top level collections to exclude when reading a complete usage page. This flag only affects a TLC whose usage page is already specified with PageOnly.</summary>
            Exclude = 0x00000010,
            /// <summary>If set, this specifies all devices whose top level collection is from the specified usUsagePage. Note that Usage must be zero. To exclude a particular top level collection, use Exclude.</summary>
            PageOnly = 0x00000020,
            /// <summary>If set, this prevents any devices specified by UsagePage or Usage from generating legacy messages. This is only for the mouse and keyboard.</summary>
            NoLegacy = 0x00000030,
            /// <summary>If set, this enables the caller to receive the input even when the caller is not in the foreground. Note that WindowHandle must be specified.</summary>
            InputSink = 0x00000100,
            /// <summary>If set, the mouse button click does not activate the other window.</summary>
            CaptureMouse = 0x00000200,
            /// <summary>If set, the application-defined keyboard device hotkeys are not handled. However, the system hotkeys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. NoHotKeys can be specified even if NoLegacy is not specified and WindowHandle is NULL.</summary>
            NoHotKeys = 0x00000200,
            /// <summary>If set, application keys are handled.  NoLegacy must be specified.  Keyboard only.</summary>
            AppKeys = 0x00000400
        }

        public enum RawInputCommand
        {
            RID_HEADER = 0x10000005,
            RID_INPUT = 0x10000003,
        }
    }
}
