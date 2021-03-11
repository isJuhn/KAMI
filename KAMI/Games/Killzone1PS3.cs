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
            uint p1 = IPCUtils.ReadU32(m_ipc, BaseAddress);
            uint p2 = IPCUtils.ReadU32(m_ipc, p1 + 0x78);
            uint p3 = IPCUtils.ReadU32(m_ipc, p2 + 0x220);
            uint p4 = IPCUtils.ReadU32(m_ipc, p3 + 0xD8);
            uint p5 = IPCUtils.ReadU32(m_ipc, p4 + 0x31C);
            m_addressVert = p4 + 0x14c;
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
