using KAMI.Cameras;
using System;

namespace KAMI.Games
{
    public class Resistance3 : Game<HVVecCamera>
    {
        const uint m_address = 0x3A504910;

        public Resistance3(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            m_camera.X = IPCUtils.ReadFloat(m_ipc, m_address);
            m_camera.Y = IPCUtils.ReadFloat(m_ipc, m_address + 4);
            m_camera.Z = IPCUtils.ReadFloat(m_ipc, m_address + 8);
            m_camera.Update(diffX * SensModifier, -diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, m_address, m_camera.X);
            IPCUtils.WriteFloat(m_ipc, m_address + 4, m_camera.Y);
            IPCUtils.WriteFloat(m_ipc, m_address + 8, m_camera.Z);
        }
    }
}
