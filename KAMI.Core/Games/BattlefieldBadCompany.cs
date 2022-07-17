using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    public class BattlefieldBadCompany : Game<HAVACamera>
    {
        DerefChain m_addr;

        public BattlefieldBadCompany(IntPtr ipc, string serialId) : base(ipc)
        {
            int baseAddress = serialId == "BLES00261" ? 0x15A5A88 : 0x15BFF28;
                                                                      // 0x1b8, 0...?
            m_addr = DerefChain.CreateDerefChain(ipc, baseAddress, 0x1c, 0x1c0, 0x14, 0x2c, 0x4, 0x4, 0x8, 0x4, 0x1bc, 0x20c, 0x3c, 0x8, 0x8);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (m_addr.Verify())
            {
                m_camera.Hor = (float)(IPCUtils.ReadFloat(m_ipc, (uint)m_addr.Value));
                m_camera.Vert = (float)(IPCUtils.ReadFloat(m_ipc, (uint)m_addr.Value + 4));
                m_camera.Update(diffX * SensModifier, -diffY * SensModifier);
                IPCUtils.WriteFloat(m_ipc, (uint)m_addr.Value, (float)(m_camera.Hor));
                IPCUtils.WriteFloat(m_ipc, (uint)m_addr.Value + 4, (float)(m_camera.Vert));
            }
        }
    }
}
