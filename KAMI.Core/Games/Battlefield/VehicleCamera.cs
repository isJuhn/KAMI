using System;

namespace KAMI.Core.Games.Battlefield
{
    // The camera of a vehicle seat without a turret, either the third person chase camera or the first person static
    // (cockpit) camera. Neither has a look angle of its own: every frame they average the look input over the last few
    // frames and turn by that average times their yaw limit, so they center themselves again as soon as the stick is
    // released.
    class VehicleCamera
    {
        // The average only decays once its window is full. Making it look like it has barely started filling, with every
        // sample so far being the held input, keeps it there for days while drowning out the game's own input.
        const uint HeldSampleCount = 100000;

        public uint Address { get; }
        // Radians, what full stick input turns the camera by in each direction
        public float MaxLeftAngle { get; }
        public float MaxRightAngle { get; }
        readonly uint m_inputAverage;
        readonly uint m_inputAverageCapacity;

        VehicleCamera(uint address, float maxLeftAngle, float maxRightAngle, uint inputAverage, uint inputAverageCapacity)
        {
            Address = address;
            MaxLeftAngle = maxLeftAngle;
            MaxRightAngle = maxRightAngle;
            m_inputAverage = inputAverage;
            m_inputAverageCapacity = inputAverageCapacity;
        }

        // Returns null unless the game is currently looking through a chase or static camera
        public static VehicleCamera ReadActive(IntPtr ipc, Addresses addresses)
        {
            uint address = IPCUtils.ReadU32(ipc, addresses.Camera + Offsets.Camera.ActiveCamera);
            if (address == 0)
            {
                return null;
            }
            uint vtable = IPCUtils.ReadU32(ipc, address);
            uint data;
            float maxLeftDegrees;
            float maxRightDegrees;
            uint inputAverage;
            if (vtable == addresses.ChaseCameraVtable)
            {
                data = IPCUtils.ReadU32(ipc, address + Offsets.ChaseCamera.Data);
                maxLeftDegrees = maxRightDegrees = IPCUtils.ReadFloat(ipc, data + Offsets.ChaseCameraData.MaxViewRotationAngleDeg);
                inputAverage = IPCUtils.ReadU32(ipc, address + Offsets.ChaseCamera.LookInputAverage);
            }
            else if (vtable == addresses.StaticCameraVtable)
            {
                // Seats off to one side can look further one way than the other
                data = IPCUtils.ReadU32(ipc, address + Offsets.StaticCamera.Data);
                maxLeftDegrees = IPCUtils.ReadFloat(ipc, data + Offsets.StaticCameraData.LeftYawAngle);
                maxRightDegrees = IPCUtils.ReadFloat(ipc, data + Offsets.StaticCameraData.RightYawAngle);
                inputAverage = IPCUtils.ReadU32(ipc, address + Offsets.StaticCamera.LookInputAverage);
            }
            else
            {
                return null;
            }
            uint capacity = IPCUtils.ReadU32(ipc, inputAverage + Offsets.MovingAverage.Capacity);
            if (IPCUtils.Error != PineIPC.IPCStatus.Success || inputAverage == 0 || capacity == 0 || capacity >= HeldSampleCount ||
                maxLeftDegrees == 0 || maxRightDegrees == 0)
            {
                return null;
            }
            return new VehicleCamera(address, MathF.Abs(maxLeftDegrees) * MathF.PI / 180, MathF.Abs(maxRightDegrees) * MathF.PI / 180,
                inputAverage, capacity);
        }

        public bool IsActive(IntPtr ipc, Addresses addresses)
        {
            return IPCUtils.ReadU32(ipc, addresses.Camera + Offsets.Camera.ActiveCamera) == Address && IsAlive(ipc, addresses);
        }

        public bool IsAlive(IntPtr ipc, Addresses addresses)
        {
            uint vtable = IPCUtils.ReadU32(ipc, Address);
            return vtable == addresses.ChaseCameraVtable || vtable == addresses.StaticCameraVtable;
        }

        // Positive angles look right, like pushing the stick right does
        public float ReadAngle(IntPtr ipc)
        {
            float input = IPCUtils.ReadFloat(ipc, m_inputAverage + Offsets.MovingAverage.Average);
            return input * (input > 0 ? MaxRightAngle : MaxLeftAngle);
        }

        public void HoldAngle(IntPtr ipc, float angle)
        {
            float input = angle > 0 ? Math.Min(angle / MaxRightAngle, 1) : Math.Max(angle / MaxLeftAngle, -1);
            // The count goes first, the game would divide the huge sum by the small window otherwise
            IPCUtils.WriteU32(ipc, m_inputAverage + Offsets.MovingAverage.Count, HeldSampleCount);
            IPCUtils.WriteFloat(ipc, m_inputAverage + Offsets.MovingAverage.Sum, input * (HeldSampleCount + 1));
            IPCUtils.WriteFloat(ipc, m_inputAverage + Offsets.MovingAverage.Average, input);
        }

        // Turns the average back into a full window of the held input, the game then eases back to the stick input
        public void Release(IntPtr ipc)
        {
            float input = IPCUtils.ReadFloat(ipc, m_inputAverage + Offsets.MovingAverage.Average);
            // The sum goes first this time, for the same reason
            IPCUtils.WriteFloat(ipc, m_inputAverage + Offsets.MovingAverage.Sum, input * m_inputAverageCapacity);
            IPCUtils.WriteU32(ipc, m_inputAverage + Offsets.MovingAverage.Count, m_inputAverageCapacity);
        }
    }
}
