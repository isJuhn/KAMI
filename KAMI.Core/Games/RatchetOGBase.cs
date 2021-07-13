using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public class RatchetOGBase : Game<HAVACamera>
    {
        protected uint m_addressHor;
        protected uint m_addressVert;

        public RatchetOGBase(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            m_camera.Hor = IPCUtils.ReadFloat(m_ipc, m_addressHor);
            m_camera.Vert = IPCUtils.ReadFloat(m_ipc, m_addressVert);
            m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, m_addressHor, m_camera.Hor);
            IPCUtils.WriteFloat(m_ipc, m_addressVert, m_camera.Vert);
        }
    }
}
