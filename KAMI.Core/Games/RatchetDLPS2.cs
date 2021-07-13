using System;

namespace KAMI.Core.Games
{
    public class RatchetDLPS2 : RatchetOGBase
    {
        public RatchetDLPS2(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (IPCUtils.ReadU32(m_ipc, 0x348410) != 0) // Some MP thing idk
            {
                m_addressHor = 0x349450;
                m_addressVert = 0x349470;
            }
            else
            {
                m_addressHor = 0x36A910;
                m_addressVert = 0x36A930;
            }
            base.UpdateCamera(diffX, diffY);
        }
    }
}
