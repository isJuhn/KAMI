using System;

namespace KAMI.Core.Games
{
    public class Ratchet3PS2 : RatchetOGBase
    {
        private uint m_addressScoped;
        private bool isSinglePlayer;

        public Ratchet3PS2(IntPtr ipc) : base(ipc)
        {
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            uint mp_map_id = IPCUtils.ReadU32(m_ipc, 0x001F8528);
            isSinglePlayer = mp_map_id < 40;
            if (isSinglePlayer) // must be SP
            {
                m_addressHor = 0x1A6160;
                m_addressVert = 0x1A6180;
                m_addressScoped = 0x1A71C5;
            }
            else
            {
                switch (mp_map_id)
                {
                    case 40://Bakisi
                        m_addressHor = 0x300AA0;
                        m_addressVert = 0x300AC0;
                        m_addressScoped = 0x3012CA;
                        break;

                    case 41://Hoven
                        m_addressHor = 0x300BE0;
                        m_addressVert = 0x300C00;
                        m_addressScoped = 0x30140A;
                        break;

                    case 42://X12
                        m_addressHor = 0x2F8AE0;
                        m_addressVert = 0x2F8B00;
                        m_addressScoped = 0x2F930A;
                        break;

                    case 43://Korgon
                        m_addressHor = 0x2F8960;
                        m_addressVert = 0x2F8980;
                        m_addressScoped = 0x2F918A;
                        break;

                    case 44://Metro
                        m_addressHor = 0x2F89A0;
                        m_addressVert = 0x2F89C0;
                        m_addressScoped = 0x2F91CA;
                        break;

                    case 45://BWC
                        m_addressHor = 0x2F8960;
                        m_addressVert = 0x2F8980;
                        m_addressScoped = 0x2F918A;
                        break;

                    case 46://CC
                        m_addressHor = 0x309520;
                        m_addressVert = 0x309540;
                        m_addressScoped = 0x309CCA;
                        break;

                    case 47://Dox
                        m_addressHor = 0x309660;
                        m_addressVert = 0x309680;
                        m_addressScoped = 0x30A0AA;
                        break;

                    case 48://Sewers
                        m_addressHor = 0x3096A0;
                        m_addressVert = 0x3096C0;
                        m_addressScoped = 0x30A0EA;
                        break;

                    case 49://Marcadia
                        m_addressHor = 0x309620;
                        m_addressVert = 0x309640;
                        m_addressScoped = 0x309E4A;
                        break;
                }
            }

            m_camera.Hor = IPCUtils.ReadFloat(m_ipc, m_addressHor);
            m_camera.Vert = IPCUtils.ReadFloat(m_ipc, m_addressVert);

            bool isScoped = IPCUtils.ReadU8(m_ipc, m_addressScoped) != 0;
            float horDiff = -diffX * SensModifier;
            float vertDiff = diffY * SensModifier;

            if (isScoped)
            {
                horDiff *= ScopedSensModifier;
                vertDiff *= ScopedSensModifier;
            }

            m_camera.Update(horDiff, vertDiff);

            IPCUtils.WriteFloat(m_ipc, m_addressHor, m_camera.Hor);
            IPCUtils.WriteFloat(m_ipc, m_addressVert, m_camera.Vert);

            // Gravity-ramp directional camera update using character up vector
            // Applied to both single player and multiplayer
            // Only update if there's actual input to avoid amplifying memory jitter
            if (horDiff != 0f || vertDiff != 0f)
            {
                UpdateGravityRampCamera(horDiff, vertDiff);
            }
        }

