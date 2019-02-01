using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public class LaneBlockerAnimator : GameHubBehaviour
	{
		private void OnEnable()
		{
			Animator[] componentsInChildren = base.GetComponentsInChildren<Animator>(true);
			this.LaneBlockersAnimator.AddRange(componentsInChildren);
			if (this.BossSpawnController)
			{
				this.BossSpawnController.OnSpawn += this.PlayOpenAnimation;
			}
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.BombManager.ListenToClientBombCreation += this.PlayCloseAnimation;
				if (GameHubBehaviour.Hub.Net.IsClient())
				{
					this.Colliders.SetActive(false);
				}
			}
		}

		private void PlayOpenAnimation(SpawnEvent spawnEvent)
		{
			for (int i = 0; i < this.LaneBlockersAnimator.Count; i++)
			{
				this.LaneBlockersAnimator[i].SetBool("active", true);
			}
		}

		private void PlayCloseAnimation(int bombEventId)
		{
			for (int i = 0; i < this.LaneBlockersAnimator.Count; i++)
			{
				this.LaneBlockersAnimator[i].SetBool("active", false);
			}
		}

		private void OnDisable()
		{
			if (this.BossSpawnController)
			{
				this.BossSpawnController.OnSpawn -= this.PlayOpenAnimation;
			}
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.BombManager)
			{
				GameHubBehaviour.Hub.BombManager.ListenToClientBombCreation -= this.PlayCloseAnimation;
			}
			this.LaneBlockersAnimator.Clear();
		}

		public SpawnController BossSpawnController;

		public GameObject Colliders;

		private List<Animator> LaneBlockersAnimator = new List<Animator>();

		private bool _isServer;
	}
}
