using System;

namespace KAMI.Core.Games
{
    public class RatchetDLPS3 : RatchetOGBase
    {
        public RatchetDLPS3(IntPtr ipc) : base(ipc)
        {
            m_addressHor = 0x10D5DE0;
            m_addressVert = 0x10D5E00;
        }
    }
}
