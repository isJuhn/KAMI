using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public class RedDeadRedemption : Game<HVVecCamera>
    {
        uint m_camera_addr;
        uint m_auto_center_addr;

        public RedDeadRedemption(IntPtr ipc, string id, string version) : base(ipc)
        {
            // ASSUMES: these addresses are static / incredibly deterministic
            (m_camera_addr, m_auto_center_addr) = id switch
            {
                // og digitals
                "NPEB00833" or "NPUB30638" when version is "01.00" or "01.02" => (0x72F0DFD0u, 0x72F02628u),
                // og physicals
                "BLES00680" or "BLUS30418" when version is "01.00" => (0x72F0ED60u, 0x72ED4B98u),
                "BLES00680" or "BLUS30418" when version is "01.08" => (0x72F361E0u, 0x72EF1F28u),
                // goty editions
                "BLES01294" or "BLUS30758" when version is "01.00" => (0x72F4DCB0u, 0x72F134D8u),
                "BLES01294" or "BLUS30758" when version is "01.01" => (0x72F495B0u, 0x72F132C8u),
                _ => throw new NotImplementedException($"{nameof(RedDeadRedemption)} ['{id}', v'{version}'] is not implemented"),
            };
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            // this would also be the place to alter the selection angle
            // in the weapon wheel. the selection angle is represented
            // as a regular degree angle, between 0 and 360, default is 90.

            // the object holding it is dynamically allocated. the weapon
            // wheel show/hide state can be determined from two addresses,
            // offsetted by -0x200 or -0x320 to the angle value respectively.
            // one of them will be -1, the other will be 1, when it's shown.

            // it would be strongly preferred, if this would only be put in
            // after a "selection wheel" facility is added to KAMI, to allow
            // proper reuse with other wheel-like menus in other games.

            // important to note, that none of the aforementioned values
            // ended up being writeable, unfortunately.

            // force-disable hor. and vert. auto-centering (game setting)
            // this is required, and it also makes for a better experience
            IPCUtils.WriteFloat(m_ipc, m_auto_center_addr + 0x0, 0x0);
            IPCUtils.WriteFloat(m_ipc, m_auto_center_addr + 0xC, 0x0);

            // read current 3D camera vector values
            m_camera.X = IPCUtils.ReadFloat(m_ipc, m_camera_addr + 0x0);
            m_camera.Y = IPCUtils.ReadFloat(m_ipc, m_camera_addr + 0x4);
            m_camera.Z = IPCUtils.ReadFloat(m_ipc, m_camera_addr + 0x8);

            // recalc the values
            m_camera.Update(diffX * SensModifier, -diffY * SensModifier);

            // write new 3D camera vector values back
            IPCUtils.WriteFloat(m_ipc, m_camera_addr + 0x0, m_camera.X);
            IPCUtils.WriteFloat(m_ipc, m_camera_addr + 0x4, m_camera.Y);
            IPCUtils.WriteFloat(m_ipc, m_camera_addr + 0x8, m_camera.Z);
        }
    }
}
