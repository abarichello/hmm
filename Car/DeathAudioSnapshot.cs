using System;
using FMod;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Car
{
	public class DeathAudioSnapshot : GameHubBehaviour
	{
		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.OnObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.OnObjectSpawn;
		}

		private void OnDisable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.OnObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.OnObjectSpawn;
			if (this.deathSnapshotInstance != null)
			{
				this.deathSnapshotInstance.KeyOff();
			}
		}

		private void OnObjectUnspawn(PlayerEvent data)
		{
			if (data.TargetId != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				return;
			}
			if (this.deathSnapshotInstance == null)
			{
				this.deathSnapshotInstance = FMODAudioManager.PlayAt(this.deathSnapshot, base.transform);
			}
		}

		private void OnObjectSpawn(PlayerEvent data)
		{
			if (data.TargetId != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				return;
			}
			if (this.deathSnapshotInstance != null)
			{
				this.deathSnapshotInstance.Stop();
			}
			this.deathSnapshotInstance = null;
		}

		public FMODAsset deathSnapshot;

		private FMODAudioManager.FMODAudio deathSnapshotInstance;

		private bool destroyed;
	}
}
