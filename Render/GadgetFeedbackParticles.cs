using System;
using HeavyMetalMachines.VFX;
using NewParticleSystem;
using Pocketverse;

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
			bool flag = this.combatObject.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
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

		public HoplonParticleSystem[] particleSystems;

		private bool _canBeActivated = true;
	}
}
