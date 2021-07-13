using KAMI.Cameras;
using KAMI.Utilities;
using System;

namespace KAMI.Games
{
    public class Killzone3PS3 : Game<HVecVACamera>
    {
        DerefChain m_hor;
        DerefChain m_vert;

        public Killzone3PS3(IntPtr ipc, string version) : base(ipc)
        {
            var baseChain = version switch
            {
                "01.00" => DerefChain.CreateDerefChain(ipc, 0x1587880 + 0x760, 0x38, 0x0, 0x68),
                "01.14" => DerefChain.CreateDerefChain(ipc, 0x15fcf80 + 0x770, 0x40, 0x68),
                _ => throw new NotImplementedException($"{nameof(Killzone3PS3)} with version '{version}' not implemented"),
            };
            m_vert = baseChain.Chain(0x150).Chain(0xA0);
            m_hor = baseChain.Chain(0xFC).Chain(0x14).Chain(0xB0);
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
