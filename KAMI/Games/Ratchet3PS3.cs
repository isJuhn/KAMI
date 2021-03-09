using System;
using System.Collections.Generic;
using System.Text;

namespace KAMI.Games
{
    public class Ratchet3PS3 : RatchetOGBase
    {
        public Ratchet3PS3(IntPtr ipc) : base(ipc)
        {
        }

        public override void InjectionStart()
        {
            m_addressHor = 0xDA3D70;
            m_addressVert = 0xDA3D90;
        }
    }
}
