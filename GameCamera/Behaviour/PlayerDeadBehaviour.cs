using System;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.GameCamera.Movement;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public class PlayerDeadBehaviour : BaseCameraBehaviour, IPlayerDeadBehaviour
	{
		[Inject]
		private void Init(IGameTime gameTime, IBombManager bombManager, IScoreBoard scoreBoard, SkyViewMovement skyViewMovement, IGameCameraPlayerSetup cameraPlayer)
		{
			this._gameTime = gameTime;
			this._bombManager = bombManager;
			this._scoreBoard = scoreBoard;
			this._currentMovement = skyViewMovement;
			this._cameraPlayer = cameraPlayer;
			this._stateData.CurrentState = PlayerDeadBehaviour.State.Inactive;
			this._stateData.Follow = true;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.OnPlayerUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning += this.OnPlayerRespawning;
			base.SnapToTarget();
		}

		~PlayerDeadBehaviour()
		{
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.OnPlayerUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning -= this.OnPlayerRespawning;
		}

		public override bool IsActive
		{
			get
			{
				return this._stateData.CurrentState != PlayerDeadBehaviour.State.Inactive;
			}
		}

		public override void BeforeUpdate(ref CameraState state)
		{
			this.CheckCurrentState();
		}

		public override ICameraMovement CurrentMovement
		{
			get
			{
				return this._currentMovement;
			}
		}

		public override Transform CurrentTargetTransform
		{
			get
			{
				return this._stateData.Target;
			}
		}

		public override Vector3 CurrentTargetPosition
		{
			get
			{
				return this._stateData.Target.position;
			}
		}

		public override bool LockPan
		{
			get
			{
				return false;
			}
		}

		public override bool FollowTarget
		{
			get
			{
				return this._stateData.Follow;
			}
		}

		public override bool SmoothTeleport
		{
			get
			{
				return false;
			}
		}

		public void SetupArena(IGameArenaInfo arenaInfo)
		{
			this._killCamWaitTime = arenaInfo.KillCamWaitTimeSeconds;
		}

		private void CheckCurrentState()
		{
			if (this._scoreBoard.CurrentState != BombScoreboardState.BombDelivery || this._cameraPlayer.IsAlive())
			{
				this.StopBehaviour();
				return;
			}
			PlayerDeadBehaviour.State currentState = this._stateData.CurrentState;
			if (currentState != PlayerDeadBehaviour.State.WatchingPlayerDeath)
			{
				if (currentState == PlayerDeadBehaviour.State.FocusBomb)
				{
					if (this._cameraPlayer.IsPreSpawning())
					{
						this._currentMovement.PlayerPreSpawing = true;
					}
				}
			}
			else if (this._gameTime.GetPlaybackTime() > this._stateData.EndTime)
			{
				this.FocusBomb();
			}
		}

		private void OnPlayerUnspawn(PlayerEvent data)
		{
			if (this._scoreBoard.CurrentState != BombScoreboardState.BombDelivery || data.TargetId != this._cameraPlayer.PlayerCarId)
			{
				return;
			}
			this._stateData.CurrentState = PlayerDeadBehaviour.State.WatchingPlayerDeath;
			this._stateData.Target = this._cameraPlayer.PlayerTransform;
			this._stateData.Follow = false;
			this._stateData.EndTime = data.EventTime + (int)(1000f * this._killCamWaitTime);
			this._currentMovement.PlayerPreSpawing = false;
			base.InterpolateToTarget();
		}

		private void FocusBomb()
		{
			this._stateData.CurrentState = PlayerDeadBehaviour.State.FocusBomb;
			this._stateData.Target = this._bombManager.GetBombTransform();
			this._stateData.Follow = true;
			this._stateData.EndTime = 0;
			this._currentMovement.PlayerPreSpawing = false;
			base.InterpolateToTarget();
		}

		private void OnPlayerRespawning(PlayerEvent data)
		{
			if (this._scoreBoard.CurrentState != BombScoreboardState.BombDelivery || data.TargetId != this._cameraPlayer.PlayerCarId)
			{
				return;
			}
			this._stateData.CurrentState = PlayerDeadBehaviour.State.WatchingPlayerRespawn;
			this._stateData.Target = this._cameraPlayer.PlayerTransform;
			this._stateData.Follow = true;
			this._stateData.EndTime = 0;
			this._currentMovement.PlayerPreSpawing = false;
			base.InterpolateToTarget();
		}

		private void StopBehaviour()
		{
			this._stateData.CurrentState = PlayerDeadBehaviour.State.Inactive;
			this._stateData.Target = null;
			this._stateData.Follow = false;
			this._stateData.EndTime = 0;
			this._currentMovement.PlayerPreSpawing = false;
		}

		private IGameTime _gameTime;

		private IBombManager _bombManager;

		private IScoreBoard _scoreBoard;

		private SkyViewMovement _currentMovement;

		private IGameCameraPlayerSetup _cameraPlayer;

		private float _killCamWaitTime;

		private PlayerDeadBehaviour.StateData _stateData;

		private enum State
		{
			Inactive,
			WatchingPlayerDeath,
			FocusBomb,
			WatchingPlayerRespawn
		}

		private struct StateData
		{
			public PlayerDeadBehaviour.State CurrentState;

			public Transform Target;

			public bool Follow;

			public int EndTime;
		}
	}
}
