using System;

namespace KAMI.Core.Games.Battlefield
{
    class Addresses
    {
        // Global game context field holding the PlayerManager, the game itself reaches the local player through this
        public uint PlayerManager { get; init; }
        // Same for the server side, vehicles are simulated there
        public uint ServerPlayerManager { get; init; }
        // Static camera singleton. The game's own LOD code scales distances by its current / reference field of view.
        public uint Camera { get; init; }
        public uint ClientVehicleVtable { get; init; }
        public uint ClientChildComponentVtable { get; init; }
        public uint ServerChildComponentVtable { get; init; }
        public uint RotationBodyVtable { get; init; }
        public uint PredictedNetStateVtable { get; init; }
        // The runtime cameras have no reflection info, unlike their ChaseCameraData and StaticCameraData
        public uint ChaseCameraVtable { get; init; }
        public uint StaticCameraVtable { get; init; }

        public static Addresses ForSerial(string serialId) => serialId switch
        {
            "BLES00261" => new Addresses
            {
                PlayerManager = 0x15A5A80,
                ServerPlayerManager = 0x1517A28,
                Camera = 0x15B3BC0,
                ClientVehicleVtable = 0x13EA938,
                ClientChildComponentVtable = 0x13E7EB8,
                ServerChildComponentVtable = 0x13B0AA8,
                RotationBodyVtable = 0x13D4788,
                PredictedNetStateVtable = 0x13E7E58,
                ChaseCameraVtable = 0x13FCF38,
                StaticCameraVtable = 0x13FCB50,
            },
            "BLUS30118" => new Addresses
            {
                PlayerManager = 0x15BFF20,
                ServerPlayerManager = 0x15CAD98,
                Camera = 0x15D3830,
                ClientVehicleVtable = 0x13DA880,
                ClientChildComponentVtable = 0x13D7D28,
                ServerChildComponentVtable = 0x13E8CA0,
                RotationBodyVtable = 0x13F4BC8,
                PredictedNetStateVtable = 0x13D7CC8,
                ChaseCameraVtable = 0x13FF770,
                StaticCameraVtable = 0x13FF388,
            },
            _ => throw new NotImplementedException($"{nameof(BadCompany)} [{serialId}] is not implemented"),
        };
    }
}
