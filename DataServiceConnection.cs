using System;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	internal class DataServiceConnection : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				string value = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.DataServiceAddress);
				if (string.IsNullOrEmpty(value))
				{
					return;
				}
				int intValue = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.DataServicePort);
				if (intValue == 0)
				{
					return;
				}
				this.heatMap = new HeatMap(this.mapVersion, this.mapSize, value, intValue);
				this.playerSpawnManager = GameHubBehaviour.Hub.Events.Players;
				this.botsSpawnManager = GameHubBehaviour.Hub.Events.Bots;
				this.playerSpawnManager.ListenToObjectUnspawn += this.OnPlayerDeath;
				this.botsSpawnManager.ListenToObjectUnspawn += this.OnPlayerDeath;
			}
		}

		private void OnDestroy()
		{
			if (this.playerSpawnManager)
			{
				this.playerSpawnManager.ListenToObjectUnspawn -= this.OnPlayerDeath;
			}
			if (this.botsSpawnManager)
			{
				this.botsSpawnManager.ListenToObjectUnspawn -= this.OnPlayerDeath;
			}
		}

		private void OnPlayerDeath(PlayerEvent data)
		{
			if (this.heatMap == null)
			{
				return;
			}
			Transform transform = GameHubBehaviour.Hub.ObjectCollection.GetObject(data.TargetId).transform;
			if (transform == null)
			{
				return;
			}
			if (data.Reason == SpawnReason.Death)
			{
				this.heatMap.RegisterEvent(HeatMap.EventType.Death, data.Location);
			}
		}

		private HeatMap heatMap;

		public string mapVersion = "Undefined";

		public float mapSize = 100f;

		private PlayerSpawnManager playerSpawnManager;

		private BotAISpawnManager botsSpawnManager;
	}
}
