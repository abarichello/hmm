using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Standard_Assets.Scripts.Infra.UnityParticles;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class UnityParticlesActivatableGadgetFeedback : BaseActivatableGadgetFeedback
	{
		protected override void OnActivate()
		{
			HMMHub hub = GameHubBehaviour.Hub;
			if (hub.Net.IsServer())
			{
				return;
			}
			CombatObject componentInParent = base.GetComponentInParent<CombatObject>();
			if (componentInParent == null)
			{
				return;
			}
			bool flag = componentInParent.Team == hub.Players.CurrentPlayerTeam;
			this._currentTeam = ((!flag) ? VFXTeam.Enemy : VFXTeam.Ally);
			for (int i = 0; i < this._particleSystems.Length; i++)
			{
				UnityParticlesGroup unityParticlesGroup = this._particleSystems[i];
				if (unityParticlesGroup.Team == VFXTeam.Neutral || this._currentTeam == unityParticlesGroup.Team)
				{
					unityParticlesGroup.Play();
				}
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
		private UnityParticlesGroup[] _particleSystems;

		private VFXTeam _currentTeam;
	}
}
