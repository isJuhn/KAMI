using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    class Xillia1 : Game<HAVACamera>
    {
        const uint BaseAddress = 0xE1552C;

        DerefChain m_addr;

        public Xillia1(IntPtr ipc) : base(ipc)
        {
            m_addr = DerefChain.CreateDerefChain(ipc, BaseAddress, 0x78);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (DerefChain.VerifyChains(m_addr))
            {
                m_camera.Hor = IPCUtils.ReadFloat(m_ipc, (uint)m_addr.Value);
                m_camera.Vert = IPCUtils.ReadFloat(m_ipc, (uint)(m_addr.Value + 4));

                m_camera.Update(diffX * SensModifier, diffY * SensModifier);

                IPCUtils.WriteFloat(m_ipc, (uint)m_addr.Value, m_camera.Hor);
                IPCUtils.WriteFloat(m_ipc, (uint)(m_addr.Value + 4), m_camera.Vert);
            }
        }
    }
}
