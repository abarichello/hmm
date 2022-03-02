using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class CameraController : GameHubBehaviour
	{
		private void OnEnable()
		{
			for (int i = 0; i < this.behaviourList.Length; i++)
			{
				this.behaviourList[i].baseBehaviour = (CameraController.CameraBehaviourTypes)i;
			}
			this.priorizedBehaviourList = new List<CameraController.CameraBehaviour>(this.behaviourList);
			this.priorizedBehaviourList.Sort(new Comparison<CameraController.CameraBehaviour>(this.SortBehaviours));
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				Object.Destroy(base.gameObject);
				return;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData != null && GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance != null)
			{
				this.LinkWithPlayerCar(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance);
			}
			else
			{
				GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.OnPlayerSpawn;
			}
			GameHubBehaviour.Hub.BombManager.OnSlowMotionToggled += this.OnSlowMotion;
			this._sqrMaxDistanceToActivateZoom = GameHubBehaviour.Hub.BombManager.Rules.MaxDistanceToActivateZoom * GameHubBehaviour.Hub.BombManager.Rules.MaxDistanceToActivateZoom;
		}

		private int SortBehaviours(CameraController.CameraBehaviour x, CameraController.CameraBehaviour y)
		{
			return y.priority.CompareTo(x.priority);
		}

		private void OnDisable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.OnSlowMotionToggled -= this.OnSlowMotion;
		}

		private void OnSlowMotion(bool enable)
		{
			this.slowMotion = enable;
			Transform transform = (!SpectatorController.IsSpectating) ? GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.transform : this._gameCameraEngine.CurrentTargetTransform;
			float sqrMagnitude = (GameHubBehaviour.Hub.BombManager.BombMovement.Combat.transform.position - transform.position).sqrMagnitude;
			this._isInsideDistanceToZoom = (sqrMagnitude < (float)this._sqrMaxDistanceToActivateZoom);
		}

		private void LinkWithPlayerCar(Identifiable ident)
		{
			this.carMovement = ident.GetBitComponent<CarMovement>();
			this.combatObject = this.carMovement.Combat;
			this.combatObject.CustomGadget2.ClientListenToEffectStarted += this.OnUltimateUsed;
		}

		private void OnUltimateUsed(BaseFX obj)
		{
			this.lastTimeUltimateUsed = Time.time;
		}

		private void Update()
		{
			if (this.cameraLockZoomUpdate > 0f)
			{
				this.cameraLockZoomUpdate -= Time.unscaledDeltaTime;
				return;
			}
			if (this.carMovement == null || this._gameCamera == null)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < this.priorizedBehaviourList.Count; i++)
			{
				CameraController.CameraBehaviour cameraBehaviour = this.priorizedBehaviourList[i];
				switch (cameraBehaviour.baseBehaviour)
				{
				case CameraController.CameraBehaviourTypes.BombDelivery:
					if (this.slowMotion && this._isInsideDistanceToZoom)
					{
						this._gameCamera.SetCameraZoom(cameraBehaviour.CameraZoom);
						BaseCameraTarget baseCameraTarget = default(BaseCameraTarget);
						baseCameraTarget.TargetTransform = GameHubBehaviour.Hub.BombManager.BombMovement.Combat.transform;
						baseCameraTarget.Condition = (() => GameHubBehaviour.Hub.BombManager.SlowMotionEnabled);
						baseCameraTarget.Mode = CarCameraMode.SkyView;
						baseCameraTarget.Snap = false;
						baseCameraTarget.Follow = true;
						baseCameraTarget.SmoothTeleport = false;
						BaseCameraTarget baseCameraTarget2 = baseCameraTarget;
						this._gameCamera.SetTarget("SlowMotion", baseCameraTarget2);
						flag = true;
					}
					break;
				case CameraController.CameraBehaviourTypes.DeadPlayer:
					if (!this.carMovement.Combat.IsAlive() && this._gameCameraEngine.CurrentTargetTransform == null)
					{
						this._gameCamera.SetCameraZoom(cameraBehaviour.CameraZoom);
						flag = true;
					}
					break;
				case CameraController.CameraBehaviourTypes.Ultimate:
					if (Time.time - this.lastTimeUltimateUsed < cameraBehaviour.minDuration)
					{
						this._gameCamera.SetCameraZoom(cameraBehaviour.CameraZoom);
						this.cameraLockZoomUpdate = cameraBehaviour.minDuration - (Time.time - this.lastTimeUltimateUsed);
						flag = true;
					}
					break;
				case CameraController.CameraBehaviourTypes.PlayerSpawn:
					if (this.isSpawning)
					{
						this._gameCamera.SetCameraZoom(cameraBehaviour.CameraZoom);
						flag = true;
					}
					break;
				}
				if (flag)
				{
					this.cameraLockZoomUpdate = cameraBehaviour.minDuration;
					break;
				}
			}
			if (!flag)
			{
				float cameraZoom = Mathf.Lerp(this.MinSpeedCameraZoom, this.MaxSpeedCameraZoom, Mathf.Clamp01(this.carMovement.SpeedZ / this.carMovement.MaxLinearSpeed));
				this._gameCamera.SetCameraZoom(cameraZoom);
			}
			this._wasDone = flag;
		}

		private void OnPlayerSpawn(PlayerEvent data)
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId != data.TargetId)
			{
				return;
			}
			this.LinkWithPlayerCar(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance);
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.OnPlayerSpawn;
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectSpawn -= this.OnPlayerSpawn;
		}

		[InjectOnClient]
		private IGameCamera _gameCamera;

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		private CarMovement carMovement;

		private bool slowMotion;

		public CameraController.CameraBehaviour[] behaviourList;

		public List<CameraController.CameraBehaviour> priorizedBehaviourList;

		[Range(-0.5f, 1f)]
		public float MaxSpeedCameraZoom = -0.1f;

		[Range(-0.5f, 1f)]
		public float MinSpeedCameraZoom = 0.1f;

		private CombatObject combatObject;

		private float lastTimeUltimateUsed;

		private float cameraLockZoomUpdate;

		public bool isSpawning;

		private bool _isInsideDistanceToZoom;

		private int _sqrMaxDistanceToActivateZoom;

		private bool _wasDone;

		public enum CameraBehaviourTypes
		{
			BombDelivery,
			DeadPlayer,
			Ultimate,
			Boost,
			PlayerSpawn
		}

		[Serializable]
		public class CameraBehaviour
		{
			[NonSerialized]
			public CameraController.CameraBehaviourTypes baseBehaviour;

			public int priority;

			public float minDuration;

			public float CameraZoom;
		}
	}
}
