using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public class RatchetToD : Game<HVVecCamera>
    {
        const uint m_base_address = 0x101B0050;
        int? m_camera_slot_index = null;
        int? m_camera_id = null;
        HAVACamera m_secondary_camera = new HAVACamera();

        public RatchetToD(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (!m_camera_slot_index.HasValue || !m_camera_id.HasValue)
            {
                FindCameraSlot();
            }
            uint camera_id = IPCUtils.ReadU32(m_ipc, m_base_address + 0xC0 + 0x64 + (uint)m_camera_slot_index * 0x200);
            if (camera_id != 0x1a && camera_id != 0x10)
            {
                FindCameraSlot();
            }

            if (m_camera_slot_index.HasValue && m_camera_id.HasValue)
            {
                if (m_camera_id == 0x1a)
                {
                    // Lock-strafe
                    uint address = m_base_address + 0xC0 + 0xd0 + (uint)m_camera_slot_index * 0x200;

                    m_camera.X = IPCUtils.ReadFloat(m_ipc, address);
                    m_camera.Y = IPCUtils.ReadFloat(m_ipc, address + 4);
                    m_camera.Z = IPCUtils.ReadFloat(m_ipc, address + 8);
                    m_camera.Update(diffX * SensModifier, -diffY * SensModifier);
                    IPCUtils.WriteFloat(m_ipc, address, m_camera.X);
                    IPCUtils.WriteFloat(m_ipc, address + 4, m_camera.Y);
                    IPCUtils.WriteFloat(m_ipc, address + 8, m_camera.Z);
                }
                else if (m_camera_id == 0x10)
                {
                    // Aim down sight
                    uint address = m_base_address + 0xC0 + 0xf4 + (uint)m_camera_slot_index * 0x200;

                    m_secondary_camera.Hor = IPCUtils.ReadFloat(m_ipc, address);
                    m_secondary_camera.Vert = IPCUtils.ReadFloat(m_ipc, address + 0x8);
                    m_secondary_camera.Update(-diffX * SensModifier, diffY * SensModifier);
                    IPCUtils.WriteFloat(m_ipc, address, m_secondary_camera.Hor);
                    IPCUtils.WriteFloat(m_ipc, address + 0x8, m_secondary_camera.Vert);
                }
            }
        }

        private void FindCameraSlot()
        {
            for (int i = 0; i < 16; i++)
            {
                // The camera struct has 0xC0 bytes of header, then 16 camera slots of size 0x200, the camera id is at offset 0x64
                uint camera_id = IPCUtils.ReadU32(m_ipc, m_base_address + 0xC0 + 0x64 + (uint)i * 0x200);
                if (camera_id == 0x1a || camera_id == 0x10)
                {
                    m_camera_slot_index = i;
                    m_camera_id = (int)camera_id;
                    break;
                }
            }
        }
    }
}
