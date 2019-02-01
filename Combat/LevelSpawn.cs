using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class LevelSpawn : GameHubBehaviour
	{
		private void Awake()
		{
			GameHubBehaviour.Hub.Events.Players.Spawn = this;
			GameHubBehaviour.Hub.Events.Bots.Spawn = this;
		}

		private void Start()
		{
			GameHubBehaviour.Hub.Events.Players.SpawnAllObjects();
			GameHubBehaviour.Hub.Events.Bots.SpawnAllObjects();
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Players.Spawn = null;
			GameHubBehaviour.Hub.Events.Bots.Spawn = null;
		}

		private void OnDrawGizmosSelected()
		{
			if (this.RedStarts == null || this.BluStarts == null)
			{
				return;
			}
			int num = this.RedStarts.Length + this.BluStarts.Length;
			for (int i = 0; i < num; i++)
			{
				bool flag = i >= this.RedStarts.Length;
				Transform transform = (!flag) ? this.RedStarts[i] : this.BluStarts[i - this.RedStarts.Length];
				if (transform)
				{
					Gizmos.color = ((!flag) ? Color.red : Color.blue);
					Gizmos.DrawWireSphere(transform.position, 5f);
					Gizmos.DrawLine(transform.position, transform.position + transform.forward * 10f);
				}
			}
		}

		public Transform GetStart(PlayerData player)
		{
			return (player.Team != TeamKind.Red) ? this.BluStarts[player.GridIndex] : this.RedStarts[player.GridIndex];
		}

		public Transform GetSpawn(PlayerData player)
		{
			return (player.Team != TeamKind.Red) ? this.BluSpawns[player.GridIndex] : this.RedSpawns[player.GridIndex];
		}

		public Transform[] RedStarts;

		public Transform[] BluStarts;

		public Transform[] RedSpawns;

		public Transform[] BluSpawns;
	}
}
