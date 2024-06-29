using System;

namespace KAMI.Core.Games
{
    public class Ratchet3PS2 : RatchetOGBase
    {
        public Ratchet3PS2(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            uint mp_map_id = IPCUtils.ReadU32(m_ipc, 0x001F8528);
            if (mp_map_id < 40) // must be SP
            {
                m_addressHor = 0x1A6160;
                m_addressVert = 0x1A6180;
            }
            else
            {
                switch (mp_map_id)
                {
                    case 40://Bakisi
                        m_addressHor = 0x300AA0;
                        m_addressVert = 0x300AC0;
                        break;

                    case 41://Hoven
                        m_addressHor = 0x300BE0;
                        m_addressVert = 0x300C00;
                        break;

                    case 42://X12
                        m_addressHor = 0x2F8AE0;
                        m_addressVert = 0x2F8B00;
                        break;

                    case 43://Korgon
                        m_addressHor = 0x2F8960;
                        m_addressVert = 0x2F8980;
                        break;

                    case 44://Metro
                        m_addressHor = 0x2F89A0;
                        m_addressVert = 0x2F89C0;
                        break;

                    case 45://BWC
                        m_addressHor = 0x2F8960;
                        m_addressVert = 0x2F8980;
                        break;

                    case 46://CC
                        m_addressHor = 0x309520;
                        m_addressVert = 0x309540;
                        break;

                    case 47://Dox
                        m_addressHor = 0x309660;
                        m_addressVert = 0x309680;
                        break;

                    case 48://Sewers
                        m_addressHor = 0x3096A0;
                        m_addressVert = 0x3096C0;
                        break;

                    case 49://Marcadia
                        m_addressHor = 0x309620;
                        m_addressVert = 0x309640;
                        break;
                }
            }

            base.UpdateCamera(diffX, diffY);
        }
    }
}
