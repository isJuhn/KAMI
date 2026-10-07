using System;

namespace KAMI.Core.Games.Battlefield
{
    // The rotating parts of the vehicle the local player is in. Turrets rotate through rotation bodies, the yaw body
    // rotates around the Y axis and the pitch body around the X axis.
    class Turret
    {
        public uint ClientVehicle { get; }
        public RotationPart Yaw { get; }
        public RotationPart Pitch { get; }

        Turret(uint clientVehicle, RotationPart yaw, RotationPart pitch)
        {
            ClientVehicle = clientVehicle;
            Yaw = yaw;
            Pitch = pitch;
        }

        // Client and server components of the same part share their data, which is used to pair them up
        public static Turret Find(IntPtr ipc, Addresses addresses, uint clientVehicle)
        {
            uint serverVehicle = GetServerVehicle(ipc, addresses);
            if (serverVehicle == 0 || IPCUtils.ReadU32(ipc, serverVehicle + Offsets.LogicEntity.Data) != IPCUtils.ReadU32(ipc, clientVehicle + Offsets.LogicEntity.Data))
            {
                return null;
            }
            RotationPart yaw = null;
            RotationPart pitch = null;
            uint componentCount = IPCUtils.ReadU8(ipc, clientVehicle + Offsets.ClientVehicleEntity.ComponentCount);
            for (uint i = 0; i < componentCount; i++)
            {
                uint clientComponent = IPCUtils.ReadU32(ipc, clientVehicle + Offsets.ClientVehicleEntity.Components + i * Offsets.ClientVehicleEntity.ComponentStride);
                if (clientComponent == 0 || IPCUtils.ReadU32(ipc, clientComponent) != addresses.ClientChildComponentVtable)
                {
                    continue;
                }
                uint serverComponent = FindServerChildComponent(ipc, addresses, serverVehicle, IPCUtils.ReadU32(ipc, clientComponent + Offsets.Component.Data));
                RotationPart part = RotationPart.Read(ipc, addresses, clientComponent, serverComponent);
                if (part == null)
                {
                    continue;
                }
                if (yaw == null && MathF.Abs(part.Axis.Y) > 0.9f)
                {
                    yaw = part;
                }
                else if (pitch == null && MathF.Abs(part.Axis.X) > 0.9f)
                {
                    pitch = part;
                }
            }
            if (IPCUtils.Error != PineIPC.IPCStatus.Success || (yaw == null && pitch == null))
            {
                return null;
            }
            return new Turret(clientVehicle, yaw, pitch);
        }

        static uint GetServerVehicle(IntPtr ipc, Addresses addresses)
        {
            uint serverPlayerManager = IPCUtils.ReadU32(ipc, addresses.ServerPlayerManager);
            if (serverPlayerManager == 0)
            {
                return 0;
            }
            uint playersBegin = IPCUtils.ReadU32(ipc, serverPlayerManager + Offsets.ServerPlayerManager.PlayersBegin);
            uint playersEnd = IPCUtils.ReadU32(ipc, serverPlayerManager + Offsets.ServerPlayerManager.PlayersEnd);
            if (playersEnd < playersBegin || playersEnd - playersBegin > 64 * 4)
            {
                return 0;
            }
            for (uint playerAddress = playersBegin; playerAddress < playersEnd; playerAddress += 4)
            {
                uint player = IPCUtils.ReadU32(ipc, playerAddress);
                if (player != 0 && IPCUtils.ReadU8(ipc, player + Offsets.ServerPlayer.IsHuman) != 0)
                {
                    return IPCUtils.ReadU32(ipc, player + Offsets.ServerPlayer.Vehicle);
                }
            }
            return 0;
        }

        static uint FindServerChildComponent(IntPtr ipc, Addresses addresses, uint serverVehicle, uint componentData)
        {
            uint componentCount = IPCUtils.ReadU8(ipc, serverVehicle + Offsets.ServerVehicleEntity.ComponentCount);
            for (uint i = 0; i < componentCount; i++)
            {
                uint component = IPCUtils.ReadU32(ipc, serverVehicle + Offsets.ServerVehicleEntity.Components + i * Offsets.ServerVehicleEntity.ComponentStride);
                if (component != 0 && IPCUtils.ReadU32(ipc, component) == addresses.ServerChildComponentVtable &&
                    IPCUtils.ReadU32(ipc, component + Offsets.Component.Data) == componentData)
                {
                    return component;
                }
            }
            return 0;
        }

        public bool IsAlive(IntPtr ipc, Addresses addresses)
        {
            return (Yaw ?? Pitch).IsAlive(ipc, addresses);
        }
    }
}
