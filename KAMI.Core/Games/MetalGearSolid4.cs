using KAMI.Core.Cameras;
using KAMI.Core.Utilities;
using System;

namespace KAMI.Core.Games
{
    public class MetalGearSolid4 : Game<HAVACamera>
    {
        uint m_base_address;
        uint m_camera_address;


        public MetalGearSolid4(IntPtr ipc, string id, string version) : base(ipc)
        {
            (m_base_address, m_camera_address) = id switch
            {
                // disc
                "BLES00246" or "BLJM67001" or "BLUS30109" when version is "02.00" => (0x559b24u, 0x559ac0u),
                // digital
                "NPUB31633" or "NPEB02182" or "NPJB00698" when version is "02.00" => (0x5496a4u, 0x549640u),
                _ => throw new NotImplementedException($"{nameof(MetalGearSolid4)} ['{id}', v'{version}'] is not implemented"),
            };
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            uint cameraChannel = m_camera_address + 0x60;
            uint flag = IPCUtils.ReadU32(m_ipc, cameraChannel);

            if ((flag & 0x00008000) == 0)
            {
                return;
            }

            uint pCameraCurrent = IPCUtils.ReadU32(m_ipc, cameraChannel + 0x10);
            IPCUtils.WriteFloat(m_ipc, pCameraCurrent + 0x188, 1.00f); // disable camera interpolation

            // shoulder
            uint pCameraShoulder = IPCUtils.ReadU32(m_ipc, m_camera_address + 0x18) + 0x170 + 0x4C;
            m_camera.Vert = IPCUtils.ReadFloat(m_ipc, pCameraShoulder);
            m_camera.Hor = IPCUtils.ReadFloat(m_ipc, pCameraShoulder + 0x4);
            m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, pCameraShoulder, m_camera.Vert);
            IPCUtils.WriteFloat(m_ipc, pCameraShoulder + 0x4, m_camera.Hor);

            // fpv
            uint pCameraFPV = IPCUtils.ReadU32(m_ipc, m_camera_address + 0x18) - 0x70;
            m_camera.Vert = IPCUtils.ReadFloat(m_ipc, pCameraFPV);
            m_camera.Hor = IPCUtils.ReadFloat(m_ipc, pCameraFPV + 0x4);
            m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, pCameraFPV, m_camera.Vert);
            IPCUtils.WriteFloat(m_ipc, pCameraFPV + 0x4, m_camera.Hor);

            // 3pp
            uint pCameraTPP = IPCUtils.ReadU32(m_ipc, m_base_address + 0x4);
            pCameraTPP = IPCUtils.ReadU32(m_ipc, pCameraTPP - 0x80) + 0x174;
            ushort hor = IPCUtils.ReadU16(m_ipc, pCameraTPP);
            ushort vert = IPCUtils.ReadU16(m_ipc, pCameraTPP + 0x8);
            hor = (ushort)(hor + (-diffX * SensModifier) * 7000);
            vert = (ushort)(vert + (diffY * SensModifier) * 7000);
            IPCUtils.WriteU16(m_ipc, pCameraTPP + 0x0, hor);
            IPCUtils.WriteU16(m_ipc, pCameraTPP + 0x8, vert);
        }
    }
}
