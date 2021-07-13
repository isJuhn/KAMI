using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public class CallOfDuty3 : Game<HAVACamera>
    {
        const uint m_addr = 0x101A17C4;

        public CallOfDuty3(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            m_camera.Vert = (float)(IPCUtils.ReadFloat(m_ipc, m_addr) * (Math.PI / 180));
            m_camera.Hor = (float)(IPCUtils.ReadFloat(m_ipc, m_addr + 4) * (Math.PI / 180));
            m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, m_addr, (float)(m_camera.Vert * (180 / Math.PI)));
            IPCUtils.WriteFloat(m_ipc, m_addr + 4, (float)(m_camera.Hor * (180 / Math.PI)));
        }
    }
}
