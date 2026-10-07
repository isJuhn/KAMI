using KAMI.Core.Cameras;
using System;
using System.Diagnostics;

namespace KAMI.Core.Games.Battlefield
{
    public class BadCompany : Game<HAVACamera>
    {
        enum ControlState
        {
            None,
            OnFoot,
            Vehicle,
        }

        // The FOVs are vertical, in degrees
        const float AspectRatio = 16f / 9f;
        // Once the mouse has been left alone for this long the vehicle camera is handed back to the game, which centers it
        static readonly TimeSpan VehicleCameraRecenterDelay = TimeSpan.FromSeconds(2);
        // Searching a vehicle for a turret takes hundreds of reads, vehicles without one are only searched again after this
        static readonly TimeSpan TurretSearchInterval = TimeSpan.FromSeconds(1);

        readonly Addresses m_addresses;
        Turret m_turret;
        uint m_turretlessVehicle;
        readonly Stopwatch m_turretSearch = new Stopwatch();
        VehicleCamera m_vehicleCamera;
        readonly Stopwatch m_mouseIdle = new Stopwatch();

        public BadCompany(IntPtr ipc, string serialId) : base(ipc)
        {
            m_addresses = Addresses.ForSerial(serialId);
        }

        // Mirrors the game's own check, the soldier is only aimed directly when the player controls the soldier itself
        ControlState GetControlState(out uint controlled)
        {
            controlled = 0;
            uint playerManager = IPCUtils.ReadU32(m_ipc, m_addresses.PlayerManager);
            if (playerManager == 0)
            {
                return ControlState.None;
            }
            uint player = IPCUtils.ReadU32(m_ipc, playerManager + Offsets.PlayerManager.LocalPlayer);
            if (player == 0)
            {
                return ControlState.None;
            }
            controlled = IPCUtils.ReadU32(m_ipc, player + Offsets.Player.ControlledEntity);
            uint soldierHandle = IPCUtils.ReadU32(m_ipc, player + Offsets.Player.SoldierHandle);
            if (controlled == 0 || soldierHandle == 0 || IPCUtils.Error != PineIPC.IPCStatus.Success)
            {
                return ControlState.None;
            }
            if (controlled == IPCUtils.ReadU32(m_ipc, soldierHandle))
            {
                return ControlState.OnFoot;
            }
            if (IPCUtils.ReadU32(m_ipc, controlled) == m_addresses.ClientVehicleVtable)
            {
                return ControlState.Vehicle;
            }
            return ControlState.None;
        }

        // Every weapon slot has its own aimer with its own yaw/pitch, so the active weapon has to be looked up each time
        uint GetAimState(uint soldier)
        {
            uint weaponsBegin = IPCUtils.ReadU32(m_ipc, soldier + Offsets.ClientSoldierEntity.WeaponsBegin);
            uint weaponCount = (IPCUtils.ReadU32(m_ipc, soldier + Offsets.ClientSoldierEntity.WeaponsEnd) - weaponsBegin) / 4;
            uint weaponIndex = IPCUtils.ReadU32(m_ipc, IPCUtils.ReadU32(m_ipc, soldier + Offsets.ClientSoldierEntity.WeaponSelector) + 4);
            uint weapon = weaponIndex < weaponCount ? IPCUtils.ReadU32(m_ipc, weaponsBegin + weaponIndex * 4) : 0;
            if (weapon == 0)
            {
                // The game falls back to the first slot when the selected one is empty
                weapon = IPCUtils.ReadU32(m_ipc, weaponsBegin);
            }
            if (weapon == 0)
            {
                return 0;
            }
            uint aimer = IPCUtils.ReadU32(m_ipc, weapon + Offsets.Weapon.Aimer);
            if (aimer == 0)
            {
                return 0;
            }
            uint aimState = IPCUtils.ReadU32(m_ipc, aimer + Offsets.Aimer.AimState);
            if (IPCUtils.Error != PineIPC.IPCStatus.Success)
            {
                return 0;
            }
            return aimState;
        }

        static float HalfHorizontalFov(float verticalFovDegrees)
        {
            return MathF.Atan(MathF.Tan(verticalFovDegrees * MathF.PI / 360) * AspectRatio);
        }

        // Scales the sensitivity so that moving the mouse far enough to turn to the edge of the screen at the
        // reference FOV turns to the edge of the screen at the current FOV as well
        float GetFovSensitivityScale()
        {
            uint parameters = IPCUtils.ReadU32(m_ipc, m_addresses.Camera + Offsets.Camera.FovParameters);
            if (parameters == 0 || IPCUtils.Error != PineIPC.IPCStatus.Success)
            {
                return 1;
            }
            float fov = IPCUtils.ReadFloat(m_ipc, parameters + Offsets.CameraFovParameters.CurrentFov);
            float referenceFov = IPCUtils.ReadFloat(m_ipc, m_addresses.Camera + Offsets.Camera.ReferenceFov);
            if (IPCUtils.Error != PineIPC.IPCStatus.Success || !(fov > 1 && fov < 170) || !(referenceFov > 1 && referenceFov < 170))
            {
                return 1;
            }
            return HalfHorizontalFov(fov) / HalfHorizontalFov(referenceFov);
        }

