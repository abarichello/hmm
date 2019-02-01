using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudCenterRespawn : HudRespawnController
	{
		protected override void OnPlayerUnspawned(PlayerEvent data)
		{
			if (!this.Configured || this.BombAlreadyDelivered || data.TargetId != this.Combat.Player.PlayerCarId)
			{
				return;
			}
			this.DeathTimeMillis = (float)this.Combat.SpawnController.GetDeathTimeRemainingMillis();
			this.ShouldPlayRespawnCountdownAudio = true;
			base.PlayInAnimationsQueued(this.UnspawnAnimationIndex, new Action(this.PlayBarsAnimation), 1f);
		}

		private void PlayBarsAnimation()
		{
			if (this.AnimationQueue.QueueSize() > 0)
			{
				return;
			}
			float num = (float)(this.Combat.SpawnController.GetDeathTimeRemainingMillis() + 500);
			float num2 = num * HudUtils.MillisToSeconds;
			GUIUtils.PlayAnimation(this.BarsAnimation, false, 1f / num2, string.Empty);
		}

		protected override void OnBombDelivery(int causerid, TeamKind scoredteam, Vector3 deliveryPosition)
		{
			base.OnBombDelivery(causerid, scoredteam, deliveryPosition);
			this.BarsAnimation.Stop();
			GUIUtils.ResetAnimation(this.BarsAnimation);
		}

		public void OnTitleHoverOver()
		{
			GUIUtils.PlayAnimation(this.TitleAnimation, false, 1f, string.Empty);
		}

		public void OnTitleHoverOut()
		{
			GUIUtils.PlayAnimation(this.TitleAnimation, true, 1f, string.Empty);
		}

		public Animation BarsAnimation;

		public Animation TitleAnimation;
	}
}
