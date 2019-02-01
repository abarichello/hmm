using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class bombDropperVisualController : GameHubBehaviour
	{
		public void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			this.animator = base.gameObject.GetComponent<Animator>();
		}

		public void Update()
		{
			Shader.SetGlobalFloat("_BombDropperColorState", this.bombDropperColorState);
			Shader.SetGlobalFloat("_IntensityDropperAlly", this.bombDropperIntensityBlue);
			Shader.SetGlobalFloat("_IntensityDropperEnemy", this.bombDropperIntensityRed);
		}

		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate += this.ListenToMatchUpdate;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate -= this.ListenToMatchUpdate;
		}

		private void ListenToMatchUpdate()
		{
			if (!GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb())
			{
				this.animator.SetBool("bombBlueTeam", false);
				this.animator.SetBool("bombRedTeam", false);
				this.animator.SetTrigger("bombReleased");
			}
			else if (GameHubBehaviour.Hub.BombManager.ActiveBomb.TeamOwner == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
			{
				this.animator.SetBool("bombReleased", false);
				this.animator.SetTrigger("bombBlueTeam");
			}
			else
			{
				this.animator.SetBool("bombReleased", false);
				this.animator.SetTrigger("bombRedTeam");
			}
		}

		public float bombDropperColorState;

		public float bombDropperIntensityBlue;

		public float bombDropperIntensityRed;

		private Animator animator;
	}
}
