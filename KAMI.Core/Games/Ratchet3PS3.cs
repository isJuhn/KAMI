using System;

namespace KAMI.Core.Games
{
    public class Ratchet3PS3 : RatchetOGBase
    {
        public Ratchet3PS3(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (IPCUtils.ReadU32(m_ipc, 0x13DD584) > 0) // MP player 1 initialized or something
            {
                m_addressHor = 0x13DE760;
                m_addressVert = 0x13DE780;
            }
            else
            {
                m_addressHor = 0xDA3D70;
                m_addressVert = 0xDA3D90;
            }
            base.UpdateCamera(diffX, diffY);
        }
    }
}
