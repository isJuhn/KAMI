using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public class MetalGearSolid4 : Game<HAVACamera>
    {
        uint m_camera_daemon;

        public MetalGearSolid4(IntPtr ipc, string id, string version) : base(ipc)
        {
            (m_camera_daemon) = id switch
            {
                // disc
                "BLES00246" or "BLJM67001" or "BLUS30109" when version is "02.00" => 0x559ac0u,
                // digital
                "NPUB31633" or "NPEB02182" or "NPJB00698" when version is "02.00" => 0x549640u,
                _ => throw new NotImplementedException($"{nameof(MetalGearSolid4)} ['{id}', v'{version}'] is not implemented"),
            };
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            uint pCameraChannel = m_camera_daemon + 0x60;
            uint pCameraCurrent = IPCUtils.ReadU32(m_ipc, pCameraChannel + 0x10);
            // the actual camera state (in training mode) is at 3641B3556.
            // need to find a way to access it for each stage.
            ushort flag = IPCUtils.ReadU16(m_ipc, m_camera_daemon + 0x7A);

            if ((flag & 0x0F00) == 0x0700)
            {
                if ((flag & 0x0001) == 0x1)
                {
                    uint pCameraShoulder = IPCUtils.ReadU32(m_ipc, IPCUtils.ReadU32(m_ipc, pCameraChannel + 0x10));
                    m_camera.Vert = IPCUtils.ReadFloat(m_ipc, pCameraShoulder + 0x15C);
                    m_camera.Hor = IPCUtils.ReadFloat(m_ipc, pCameraShoulder + 0x160);
                    m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
                    IPCUtils.WriteFloat(m_ipc, pCameraShoulder + 0x15C, m_camera.Vert);
                    IPCUtils.WriteFloat(m_ipc, pCameraShoulder + 0x160, m_camera.Hor);
                }
                else
                {
                    uint pCameraFPP = IPCUtils.ReadU32(m_ipc, pCameraChannel + 0x8);
                    m_camera.Vert = IPCUtils.ReadFloat(m_ipc, pCameraFPP - 0xD0);
                    m_camera.Hor = IPCUtils.ReadFloat(m_ipc, pCameraFPP - 0xCC);
                    m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
                    IPCUtils.WriteFloat(m_ipc, pCameraFPP - 0xD0, m_camera.Vert);
                    IPCUtils.WriteFloat(m_ipc, pCameraFPP - 0xCC, m_camera.Hor);
                }
            }
            else
            {
                if (pCameraCurrent != 0)
                {
                    IPCUtils.WriteFloat(m_ipc, pCameraCurrent + 0x188, 1.00f); // disable camera interpolation
                }

                uint pCameraTPP = IPCUtils.ReadU32(m_ipc, IPCUtils.ReadU32(m_ipc, pCameraChannel + 0x8));
                ushort hor = IPCUtils.ReadU16(m_ipc, pCameraTPP + 0x2C4);
                ushort vert = IPCUtils.ReadU16(m_ipc, pCameraTPP + 0x2CC);
                m_camera.Hor = (hor / 65536f) * (float)(2 * Math.PI);
                m_camera.Vert = (vert / 65536f) * (float)(2 * Math.PI);
                m_camera.Update(-diffX * SensModifier, diffY * SensModifier);
                IPCUtils.WriteU16(m_ipc, pCameraTPP + 0x2C4, (ushort)Math.Round(m_camera.Hor / (float)(2 * Math.PI) * 65536f));
                IPCUtils.WriteU16(m_ipc, pCameraTPP + 0x2CC, (ushort)Math.Round(m_camera.Vert / (float)(2 * Math.PI) * 65536f));
            }
        }
    }
}
