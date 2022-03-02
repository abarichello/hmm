using System;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class DamageAudioVFX : BaseVFX
	{
		private void Update()
		{
			if (this.timeToExpire <= Time.timeSinceLevelLoad)
			{
				this.ReturnToDefault();
			}
		}

		protected override void OnActivate()
		{
			if (!GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer())
			{
				this.LogWarn("{0} not on hub client", new object[]
				{
					base.name
				});
				return;
			}
			if (!this._targetFXInfo.Owner)
			{
				this.LogWarn(" Owner null in {0}", new object[]
				{
					base.name
				});
				return;
			}
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId);
			if (playerOrBotsByObjectId != null)
			{
				this.ownerVOController = playerOrBotsByObjectId.CharacterInstance.GetComponent<VoiceOverController>();
				if (this.ownerVOController)
				{
					this.ownerVOController.ChangeDamageSFX(this.DamageSFX);
					this.timeToExpire = Time.timeSinceLevelLoad + this.expireTime;
					this.ended = false;
					return;
				}
			}
			this.LogWarn("{0} Couldn't find VoiceOverController", new object[]
			{
				base.name
			});
		}

		protected override void WillDeactivate()
		{
			this.ReturnToDefault();
		}

		protected override void OnDeactivate()
		{
			this.ReturnToDefault();
		}

		private void ReturnToDefault()
		{
			if (!this.ended && this.ownerVOController)
			{
				this.ended = true;
				this.ownerVOController.SetDamageSFXToDefault();
			}
		}

		private void LogWarn(string msg, params object[] args)
		{
		}

		public AudioEventAsset DamageSFX;

		public float expireTime = 30f;

		private bool ended;

		private float timeToExpire = float.MaxValue;

		private VoiceOverController ownerVOController;
	}
}
