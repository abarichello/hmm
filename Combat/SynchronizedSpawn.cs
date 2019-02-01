using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class SynchronizedSpawn : GameHubBehaviour
	{
		public int RespawnEveryMillis
		{
			get
			{
				return (int)(this.RespawnEverySeconds * 1000f);
			}
		}

		private void Awake()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._nextWave = GameHubBehaviour.Hub.MatchMan.WarmupSeconds * 1000;
		}

		private void Start()
		{
			for (int i = 0; i < this.Objects.Length; i++)
			{
				SpawnController spawnController = this.Objects[i];
				if (!(spawnController == null))
				{
					GameHubBehaviour.Hub.Events.TriggerEvent(new SceneEvent
					{
						SceneObjectId = spawnController.Id.ObjId,
						State = SceneEvent.StateKind.Unspawned,
						CauserId = -1
					});
				}
			}
		}

		private void Update()
		{
			if (this._update.ShouldHalt() || GameHubBehaviour.Hub.GameTime.GetPlaybackTime() < this._nextWave)
			{
				return;
			}
			this._nextWave += this.RespawnEveryMillis;
			for (int i = 0; i < this.Objects.Length; i++)
			{
				SpawnController spawnController = this.Objects[i];
				if (!(spawnController == null) && !spawnController.IsSpawned())
				{
					GameHubBehaviour.Hub.Events.TriggerEvent(new SceneEvent
					{
						SceneObjectId = spawnController.Id.ObjId,
						State = SceneEvent.StateKind.Spawned,
						CauserId = -1
					});
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SynchronizedSpawn));

		public float RespawnEverySeconds;

		public SpawnController[] Objects;

		private int _lastWave;

		private int _nextWave;

		private TimedUpdater _update = new TimedUpdater(250, true, false);
	}
}
