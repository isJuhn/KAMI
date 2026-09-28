using KAMI.Core.Cameras;
using System;
using System.Numerics;

namespace KAMI.Core.Games
{
    // Ratchet & Clank Future: Tools of Destruction
    // Reversed from BCES00052.
    //
    // The game keeps one CameraManager per local player in g_CameraManagers (0x101B0050, 4 x 0x21D0 bytes).
    // Each manager owns 16 camera slots of 0x200 bytes starting at +0xC0. A slot is a polymorphic camera
    // object, its type id (index into the camera type table at 0x10060198) lives at +0x64, -1 means free.
    // Slots are not cleared when a camera stops being used, so scanning slots for a type finds stale cameras.
    // Instead, CameraManager+0x2114 points at the slot that is currently in control. While switching
    // cameras the active slot is a transition camera (type 0) which blends between two other slots, the
    // camera being blended to is the one that should receive mouse input.
    public class RatchetToD : Game<HAVACamera>
    {
        const uint CameraManagerAddress = 0x101B0050; // g_CameraManagers[0], player 1
        const uint ManagerUp = 0xA0;                  // float[4], current gravity up vector
        const uint ManagerSlots = 0xC0;
        const uint ManagerActiveCamera = 0x2114;      // CameraSlot*
        const uint SlotSize = 0x200;
        const uint SlotCount = 16;

        const uint CameraType = 0x64;
        const uint CameraHandle = 0x6C;

        const uint LevelObjPoolCount = 0x10723AE4;
        const uint LevelObjPool = 0x10723AE8;         // pointer to 0x90 byte entries, entry+0x54 = handle
        const uint LevelObjSize = 0x90;

        const uint TypeTransition = 0x00;
        const uint TypeAimFree = 0x0F;
        const uint TypeAim = 0x10;
        const uint TypeLockStrafe = 0x1A;

        // LockStrafeCamera: unit look direction, the camera sits at pivot - lookDir * 5.5
        const uint LockStrafeLookDir = 0xD0;
        // The game clamps the look direction to this elevation range relative to the up vector
        static readonly float LockStrafeMinElevation = -85f * MathF.PI / 180f;
        static readonly float LockStrafeMaxElevation = 45f * MathF.PI / 180f;

        // AimCamera (0x10) and AimFreeCamera (0x0F): yaw/pitch in radians, clamped by the game
        const uint AimYaw = 0xF4;
        const uint AimPitch = 0xFC;
        const uint AimFreeYaw = 0xC0;
        const uint AimFreePitch = 0xC8;

        public RatchetToD(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            uint camera = GetControlledCamera();
            if (camera == 0)
            {
                return;
            }

            switch (IPCUtils.ReadU32(m_ipc, camera + CameraType))
            {
                case TypeLockStrafe:
                    UpdateLockStrafe(camera, diffX * SensModifier, -diffY * SensModifier);
                    break;
                case TypeAim:
                    UpdateAim(camera + AimYaw, camera + AimPitch, diffX, diffY);
                    break;
                case TypeAimFree:
                    UpdateAim(camera + AimFreeYaw, camera + AimFreePitch, diffX, diffY);
                    break;
            }
        }

        private uint GetControlledCamera()
        {
            uint camera = IPCUtils.ReadU32(m_ipc, CameraManagerAddress + ManagerActiveCamera);
            if (!IsCameraSlot(camera))
            {
                return 0;
            }
            if (IPCUtils.ReadU32(m_ipc, camera + CameraType) == TypeTransition)
            {
                camera = GetTransitionTarget(camera);
            }
            return IsCameraSlot(camera) ? camera : 0;
        }

        private static bool IsCameraSlot(uint camera)
        {
            uint firstSlot = CameraManagerAddress + ManagerSlots;
            return camera >= firstSlot && camera < firstSlot + SlotCount * SlotSize && (camera - firstSlot) % SlotSize == 0;
        }

        private uint GetTransitionTarget(uint camera)
        {
            // TransitionCamera_GetData: the slot handle resolves to a level object whose +0x40 points at the
            // transition data, which holds the camera being blended from (+0x5C) and to (+0x60)
            uint handle = IPCUtils.ReadU32(m_ipc, camera + CameraHandle);
            uint index = handle & 0xFFFF;
            if (index >= IPCUtils.ReadU32(m_ipc, LevelObjPoolCount))
            {
                return 0;
            }
            uint entry = IPCUtils.ReadU32(m_ipc, LevelObjPool) + index * LevelObjSize;
            if (IPCUtils.ReadU32(m_ipc, entry + 0x54) != handle)
            {
                return 0;
            }
            uint data = IPCUtils.ReadU32(m_ipc, entry + 0x40);
            return data == 0 ? 0 : IPCUtils.ReadU32(m_ipc, data + 0x60);
        }

        private void UpdateLockStrafe(uint camera, float yaw, float pitch)
        {
            // Rotate relative to the manager's up vector rather than world Y so the camera keeps working
            // when gravity changes (gravity ramps, magnetic boots)
            Vector3 up = ReadVector3(CameraManagerAddress + ManagerUp);
            if (up.LengthSquared() < 1e-6f)
            {
                up = Vector3.UnitY;
            }
            up = Vector3.Normalize(up);

            uint address = camera + LockStrafeLookDir;
            Vector3 lookDir = ReadVector3(address);
            if (lookDir.LengthSquared() < 1e-6f)
            {
                return;
            }
            lookDir = Vector3.Normalize(lookDir);

            // Split into elevation and a horizontal direction in the plane perpendicular to up
            float elevation = MathF.Asin(Math.Clamp(Vector3.Dot(lookDir, up), -1f, 1f));
            Vector3 horizontal = lookDir - up * Vector3.Dot(lookDir, up);
            if (horizontal.LengthSquared() < 1e-6f)
            {
                return;
            }
            horizontal = Vector3.Normalize(horizontal);

            horizontal = Vector3.Transform(horizontal, Quaternion.CreateFromAxisAngle(up, -yaw));
            elevation = Math.Clamp(elevation + pitch, LockStrafeMinElevation, LockStrafeMaxElevation);
            lookDir = horizontal * MathF.Cos(elevation) + up * MathF.Sin(elevation);

            WriteVector3(address, lookDir);
        }

        private void UpdateAim(uint yawAddress, uint pitchAddress, int diffX, int diffY)
        {
            m_camera.Hor = IPCUtils.ReadFloat(m_ipc, yawAddress);
            m_camera.Vert = IPCUtils.ReadFloat(m_ipc, pitchAddress);
            m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, yawAddress, m_camera.Hor);
            IPCUtils.WriteFloat(m_ipc, pitchAddress, m_camera.Vert);
        }

        private Vector3 ReadVector3(uint address)
        {
            return new Vector3(
                IPCUtils.ReadFloat(m_ipc, address),
                IPCUtils.ReadFloat(m_ipc, address + 4),
                IPCUtils.ReadFloat(m_ipc, address + 8));
        }

        private void WriteVector3(uint address, Vector3 value)
        {
            IPCUtils.WriteFloat(m_ipc, address, value.X);
            IPCUtils.WriteFloat(m_ipc, address + 4, value.Y);
            IPCUtils.WriteFloat(m_ipc, address + 8, value.Z);
        }
    }
}