        void UpdateOnFoot(uint soldier, int diffX, int diffY)
        {
            uint aimState = GetAimState(soldier);
            if (aimState == 0)
            {
                return;
            }
            m_camera.Hor = IPCUtils.ReadFloat(m_ipc, aimState + Offsets.AimState.Yaw);
            m_camera.Vert = IPCUtils.ReadFloat(m_ipc, aimState + Offsets.AimState.Pitch);
            float sensitivity = SensModifier * GetFovSensitivityScale();
            m_camera.Update(diffX * sensitivity, -diffY * sensitivity);
            IPCUtils.WriteFloat(m_ipc, aimState + Offsets.AimState.Yaw, m_camera.Hor);
            IPCUtils.WriteFloat(m_ipc, aimState + Offsets.AimState.Pitch, m_camera.Vert);
        }

        // Returns whether the local player controls a turret in the vehicle
        bool UpdateTurret(uint clientVehicle, int diffX, int diffY)
        {
            if (m_turret == null || m_turret.ClientVehicle != clientVehicle || !m_turret.IsAlive(m_ipc, m_addresses))
            {
                if (clientVehicle == m_turretlessVehicle && m_turretSearch.Elapsed < TurretSearchInterval)
                {
                    return false;
                }
                m_turret = Turret.Find(m_ipc, m_addresses, clientVehicle);
                if (m_turret == null)
                {
                    m_turretlessVehicle = clientVehicle;
                    m_turretSearch.Restart();
                    return false;
                }
            }
            // Leave the turret alone unless the game itself has handed the local player control of it
            uint yawNetState = m_turret.Yaw?.GetControlledNetState(m_ipc, m_addresses) ?? 0;
            uint pitchNetState = m_turret.Pitch?.GetControlledNetState(m_ipc, m_addresses) ?? 0;
            if (yawNetState == 0 && pitchNetState == 0)
            {
                return false;
            }
            // Turning right and looking up both decrease the body angles
            m_camera.Hor = m_turret.Yaw != null ? -m_turret.Yaw.ReadAngle(m_ipc) : 0;
            m_camera.Vert = m_turret.Pitch != null ? -m_turret.Pitch.ReadAngle(m_ipc) : 0;
            if (IPCUtils.Error != PineIPC.IPCStatus.Success)
            {
                m_turret = null;
                return false;
            }
            float sensitivity = SensModifier * GetFovSensitivityScale();
            m_camera.Update(diffX * sensitivity, -diffY * sensitivity);
            if (yawNetState != 0)
            {
                m_turret.Yaw.WriteAngle(m_ipc, yawNetState, -m_camera.Hor);
            }
            if (pitchNetState != 0)
            {
                m_turret.Pitch.WriteAngle(m_ipc, pitchNetState, -m_camera.Vert);
            }
            return true;
        }

        // Returns whether the vehicle camera is being held. Its pitch is fixed, so only the yaw is controlled.
        bool UpdateVehicleCamera(int diffX)
        {
            if (!m_mouseIdle.IsRunning || m_mouseIdle.Elapsed > VehicleCameraRecenterDelay)
            {
                return false;
            }
            if (m_vehicleCamera == null || !m_vehicleCamera.IsActive(m_ipc, m_addresses))
            {
                ReleaseVehicleCamera();
                m_vehicleCamera = VehicleCamera.ReadActive(m_ipc, m_addresses);
                if (m_vehicleCamera == null)
                {
                    return false;
                }
                // Pick up wherever the stick or the game's own centering left it
                m_camera.Hor = m_vehicleCamera.ReadAngle(m_ipc);
            }
            float sensitivity = SensModifier * GetFovSensitivityScale();
            m_camera.Update(diffX * sensitivity, 0);
            m_camera.Hor = Math.Clamp(m_camera.Hor, -m_vehicleCamera.MaxLeftAngle, m_vehicleCamera.MaxRightAngle);
            m_vehicleCamera.HoldAngle(m_ipc, m_camera.Hor);
            return true;
        }

        void ReleaseVehicleCamera()
        {
            if (m_vehicleCamera != null && m_vehicleCamera.IsAlive(m_ipc, m_addresses))
            {
                m_vehicleCamera.Release(m_ipc);
            }
            m_vehicleCamera = null;
        }

        public override void UpdateCamera(int diffX, int diffY)
        {
            if (diffX != 0 || diffY != 0)
            {
                m_mouseIdle.Restart();
            }
            bool holdingVehicleCamera = false;
            switch (GetControlState(out uint controlled))
            {
                case ControlState.OnFoot:
                    UpdateOnFoot(controlled, diffX, diffY);
                    break;
                case ControlState.Vehicle:
                    holdingVehicleCamera = !UpdateTurret(controlled, diffX, diffY) && UpdateVehicleCamera(diffX);
                    break;
            }
            if (!holdingVehicleCamera)
            {
                ReleaseVehicleCamera();
            }
        }

        public override void InjectionStop()
        {
            ReleaseVehicleCamera();
        }
    }
}
