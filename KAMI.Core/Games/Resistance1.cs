using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    public class Resistance1 : Game<HAVACamera>
    {
        DerefChain m_addr;

        public Resistance1(IntPtr ipc) : base(ipc)
        {
            m_addr = DerefChain.CreateDerefChain(ipc, 0x1034fd04, 0x360);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (m_addr.Verify())
            {
                m_camera.Vert = IPCUtils.ReadFloat(m_ipc, (uint)m_addr.Value);
                m_camera.Hor = IPCUtils.ReadFloat(m_ipc, (uint)(m_addr.Value + 4));
                m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
                IPCUtils.WriteFloat(m_ipc, (uint)m_addr.Value, m_camera.Vert);
                IPCUtils.WriteFloat(m_ipc, (uint)(m_addr.Value + 4), m_camera.Hor);
            }
        }
    }
}
