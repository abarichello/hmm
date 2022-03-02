using System;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Standard_Assets.Scripts.Infra.UnityParticles;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class GadgetFeedbackUnityParticles : BaseGadgetFeedback
	{
		protected override void InitializeGadgetFeedback()
		{
			base.InitializeGadgetFeedback();
			if (this.combatObject == null)
			{
				return;
			}
			this._currentTeam = ((this.combatObject.Team != GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? VFXTeam.Enemy : VFXTeam.Ally);
		}

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

		private void OnDeactivateInternal()
		{
			for (int i = 0; i < this._particles.Length; i++)
			{
				if (!(this._particles[i] == null))
				{
					this._particles[i].Stop();
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
			for (int i = 0; i < this._particles.Length; i++)
			{
				if (!(this._particles[i] == null))
				{
					if (this._particles[i].Team == this._currentTeam || this._particles[i].Team == VFXTeam.Neutral)
					{
						this._particles[i].Play();
					}
				}
			}
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			this.OnDeactivateInternal();
		}

		[SerializeField]
		private UnityParticlesGroup[] _particles;

		[NonSerialized]
		private bool _canBeActivated = true;

		[NonSerialized]
		private VFXTeam _currentTeam;
	}
}
