using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    class NierPS3 : Game<HAVACamera>
    {
        const uint BaseAddress = 0x1126854;

        DerefChain m_hor;
        DerefChain m_vert;

        public NierPS3(IntPtr ipc) : base(ipc)
        {
            var baseChain = DerefChain.CreateDerefChain(ipc, BaseAddress, 0x7c);
            m_hor = baseChain.Chain(0x1a8);
            m_vert = baseChain.Chain(0x1c4);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (DerefChain.VerifyChains(m_hor, m_vert))
            {
                m_camera.Hor = (float)(IPCUtils.ReadFloat(m_ipc, (uint)m_hor.Value));
                m_camera.Vert = (float)(IPCUtils.ReadFloat(m_ipc, (uint)m_vert.Value));

                m_camera.Update(-diffX * SensModifier, diffY * SensModifier);

                IPCUtils.WriteFloat(m_ipc, (uint)m_hor.Value, m_camera.Hor);
                IPCUtils.WriteFloat(m_ipc, (uint)m_vert.Value, m_camera.Vert);
            }
        }
    }
}
