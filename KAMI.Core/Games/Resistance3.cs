﻿using KAMI.Cameras;
using KAMI.Utilities;
using System;

namespace KAMI.Games
{
    public class Resistance3 : Game<HVVecCamera>
    {
        DerefChain m_addr;

        public Resistance3(IntPtr ipc) : base(ipc)
        {
            m_addr = DerefChain.CreateDerefChain(ipc, 0x11e09d0 + 0x98, 0x390);
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            m_camera.X = IPCUtils.ReadFloat(m_ipc, (uint)m_addr.Value);
            m_camera.Y = IPCUtils.ReadFloat(m_ipc, (uint)(m_addr.Value + 4));
            m_camera.Z = IPCUtils.ReadFloat(m_ipc, (uint)(m_addr.Value + 8));
            m_camera.Update(diffX * SensModifier, -diffY * SensModifier);
            IPCUtils.WriteFloat(m_ipc, (uint)m_addr.Value, m_camera.X);
            IPCUtils.WriteFloat(m_ipc, (uint)(m_addr.Value + 4), m_camera.Y);
            IPCUtils.WriteFloat(m_ipc, (uint)(m_addr.Value + 8), m_camera.Z);
        }
    }
}
