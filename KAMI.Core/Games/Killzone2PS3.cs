using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    public class Killzone2PS3 : Game<HVecVACamera>
    {
        const uint BaseAddress = 0x117e740 + 0x234;

        DerefChain m_hor;
        DerefChain m_vert;

        public Killzone2PS3(IntPtr ipc) : base(ipc)
        {
            var baseChain = DerefChain.CreateDerefChain(ipc, BaseAddress, 0x0);
            m_vert = baseChain.Chain(0x80).Chain(0x5c).Chain(0x11c).Chain(0x78);
            m_hor = baseChain.Chain(0x78).Chain(0x0).Chain(0x68).Chain(0xc).Chain(0x90);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (DerefChain.VerifyChains(m_hor, m_vert))
            {
                m_camera.HorY = IPCUtils.ReadFloat(m_ipc, (uint)m_hor.Value);
                m_camera.HorX = IPCUtils.ReadFloat(m_ipc, (uint)(m_hor.Value + 4));
                m_camera.Vert = IPCUtils.ReadFloat(m_ipc, (uint)m_vert.Value);
                m_camera.Update(diffX * SensModifier, -diffY * SensModifier);
                IPCUtils.WriteFloat(m_ipc, (uint)m_hor.Value, m_camera.HorY);
                IPCUtils.WriteFloat(m_ipc, (uint)(m_hor.Value + 4), m_camera.HorX);
                IPCUtils.WriteFloat(m_ipc, (uint)m_vert.Value, m_camera.Vert);
            }
        }
    }
}
