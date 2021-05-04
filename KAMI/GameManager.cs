using KAMI.Games;
using System;
using System.Collections.Generic;
using System.Text;

namespace KAMI
{
    public static class GameManager
    {
        public static IGame GetGame(IntPtr ipc, string id, string version)
        {
            switch (id)
            {
                case "BLUS31006":
                case "NPUB31136": return new Xillia1(ipc);
                case "BLUS31604":
                case "BLES02247":
                case "NPUB31848":
                case "NPEB02436": return new Persona5PS3(ipc);
                case "BLUS30481": return new NierPS3(ipc);
                case "NPEB00833":
                case "NPUB30638":
                case "BLES00680":
                case "BLUS30418":
                case "BLES01294":
                case "BLUS30758": return new RedDeadRedemption(ipc, id, version);
                case "BCES00052": return new RatchetToD(ipc);
                case "BCES01503": return new Ratchet3PS3(ipc);
                case "NPUA80646": return new RatchetDLPS3(ipc);
                case "BCES01743": return new Killzone1PS3(ipc);
                case "BCES00081": return new Killzone2PS3(ipc);
                case "BCUS98234":
                case "BCES01007": return new Killzone3PS3(ipc, version);
                case "BCES00001": return new Resistance1(ipc);
                case "BCES00226": return new Resistance2(ipc);
                case "BCES01118": return new Resistance3(ipc);
                case " [SCUS-97353]": return new Ratchet3PS2(ipc);
                case " [SCUS-97465]": return new RatchetDLPS2(ipc);
                default:
                    throw new NotImplementedException($"Game with id '{id}' not implemented");
            }
        }
    }
}
