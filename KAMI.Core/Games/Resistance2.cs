using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    public class Resistance2 : Game<HVVecCamera>
    {
        DerefChain m_addr;

        public Resistance2(IntPtr ipc) : base(ipc)
        {
            m_addr = DerefChain.CreateDerefChain(ipc, 0x1210990 + 0x98, 0x370);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (m_addr.Verify())
            {
                m_camera.X = IPCUtils.ReadFloat(m_ipc, (uint)m_addr.Value);
                m_camera.Y = IPCUtils.ReadFloat(m_ipc, (uint)(m_addr.Value + 4));
                m_camera.Z = IPCUtils.ReadFloat(m_ipc, (uint)(m_addr.Value + 8));
                m_camera.Update(diffX * SensModifier, -diffY * SensModifier);
                IPCUtils.WriteFloat(m_ipc, (uint)m_addr.Value, m_camera.X);
                IPCUtils.WriteFloat(m_ipc, (uint)(m_addr.Value + 4), m_camera.Y);
                IPCUtils.WriteFloat(m_ipc, (uint)(m_addr.Value + 8), m_camera.Z);
            }
        }
    }
}
