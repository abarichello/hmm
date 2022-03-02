using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public class BombScoreCameraBehaviourAdapter : GameHubObject, IBombScoreCameraBehaviour
	{
		public void LookAtExplosion(Transform explosionTransform)
		{
			BaseCameraTarget baseCameraTarget = new BaseCameraTarget
			{
				TargetTransform = explosionTransform,
				Mode = CarCameraMode.SkyView,
				Follow = false,
				Snap = false,
				SmoothTeleport = false
			};
			if (GameHubObject.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.Replay)
			{
				baseCameraTarget.Condition = delegate()
				{
					bool flag = LogoTransition.IsPlaying() && !LogoTransition.HasTriggeredMiddleEvent();
					bool flag2 = GameHubObject.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.Replay;
					return flag || flag2;
				};
				this._gameCamera.SetTarget("BombExplosionReplay", baseCameraTarget);
			}
			else
			{
				baseCameraTarget.Condition = delegate()
				{
					BombScoreboardState currentState = GameHubObject.Hub.BombManager.ScoreBoard.CurrentState;
					return currentState == BombScoreboardState.BombDelivery || currentState == BombScoreboardState.PreReplay;
				};
				this._gameCamera.SetTarget("BombExplosion", baseCameraTarget);
			}
		}

		public void FollowBomb(Transform bombTransform)
		{
			BaseCameraTarget baseCameraTarget = default(BaseCameraTarget);
			baseCameraTarget.TargetTransform = bombTransform;
			baseCameraTarget.Condition = (() => GameHubObject.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.Replay && GameHubObject.Hub.BombManager.ActiveBomb.IsSpawned);
			baseCameraTarget.Mode = CarCameraMode.SkyView;
			baseCameraTarget.Snap = true;
			baseCameraTarget.Follow = true;
			baseCameraTarget.SmoothTeleport = false;
			BaseCameraTarget baseCameraTarget2 = baseCameraTarget;
			this._gameCamera.SetPanLock(true);
			this._gameCamera.SetTarget("Replay", baseCameraTarget2);
		}

		public void StopBehaviour()
		{
			this._gameCamera.SetPanLock(false);
		}

		[InjectOnClient]
		private IGameCamera _gameCamera;
	}
}
