using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public class RatchetACiT : Game<HVVecCamera>
    {
        const uint m_base_address = 0xDE53C0;
        int? m_camera_slot_index = null;
        int? m_camera_id = null;
        HVVecCamera m_clank_camera = new HVVecCamera(4.6, -0.1, 0.97);
        HAVACamera m_ads_camera = new HAVACamera();

        public RatchetACiT(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (!m_camera_slot_index.HasValue || !m_camera_id.HasValue)
            {
                FindCameraSlot();
            }
            uint camera_id = IPCUtils.ReadU32(m_ipc, m_base_address + 0xC0 + 0xAC + (uint)m_camera_slot_index * 0x280);
            if (camera_id != 0x3 && camera_id != 0xf && camera_id != 0x15)
            {
                FindCameraSlot();
            }

            if (m_camera_slot_index.HasValue && m_camera_id.HasValue)
            {
                if (m_camera_id == 0x15)
                {
                    // Lock-strafe
                    uint address = m_base_address + 0xC0 + 0x120 + (uint)m_camera_slot_index * 0x280;

                    m_camera.X = IPCUtils.ReadFloat(m_ipc, address);
                    m_camera.Y = IPCUtils.ReadFloat(m_ipc, address + 4);
                    m_camera.Z = IPCUtils.ReadFloat(m_ipc, address + 8);
                    m_camera.Update(diffX * SensModifier, -diffY * SensModifier);
                    IPCUtils.WriteFloat(m_ipc, address, m_camera.X);
                    IPCUtils.WriteFloat(m_ipc, address + 4, m_camera.Y);
                    IPCUtils.WriteFloat(m_ipc, address + 8, m_camera.Z);
                }
                else if (m_camera_id == 0x3)
                {
                    // Clank
                    uint address = m_base_address + 0xC0 + 0x120 + (uint)m_camera_slot_index * 0x280;

                    m_clank_camera.X = IPCUtils.ReadFloat(m_ipc, address + 0x10);
                    m_clank_camera.Y = IPCUtils.ReadFloat(m_ipc, address + 0x14);
                    m_clank_camera.Z = IPCUtils.ReadFloat(m_ipc, address + 0x18);
                    m_clank_camera.Update(diffX * SensModifier, diffY * SensModifier);

                    // Comment these three out for industrial smoothing
                    IPCUtils.WriteFloat(m_ipc, address, m_clank_camera.X);
                    IPCUtils.WriteFloat(m_ipc, address + 4, m_clank_camera.Y);
                    IPCUtils.WriteFloat(m_ipc, address + 8, m_clank_camera.Z);

                    IPCUtils.WriteFloat(m_ipc, address + 0x10, m_clank_camera.X);
                    IPCUtils.WriteFloat(m_ipc, address + 0x14, m_clank_camera.Y);
                    IPCUtils.WriteFloat(m_ipc, address + 0x18, m_clank_camera.Z);
                }
                else if (m_camera_id == 0xf)
                {
                    // Aim down sight
                    uint address = m_base_address + 0xC0 + 0x144 + (uint)m_camera_slot_index * 0x280;

                    m_ads_camera.Hor = IPCUtils.ReadFloat(m_ipc, address);
                    m_ads_camera.Vert = IPCUtils.ReadFloat(m_ipc, address + 0x8);
                    m_ads_camera.Update(-diffX * SensModifier, diffY * SensModifier);
                    IPCUtils.WriteFloat(m_ipc, address, m_ads_camera.Hor);
                    IPCUtils.WriteFloat(m_ipc, address + 0x8, m_ads_camera.Vert);
                }
            }
        }

        private void FindCameraSlot()
        {
            for (int i = 0; i < 16; i++)
            {
                // The camera struct has 0xC0 bytes of header, then 16 camera slots of size 0x280, the camera id is at offset 0xAC
                uint camera_id = IPCUtils.ReadU32(m_ipc, m_base_address + 0xC0 + 0xAC + (uint)i * 0x280);
                if (camera_id == 0x3 || camera_id == 0xf || camera_id == 0x15)
                {
                    m_camera_slot_index = i;
                    m_camera_id = (int)camera_id;
                    break;
                }
            }
        }
    }
}
