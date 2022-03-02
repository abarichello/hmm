using System;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Scene;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	public class CarCameraAdapter : IGameCamera, IGameCameraInversion, IGameCameraEngine, IGameCameraPlayerSetup
	{
		public bool SetTarget(string context, ICameraTarget target)
		{
			CarCamera singleton = CarCamera.Singleton;
			return singleton.SetTarget(context, target.Condition, target.TargetTransform, target.Mode, target.Snap, target.Follow, target.SmoothTeleport);
		}

		public void Enable()
		{
			CarCamera.Singleton.enabled = true;
		}

		public void Disable()
		{
			CarCamera.Singleton.enabled = false;
		}

		public bool IsEnabled()
		{
			return CarCamera.Singleton.enabled;
		}

		public void TriggerTeleport()
		{
			CarCamera.Singleton.Teleported = true;
		}

		public float CurrentFov
		{
			get
			{
				return CarCamera.Singleton.skyViewCamera.fov;
			}
			set
			{
				CarCamera.Singleton.skyViewCamera.fov = value;
			}
		}

		public void SetJitter(Vector3 jitter)
		{
			CarCamera.Singleton._SkyViewOffsetJitter = jitter;
		}

		public void Shake(float amount)
		{
			CarCamera.Singleton.Shake(amount);
		}

		public void SetCameraZoom(float zoom)
		{
			CarCamera.Singleton.CameraDynamicZoom = zoom;
		}

		public SkyViewFollowMode SkyViewFollowMouse
		{
			get
			{
				return CarCamera.Singleton.SkyViewFollowMouse;
			}
			set
			{
				CarCamera.Singleton.SkyViewFollowMouse = value;
			}
		}

		public void SnapToTarget()
		{
			CarCamera.Singleton.ForceSnappingToTarget();
		}

		public void SetPanLock(bool locked)
		{
			CarCamera.Singleton.LockPan = locked;
		}

		public bool CameraInverted
		{
			get
			{
				return CarCamera.Singleton.CameraInversionIsInverted;
			}
			set
			{
				CarCamera.Singleton.CameraInversionIsInverted = value;
			}
		}

		public void SetupArena(IGameArenaInfo config)
		{
			CarCamera singleton = CarCamera.Singleton;
			singleton.CameraInversionTeamAAngleY = (float)config.CameraInversionTeamAAngleY;
			singleton.CameraInversionTeamBAngleY = (float)config.CameraInversionTeamBAngleY;
		}

		public int PlayerCarId
		{
			get
			{
				return GameHubBehaviour.Hub.Players.CurrentPlayerData.GetPlayerCarObjectId();
			}
		}

		public Transform PlayerTransform
		{
			get
			{
				return CarCamera.Singleton.MyPlayerTransform;
			}
		}

		public bool IsAlive()
		{
			return CarCamera.Singleton.MyPlayerCombatData.IsAlive();
		}

		public bool IsPreSpawning()
		{
			SpawnController spawnController = CarCamera.Singleton.MyPlayerCombatData.Combat.SpawnController;
			return spawnController.State == SpawnStateKind.PreSpawned || spawnController.State == SpawnStateKind.Respawning;
		}

		public CombatData PlayerCombatData
		{
			get
			{
				return CarCamera.Singleton.MyPlayerCombatData;
			}
		}

		void IGameCameraPlayerSetup.Cleanup()
		{
		}

		public void SetupCurrentPlayer(Transform playerTransform, CombatData playerCombat)
		{
			CarCamera singleton = CarCamera.Singleton;
			singleton.MyPlayerTransform = playerTransform;
			singleton.MyPlayerCombatData = playerCombat;
			BaseCameraTarget baseCameraTarget = default(BaseCameraTarget);
			baseCameraTarget.TargetTransform = playerTransform;
			baseCameraTarget.Condition = (() => true);
			baseCameraTarget.Follow = true;
			baseCameraTarget.Mode = CarCameraMode.SkyView;
			baseCameraTarget.SmoothTeleport = true;
			baseCameraTarget.Snap = true;
			BaseCameraTarget baseCameraTarget2 = baseCameraTarget;
			this.SetTarget("PlayerSnap", baseCameraTarget2);
		}

		public void SetFocusTarget(CameraFocusTrigger focus)
		{
			CarCamera.Singleton.SkyViewFocusTarget = focus;
		}

		public CarCameraMode CameraMode
		{
			get
			{
				return CarCamera.Singleton.CameraMode;
			}
		}

		public Transform CurrentTargetTransform
		{
			get
			{
				return CarCamera.Singleton.CameraTargetTransform;
			}
		}

		public Vector3 CurrentTargetPosition
		{
			get
			{
				return CarCamera.Singleton.CurrentTargetPosition;
			}
		}

		public Transform CameraTransform
		{
			get
			{
				return CarCamera.Singleton.CameraTransform;
			}
		}

		public Camera UnityCamera
		{
			get
			{
				return CarCamera.Singleton.Camera;
			}
		}

		public float ScreenSpaceAngle
		{
			get
			{
				return -90f - CarCamera.Singleton.CameraInversionAngleY;
			}
		}

		public Vector3 InversionAngle
		{
			get
			{
				return new Vector3(0f, CarCamera.Singleton.CameraInversionAngleY, CarCamera.Singleton.CameraInversionAngle);
			}
		}
	}
}
