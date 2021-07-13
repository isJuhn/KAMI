using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public class RatchetToD : Game<HVVecCamera>
    {
        const uint m_address = 0x101B01E0;

        public RatchetToD(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            m_camera.X = IPCUtils.ReadFloat(m_ipc, m_address);
            m_camera.Y = IPCUtils.ReadFloat(m_ipc, m_address + 4);
            m_camera.Z = IPCUtils.ReadFloat(m_ipc, m_address + 8);
            m_camera.Update(-diffX * SensModifier, -diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, m_address, m_camera.X);
            IPCUtils.WriteFloat(m_ipc, m_address + 4, m_camera.Y);
            IPCUtils.WriteFloat(m_ipc, m_address + 8, m_camera.Z);
        }
    }
}
