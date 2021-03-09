using KAMI.Cameras;
using System;

namespace KAMI.Games
{
    public class Killzone1PS3 : Game<HVecVACamera>
    {
        const uint BaseAddress = 0x828734;

        uint m_addressHor;
        uint m_addressVert;

        public Killzone1PS3(IntPtr ipc) : base(ipc)
        {
        }

        public override void InjectionStart()
        {
            uint p1 = (uint)PCSX2IPC.Read(m_ipc, BaseAddress, PCSX2IPC.IPCCommand.MsgRead32, false);
            uint p2 = (uint)PCSX2IPC.Read(m_ipc, p1 + 0x78, PCSX2IPC.IPCCommand.MsgRead32, false);
            uint p3 = (uint)PCSX2IPC.Read(m_ipc, p2 + 0x220, PCSX2IPC.IPCCommand.MsgRead32, false);
            uint p4 = (uint)PCSX2IPC.Read(m_ipc, p3 + 0xD8, PCSX2IPC.IPCCommand.MsgRead32, false);
            m_addressVert = p4 + 0x14c;
            uint p5 = (uint)PCSX2IPC.Read(m_ipc, p4 + 0x31C, PCSX2IPC.IPCCommand.MsgRead32, false);
            m_addressHor = p5 + 0x78;
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            m_camera.HorX = IPCUtils.ReadFloat(m_ipc, m_addressHor);
            m_camera.HorY = IPCUtils.ReadFloat(m_ipc, m_addressHor + 4);
            m_camera.Vert = IPCUtils.ReadFloat(m_ipc, m_addressVert);
            m_camera.Update(-diffX * SensModifier, -diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, m_addressHor, m_camera.HorX);
            IPCUtils.WriteFloat(m_ipc, m_addressHor + 4, m_camera.HorY);
            IPCUtils.WriteFloat(m_ipc, m_addressVert, m_camera.Vert);
        }
    }
}