        private void UpdateGravityRampCamera(float horDiff, float vertDiff)
        {
            // Read current gravity camera direction (unit-ish vector)
            float camX = IPCUtils.ReadFloat(m_ipc, m_addressHor + 0xB0);
            float camY = IPCUtils.ReadFloat(m_ipc, m_addressHor + 0xB4);
            float camZ = IPCUtils.ReadFloat(m_ipc, m_addressHor + 0xB8);

            // Read character up vector U = (Ux, Uy, Uz)
            float Ux;
            float Uy;
            float Uz;
            if (isSinglePlayer)
            {
                Ux = IPCUtils.ReadFloat(m_ipc, m_addressHor - 0xC8);
                Uy = IPCUtils.ReadFloat(m_ipc, m_addressHor - 0xB8);
                Uz = IPCUtils.ReadFloat(m_ipc, m_addressHor - 0xD8);
            }
            else
            {
                Ux = IPCUtils.ReadFloat(m_ipc, m_addressHor - 0xA8);
                Uy = IPCUtils.ReadFloat(m_ipc, m_addressHor - 0x98);
                Uz = IPCUtils.ReadFloat(m_ipc, m_addressHor - 0xB8);
            }

            // if up vector is NaN or extremely large, skip update
            if (float.IsNaN(Ux) || float.IsNaN(Uy) || float.IsNaN(Uz) ||
                float.IsInfinity(Ux) || float.IsInfinity(Uy) || float.IsInfinity(Uz))
            {
                return;
            }

            // Normalize U
            float uLen = MathF.Sqrt(Ux * Ux + Uy * Uy + Uz * Uz);
            if (uLen < 1e-4f)
            {
                Ux = 0f; Uy = 0f; Uz = 1f;
            }
            else
            {
                Ux /= uLen; Uy /= uLen; Uz /= uLen;
            }

            // Normalize current direction D
            float dLen = MathF.Sqrt(camX * camX + camY * camY + camZ * camZ);
            if (dLen < 1e-4f)
            {
                camX = 1f; camY = 0f; camZ = 0f;
            }
            else
            {
                camX /= dLen; camY /= dLen; camZ /= dLen;
            }

            // 1) Yaw around up vector U by horDiff
            var (yawX, yawY, yawZ) = RotateAroundAxis(
                camX, camY, camZ,
                Ux, Uy, Uz,
                horDiff);

            // Recompute right after yaw
            float Rx = Uy * yawZ - Uz * yawY;
            float Ry = Uz * yawX - Ux * yawZ;
            float Rz = Ux * yawY - Uy * yawX;
            float rLen = MathF.Sqrt(Rx * Rx + Ry * Ry + Rz * Rz);
            if (rLen < 1e-4f)
            {
                // Degenerate (looking straight up/down) – pick any right perpendicular to U
                Rx = 1f; Ry = 0f; Rz = 0f;
            }
            else
            {
                Rx /= rLen; Ry /= rLen; Rz /= rLen;
            }

            // 2) Pitch around right vector R by vertDiff (mouse down looks down)
            var (pitchX, pitchY, pitchZ) = RotateAroundAxis(
                yawX, yawY, yawZ,
                Rx, Ry, Rz,
                vertDiff);

            // if result is NaN or infinite, skip write
            if (float.IsNaN(pitchX) || float.IsNaN(pitchY) || float.IsNaN(pitchZ) ||
                float.IsInfinity(pitchX) || float.IsInfinity(pitchY) || float.IsInfinity(pitchZ))
            {
                return;
            }

            // Write back updated gravity camera direction
            IPCUtils.WriteFloat(m_ipc, m_addressHor + 0xB0, pitchX);
            IPCUtils.WriteFloat(m_ipc, m_addressHor + 0xB4, pitchY);
            IPCUtils.WriteFloat(m_ipc, m_addressHor + 0xB8, pitchZ);
        }

        private static (float x, float y, float z) RotateAroundAxis(
            float vx, float vy, float vz,
            float ax, float ay, float az,
            float angle)
        {
            float c = MathF.Cos(angle);
            float s = MathF.Sin(angle);

            // v_parallel = (v·a)a
            float dot = vx * ax + vy * ay + vz * az;
            float px = ax * dot;
            float py = ay * dot;
            float pz = az * dot;

            // v_perp = v - v_parallel
            float wx = vx - px;
            float wy = vy - py;
            float wz = vz - pz;

            // a x v_perp
            float cx = ay * wz - az * wy;
            float cy = az * wx - ax * wz;
            float cz = ax * wy - ay * wx;

            // v' = v_parallel + v_perp * c + (a x v_perp) * s
            float rx = px + wx * c + cx * s;
            float ry = py + wy * c + cy * s;
            float rz = pz + wz * c + cz * s;
            return (rx, ry, rz);
        }
    }
}
