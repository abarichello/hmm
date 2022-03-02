using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombRoundStartObject : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				Object.Destroy(this);
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombUnspawn += this.OnBombTaken;
			this.RoundStart();
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombUnspawn -= this.OnBombTaken;
		}

		private void OnBombTaken(PickupRemoveEvent obj)
		{
			this.bombDroppersManager.SetBool("round_start", false);
			this.bombDroppersManager.SetTrigger("round_bombTaken");
		}

		private void RoundStart()
		{
			this.bombDroppersManager.SetBool("round_bombTaken", false);
			this.bombDroppersManager.SetTrigger("round_start");
		}

		public Animator bombDroppersManager;
	}
}
