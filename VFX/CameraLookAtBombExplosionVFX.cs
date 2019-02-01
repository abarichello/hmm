using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.VFX
{
	public class CameraLookAtBombExplosionVFX : BaseVFX
	{
		protected override void OnActivate()
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay)
			{
				CarCamera.Singleton.SetTarget("BombExplosionReplay", delegate()
				{
					bool flag = LogoTransition.IsPlaying() && !LogoTransition.HasTriggeredMiddleEvent();
					bool flag2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay;
					return flag || flag2;
				}, base.transform, false, false, false);
			}
			else
			{
				CarCamera.Singleton.SetTarget("BombExplosion", delegate()
				{
					BombScoreBoard.State currentState = GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState;
					return currentState == BombScoreBoard.State.BombDelivery || currentState == BombScoreBoard.State.PreReplay;
				}, base.transform, false, false, false);
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}
	}
}
