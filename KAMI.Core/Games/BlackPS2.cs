using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public class BlackPS2 : Game<HAVACamera>
    {
        const uint m_addrY = 0x5A8FAC;
        const uint m_addrX = 0x5A8FA8;

        public BlackPS2(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            m_camera.Vert = (float)(IPCUtils.ReadFloat(m_ipc, m_addrY) * (Math.PI / 180));
            m_camera.Hor = (float)(IPCUtils.ReadFloat(m_ipc, m_addrX) * (Math.PI / 180));
            m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, m_addrY, (float)(m_camera.Vert * (180 / Math.PI)));
            IPCUtils.WriteFloat(m_ipc, m_addrX, (float)(m_camera.Hor * (180 / Math.PI)));
        }
    }
}
