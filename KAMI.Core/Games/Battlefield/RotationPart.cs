using System;
using System.Numerics;

namespace KAMI.Core.Games.Battlefield
{
    // A rotating vehicle part (turret yaw or pitch). The server body is authoritative, the client body and its
    // network smoothing state decide what is rendered.
    class RotationPart
    {
        readonly uint m_clientComponent;
        readonly RotationBody m_serverBody;
        readonly RotationBody m_clientBody;

        public Vector3 Axis => m_serverBody.Axis;

        RotationPart(uint clientComponent, RotationBody serverBody, RotationBody clientBody)
        {
            m_clientComponent = clientComponent;
            m_serverBody = serverBody;
            m_clientBody = clientBody;
        }

        // Returns null unless both components are child components with rotation bodies
        public static RotationPart Read(IntPtr ipc, Addresses addresses, uint clientComponent, uint serverComponent)
        {
            if (clientComponent == 0 || IPCUtils.ReadU32(ipc, clientComponent) != addresses.ClientChildComponentVtable ||
                serverComponent == 0 || IPCUtils.ReadU32(ipc, serverComponent) != addresses.ServerChildComponentVtable)
            {
                return null;
            }
            RotationBody clientBody = RotationBody.Read(ipc, addresses, IPCUtils.ReadU32(ipc, clientComponent + Offsets.ClientChildComponent.Body));
            RotationBody serverBody = RotationBody.Read(ipc, addresses, IPCUtils.ReadU32(ipc, serverComponent + Offsets.ServerChildComponent.Body));
            if (clientBody == null || serverBody == null)
            {
                return null;
            }
            return new RotationPart(clientComponent, serverBody, clientBody);
        }

        public bool IsAlive(IntPtr ipc, Addresses addresses)
        {
            return m_serverBody.IsAlive(ipc, addresses) && IPCUtils.ReadU32(ipc, m_clientComponent) == addresses.ClientChildComponentVtable;
        }

        public float ReadAngle(IntPtr ipc)
        {
            return m_serverBody.ReadAngle(ipc);
        }

        // Returns the part's predicted net state if the local player currently controls the part, otherwise 0. While
        // entering or leaving, and for parts another seat controls, the active state is the smaller remote state.
        public uint GetControlledNetState(IntPtr ipc, Addresses addresses)
        {
            uint predicted = IPCUtils.ReadU32(ipc, m_clientComponent + Offsets.ClientChildComponent.PredictedNetState);
            if (predicted == 0 || IPCUtils.ReadU32(ipc, m_clientComponent + Offsets.ClientChildComponent.ActiveNetState) != predicted ||
                IPCUtils.ReadU32(ipc, predicted) != addresses.PredictedNetStateVtable || IPCUtils.Error != PineIPC.IPCStatus.Success)
            {
                return 0;
            }
            return predicted;
        }

        public void WriteAngle(IntPtr ipc, uint netState, float angle)
        {
            angle = Math.Clamp(angle, m_serverBody.MinAngle, m_serverBody.MaxAngle);
            m_serverBody.WriteAngle(ipc, angle);
            m_clientBody.WriteAngle(ipc, angle);
            // The client smooths out "network errors" between 1 and 135 degrees over up to a second, which every
            // mouse movement would trigger. Clearing the error makes the rendered part follow immediately.
            IPCUtils.WriteFloat(ipc, netState + Offsets.ClientChildComponentNetState.SmoothTime, 0);
            IPCUtils.WriteFloat(ipc, netState + Offsets.ClientChildComponentNetState.Error, 0);
            IPCUtils.WriteFloat(ipc, netState + Offsets.ClientChildComponentNetState.Error + 4, 0);
            IPCUtils.WriteFloat(ipc, netState + Offsets.ClientChildComponentNetState.Error + 8, 0);
            IPCUtils.WriteFloat(ipc, netState + Offsets.ClientChildComponentNetState.Error + 12, 1);
        }
    }
}
