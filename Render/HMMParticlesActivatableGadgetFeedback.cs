using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.VFX;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class HMMParticlesActivatableGadgetFeedback : BaseActivatableGadgetFeedback
	{
		public bool SetPrevizIsAlly
		{
			set
			{
				this.previzIsAlly = value;
			}
		}

		protected void Start()
		{
			HMMHub hub = GameHubBehaviour.Hub;
			if (hub && hub.Net.IsServer())
			{
				return;
			}
			CombatObject componentInParent = base.GetComponentInParent<CombatObject>();
			bool flag;
			if (!hub)
			{
				flag = this.previzIsAlly;
			}
			else
			{
				flag = (componentInParent.Team == hub.Players.CurrentPlayerTeam);
			}
			for (int i = 0; i < this._particleSystems.Length; i++)
			{
				HMMTeamParticleSystem hmmteamParticleSystem = this._particleSystems[i] as HMMTeamParticleSystem;
				if (hmmteamParticleSystem)
				{
					hmmteamParticleSystem.CurrentTeam = ((!flag) ? VFXTeam.Enemy : VFXTeam.Ally);
				}
			}
		}

		protected override void OnActivate()
		{
			for (int i = 0; i < this._particleSystems.Length; i++)
			{
				this._particleSystems[i].Play();
			}
		}

		protected override void OnDeactivate()
		{
			for (int i = 0; i < this._particleSystems.Length; i++)
			{
				this._particleSystems[i].Stop();
			}
		}

		[SerializeField]
		private HoplonParticleSystem[] _particleSystems;

		private bool previzIsAlly;
	}
}
