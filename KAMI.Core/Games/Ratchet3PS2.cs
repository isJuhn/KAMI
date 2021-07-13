using System;

namespace KAMI.Games
{
    public class Ratchet3PS2 : RatchetOGBase
    {
        public Ratchet3PS2(IntPtr ipc) : base(ipc)
        {
            m_addressHor = 0x1A6160;
            m_addressVert = 0x1A6180;
        }
    }
}
