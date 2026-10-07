using System;
using System.Numerics;

namespace KAMI.Core.Games.Battlefield
{
    // A ChildRotationBody, the physics body a turret part rotates with. The axis and angular constraint come from the
    // body's data and never change, so they are only read once.
    class RotationBody
    {
        public uint Address { get; }
        public Vector3 Axis { get; }
        public float MinAngle { get; }
        public float MaxAngle { get; }

        RotationBody(uint address, Vector3 axis, float minAngle, float maxAngle)
        {
            Address = address;
            Axis = axis;
            MinAngle = minAngle;
            MaxAngle = maxAngle;
        }

        // Returns null unless address points to a rotation body
        public static RotationBody Read(IntPtr ipc, Addresses addresses, uint address)
        {
            if (address == 0 || IPCUtils.ReadU32(ipc, address) != addresses.RotationBodyVtable)
            {
                return null;
            }
            var axis = new Vector3(
                IPCUtils.ReadFloat(ipc, address + Offsets.ChildRotationBody.Axis),
                IPCUtils.ReadFloat(ipc, address + Offsets.ChildRotationBody.Axis + 4),
                IPCUtils.ReadFloat(ipc, address + Offsets.ChildRotationBody.Axis + 8));
            (float min, float max) = ReadAngularConstraint(ipc, address);
            if (IPCUtils.Error != PineIPC.IPCStatus.Success)
            {
                return null;
            }
            return new RotationBody(address, axis, min, max);
        }

        // The physics clamps the body to these limits anyway, clamping before writing avoids fighting it at the edges
        static (float, float) ReadAngularConstraint(IntPtr ipc, uint address)
        {
            uint data = IPCUtils.ReadU32(ipc, address + Offsets.ChildRotationBody.Data);
            if (IPCUtils.ReadU8(ipc, data + Offsets.ChildRotationBodyData.UseAngularConstraint) == 0)
            {
                return (float.NegativeInfinity, float.PositiveInfinity);
            }
            float min = IPCUtils.ReadFloat(ipc, data + Offsets.ChildRotationBodyData.AngularConstraintMin);
            float max = IPCUtils.ReadFloat(ipc, data + Offsets.ChildRotationBodyData.AngularConstraintMax);
            return (min * MathF.PI / 180, max * MathF.PI / 180);
        }

        public bool IsAlive(IntPtr ipc, Addresses addresses)
        {
            return IPCUtils.ReadU32(ipc, Address) == addresses.RotationBodyVtable;
        }

        // The game only keeps its own angle field up to date for bodies with an angular constraint, a freely spinning
        // turret (the LAV's yaw) always reads 0 there. The orientation is right for every body.
        public float ReadAngle(IntPtr ipc)
        {
            uint orientation = Address + Offsets.ChildRotationBody.Orientation;
            var axisPart = new Vector3(
                IPCUtils.ReadFloat(ipc, orientation),
                IPCUtils.ReadFloat(ipc, orientation + 4),
                IPCUtils.ReadFloat(ipc, orientation + 8));
            float w = IPCUtils.ReadFloat(ipc, orientation + 12);
            float angle = 2 * MathF.Atan2(Vector3.Dot(axisPart, Axis), w);
            // Keep it within -pi..pi, a quaternion and its negation are the same rotation
            return angle > MathF.PI ? angle - 2 * MathF.PI : angle <= -MathF.PI ? angle + 2 * MathF.PI : angle;
        }

        public void WriteAngle(IntPtr ipc, float angle)
        {
            // The angle is derived from the orientation quaternion, so that is what has to be written
            Quaternion orientation = Quaternion.CreateFromAxisAngle(Axis, angle);
            foreach (uint offset in new[] { Offsets.ChildRotationBody.Orientation, Offsets.ChildRotationBody.OrientationCopy })
            {
                IPCUtils.WriteFloat(ipc, Address + offset, orientation.X);
                IPCUtils.WriteFloat(ipc, Address + offset + 4, orientation.Y);
                IPCUtils.WriteFloat(ipc, Address + offset + 8, orientation.Z);
                IPCUtils.WriteFloat(ipc, Address + offset + 12, orientation.W);
            }
        }
    }
}
