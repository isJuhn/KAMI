using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    public class Persona5PS3 : Game<HAVACamera>
    {
        const uint BaseAddress = 0x010DD540;

        DerefChain m_hor;
        DerefChain m_vert;

        public Persona5PS3(IntPtr ipc) : base(ipc)
        {
            var baseChain = DerefChain.CreateDerefChain(ipc, BaseAddress, 0x34, 0xD8, 0x34);
            m_hor = baseChain.Chain(0x170);
            m_vert = baseChain.Chain(0x174);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (DerefChain.VerifyChains(m_hor, m_vert))
            {
                // the values are in degrees, so they need a deg -> rad conversion
                m_camera.Hor = (float)(IPCUtils.ReadFloat(m_ipc, (uint)m_hor.Value) * (Math.PI / 180));
                m_camera.Vert = (float)(IPCUtils.ReadFloat(m_ipc, (uint)m_vert.Value) * (Math.PI / 180));

                // the vertical value needs to be clamped, these values seemed reasonable
                m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
                m_camera.Vert = (float)Math.Clamp(m_camera.Vert, -60f * (Math.PI / 180), 75f * (Math.PI / 180));

                // the new values are in radians, so they need a rad -> deg conversion
                IPCUtils.WriteFloat(m_ipc, (uint)m_hor.Value, (float)(m_camera.Hor * (180 / Math.PI)));
                IPCUtils.WriteFloat(m_ipc, (uint)m_vert.Value, (float)(m_camera.Vert * (180 / Math.PI)));
            }
        }
    }
}
