using System;
using System.Collections;
using HeavyMetalMachines.VFX;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class GadgetFeedbackParticles : BaseGadgetFeedback
	{
		public void ExternalToggle(bool enable)
		{
			if (enable == this._canBeActivated)
			{
				return;
			}
			this._canBeActivated = enable;
			if (this._canBeActivated)
			{
				if (this._active)
				{
					this.OnActivate();
				}
			}
			else
			{
				this.OnDeactivateInternal();
			}
		}

		protected override void Start()
		{
			base.Start();
			bool flag;
			if (this.previzMode)
			{
				flag = this.previzIsAlly;
			}
			else
			{
				flag = (this.combatObject.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam);
			}
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				if (!(this.particleSystems[i] == null))
				{
					HMMTeamParticleSystem hmmteamParticleSystem = this.particleSystems[i] as HMMTeamParticleSystem;
					if (hmmteamParticleSystem)
					{
						hmmteamParticleSystem.CurrentTeam = ((!flag) ? VFXTeam.Enemy : VFXTeam.Ally);
					}
				}
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			if (!this._canBeActivated)
			{
				return;
			}
			if (this.DelayStart > 0f)
			{
				base.StartCoroutine(this.WaitToActivate());
			}
			else
			{
				this.StartParticles();
			}
			if (this.stopOnTimer > 0f)
			{
				base.StartCoroutine(this.WaitToDeactivate());
			}
		}

		private void StartParticles()
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				if (!(this.particleSystems[i] == null))
				{
					this.particleSystems[i].Play();
				}
			}
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			this.OnDeactivateInternal();
		}

		private void OnDeactivateInternal()
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				if (!(this.particleSystems[i] == null))
				{
					this.particleSystems[i].Stop();
				}
			}
		}

		private IEnumerator WaitToActivate()
		{
			yield return new WaitForSeconds(this.DelayStart);
			this.StartParticles();
			yield break;
		}

		private IEnumerator WaitToDeactivate()
		{
			yield return new WaitForSeconds(this.stopOnTimer);
			base.OnDeactivate();
			this.OnDeactivateInternal();
			yield break;
		}

		public HoplonParticleSystem[] particleSystems;

		[SerializeField]
		private float DelayStart;

		[SerializeField]
		private float stopOnTimer;

		private bool _canBeActivated = true;
	}
}
