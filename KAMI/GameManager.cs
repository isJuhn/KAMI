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
                case "BLUS31604":
                case "BLES02247":
                case "NPUB31848":
                case "NPEB02436": return new Persona5PS3(ipc);
                case "BLUS30481": return new NierPS3(ipc);
                case "BCES00052": return new RatchetToD(ipc);
                case "BCES01503": return new Ratchet3PS3(ipc);
                case "BCES01743": return new Killzone1PS3(ipc);
                case "BCES00081": return new Killzone2PS3(ipc);
                case "BCUS98234":
                case "BCES01007": return new Killzone3PS3(ipc, version);
                case "BCES00001": return new Resistance1(ipc);
                case "BCES00226": return new Resistance2(ipc);
                case "[SCUS-97353]": return new Ratchet3PS2(ipc);
                default:
                    throw new NotImplementedException($"Game with id '{id}' not implemented");
            }
        }
    }
}
