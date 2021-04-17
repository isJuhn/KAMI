using KAMI.Cameras;
using KAMI.Utilities;
using System;

namespace KAMI.Games
{
    public class RDRPS3 : Game<HVVecCamera>
    {
        uint m_addr;

        public RDRPS3(IntPtr ipc, string id, string version) : base(ipc)
        {
            m_addr = id switch
            {
                // og digitals
                "NPEB00833" or "NPUB30638" when version is "01.00" or "01.02" => 0x72F0DFD0,
                // og physicals
                "BLES00680" when version is "01.00" => 0x72F0E810,
                "BLUS30418" when version is "01.00" => 0x72F25700,
                "BLES00680" or "BLUS30418" when version is "01.08" => 0x72F361E0,
                // goty editions
                "BLES01294" or "BLUS30758" when version is "01.00" => 0x72F4DCB0,
                "BLES01294" or "BLUS30758" when version is "01.01" => 0x72F495B0,
                _ => throw new NotImplementedException($"{nameof(RDRPS3)} with id '{id}' and version '{version}' is not implemented"),
            };
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            m_camera.X = IPCUtils.ReadFloat(m_ipc, m_addr + 0x0);
            m_camera.Z = IPCUtils.ReadFloat(m_ipc, m_addr + 0x8);
            m_camera.Y = IPCUtils.ReadFloat(m_ipc, m_addr + 0x4);

            m_camera.Update(diffX * SensModifier, -diffY * SensModifier);

            IPCUtils.WriteFloat(m_ipc, m_addr + 0x0, m_camera.X);
            IPCUtils.WriteFloat(m_ipc, m_addr + 0x8, m_camera.Z);
            IPCUtils.WriteFloat(m_ipc, m_addr + 0x4, m_camera.Y);

            // force disable horizontal auto centering (game setting)
            IPCUtils.WriteFloat(m_ipc, m_addr - 0xb9a8, 0x0);
            IPCUtils.WriteFloat(m_ipc, m_addr - 0xb99c, 0x0);
        }
    }
}
