using System;
using KAMI.Cameras;
using KAMI.Utilities;

namespace KAMI.Games
{
    class Drakengard3PS3 : Game<HAVACamera>
    {
        const uint BaseAddress = 0x019D2414;

        DerefChain m_hor;
        DerefChain m_vert;

        public Drakengard3PS3(IntPtr ipc) : base(ipc)
        {
            var baseChain = DerefChain.CreateDerefChain(ipc, BaseAddress);
            m_vert = baseChain.Chain(0x9E0);
            m_hor = baseChain.Chain(0x9E4);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (DerefChain.VerifyChains(m_hor, m_vert))
            {
                // the game uses a signed integer based camera system, with a 65536 resolution
                m_camera.Hor = ((int)(IPCUtils.ReadU32(m_ipc, (uint)m_hor.Value)) / 65536f) * (float)(2 * Math.PI);
                m_camera.Vert = ((int)(IPCUtils.ReadU32(m_ipc, (uint)m_vert.Value)) / 65536f) * (float)(2 * Math.PI);

                // the vertical value is clamped automagically by the game, min is -20°, max is 85°
                m_camera.Update(diffX * SensModifier, -diffY * SensModifier);

                IPCUtils.WriteU32(m_ipc, (uint)m_hor.Value, (uint)Math.Round(m_camera.Hor / (float)(2 * Math.PI) * 65536f));
                IPCUtils.WriteU32(m_ipc, (uint)m_vert.Value, (uint)Math.Round(m_camera.Vert / (float)(2 * Math.PI) * 65536f));
            }
        }
    }
}
