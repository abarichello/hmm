using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class ArenaVisualEvents : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivery;
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivery;
		}

		public void OnBombDelivery(int causerId, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			HoplonParticleSystem[] array = (scoredTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? this.enemyBaseParticles : this.allyBaseParticles;
			if (array == null)
			{
				return;
			}
			foreach (HoplonParticleSystem hoplonParticleSystem in array)
			{
				Debug.Assert(hoplonParticleSystem != null, "Unassigned particle system!", Debug.TargetTeam.All);
				hoplonParticleSystem.Play();
			}
		}

		public HoplonParticleSystem[] enemyBaseParticles;

		public HoplonParticleSystem[] allyBaseParticles;
	}
}
