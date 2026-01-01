using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    public class GoldenEyeReloadedPS3 : Game<HVecVACamera>
    {
        const uint baseAddress = 0xAF804C;

        DerefChain m_hor;
        DerefChain m_vert;

        public GoldenEyeReloadedPS3(IntPtr ipc, string version) : base(ipc)
        {
            if (version != "01.02")
            {
                throw new NotImplementedException($"{nameof(GoldenEyeReloadedPS3)} v'{version}' is not implemented");
            }
            var commonCameraChain = DerefChain.CreateDerefChain(ipc,
                baseAddress,
                0x1EB8,
                0x84,
                0x34,
                0x5B8,
                0xC,
                0x30
            );
            m_vert = commonCameraChain.Chain(0x84c);
            m_hor = commonCameraChain.Chain(0x14).Chain(0xFC).Chain(0x274); // HorX at +8
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (DerefChain.VerifyChains(m_hor, m_vert))
            {
                m_camera.HorY = IPCUtils.ReadFloat(m_ipc, (uint)m_hor.Value);
                m_camera.HorX = IPCUtils.ReadFloat(m_ipc, (uint)(m_hor.Value + 8));
                m_camera.Vert = IPCUtils.ReadFloat(m_ipc, (uint)m_vert.Value);                                
                m_camera.Update(diffX * SensModifier, diffY * SensModifier);                
                IPCUtils.WriteFloat(m_ipc, (uint)m_hor.Value, m_camera.HorY);
                IPCUtils.WriteFloat(m_ipc, (uint)(m_hor.Value + 8), m_camera.HorX);
                IPCUtils.WriteFloat(m_ipc, (uint)m_vert.Value, m_camera.Vert);
                
            }
        }

    }
}
