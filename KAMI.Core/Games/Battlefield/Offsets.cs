namespace KAMI.Core.Games.Battlefield
{
    // Field offsets, grouped by the game type they belong to. Types with Frostbite reflection info use its names, the
    // player and aiming types have none and are named after what they hold.
    static class Offsets
    {
        public static class PlayerManager
        {
            public const uint LocalPlayer = 0xb4;
        }

        public static class Player
        {
            public const uint SoldierHandle = 0x108;
            public const uint ControlledEntity = 0x118;
        }

        public static class ClientSoldierEntity
        {
            public const uint WeaponSelector = 0x1ec;
            public const uint WeaponsBegin = 0x1fc;
            public const uint WeaponsEnd = 0x200;
        }

        public static class Weapon
        {
            public const uint Aimer = 0x3c;
        }

        public static class Aimer
        {
            public const uint AimState = 0x8;
        }

        public static class AimState
        {
            public const uint Yaw = 0x8;
            public const uint Pitch = 0xc;
        }

        public static class ServerPlayerManager
        {
            public const uint PlayersBegin = 0x54;
            public const uint PlayersEnd = 0x58;
        }

        public static class ServerPlayer
        {
            public const uint IsHuman = 0xf0;
            public const uint Vehicle = 0x10c;
        }

        public static class LogicEntity
        {
            public const uint Data = 0x14;
        }

        public static class Component
        {
            public const uint Data = 0x14;
        }

        public static class ClientVehicleEntity
        {
            public const uint ComponentCount = 0x1ad;
            public const uint Components = 0x1b0;
            public const uint ComponentStride = 0x10;
        }

        public static class ServerVehicleEntity
        {
            public const uint ComponentCount = 0x19d;
            public const uint Components = 0x1a0;
            public const uint ComponentStride = 0x10;
        }

        public static class ClientChildComponent
        {
            public const uint Body = 0x6c;
            // The game switches the active net state to the predicted one only while the local player controls the part.
            // The predicted state is freed on every exit and reallocated on every entry, so it can't be cached.
            public const uint ActiveNetState = 0xb0;
            public const uint PredictedNetState = 0xb8;
        }

        public static class ServerChildComponent
        {
            public const uint Body = 0x70;
        }

        public static class ClientChildComponentNetState
        {
            public const uint SmoothTime = 0x14;
            public const uint Error = 0x20;
        }

        public static class ChildRotationBody
        {
            public const uint Data = 0x4;
            public const uint Orientation = 0x10;
            public const uint Axis = 0x80;
            public const uint OrientationCopy = 0xa0;
        }

        public static class ChildRotationBodyData
        {
            public const uint AngularConstraintMin = 0x14;
            public const uint AngularConstraintMax = 0x18;
            public const uint UseAngularConstraint = 0x2c;
        }

        public static class Camera
        {
            public const uint ActiveCamera = 0xdc;
            public const uint FovParameters = 0xe0;
            public const uint ReferenceFov = 0xfc;
        }

        public static class ChaseCamera
        {
            public const uint Data = 0x70;
            public const uint LookInputAverage = 0x8c;
        }

        public static class ChaseCameraData
        {
            public const uint MaxViewRotationAngleDeg = 0xdc;
        }

        public static class StaticCamera
        {
            public const uint Data = 0x70;
            public const uint LookInputAverage = 0x88;
        }

        public static class StaticCameraData
        {
            public const uint LeftYawAngle = 0xa0;
            public const uint RightYawAngle = 0xa4;
        }

        public static class MovingAverage
        {
            public const uint Capacity = 0x0;
            public const uint Count = 0x4;
            public const uint Sum = 0x8;
            public const uint Average = 0xc;
        }

        public static class CameraFovParameters
        {
            // The parameters interpolate between FOVs (zooming, sprinting), the current value is what is on screen
            public const uint CurrentFov = 0x30;
        }
    }
}
