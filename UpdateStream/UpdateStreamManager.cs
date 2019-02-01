using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Fog;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public class UpdateStreamManager : GameHubBehaviour, ICleanupListener, ISerializationCallbackReceiver
	{
		public Dictionary<int, StreamObject> StreamObjectsMap
		{
			get
			{
				return this._streamObjects;
			}
		}

		public List<StreamObject> StreamObjects
		{
			get
			{
				return this._streamObjectsList;
			}
		}

		private void Awake()
		{
			this._updaterHigh = new TimedUpdater(this.FrequencyHigh, true, true);
		}

		public void SetRunning(bool running)
		{
			this._stopped = !running;
			if (running)
			{
				TRCInterpolator.ResetUpdateTimers();
			}
		}

		private void Update()
		{
			if (this._stopped)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				TRCInterpolator.RunUpdate();
				return;
			}
			if (this._updaterHigh.ShouldHalt())
			{
				return;
			}
			PlaybackManager.ModifierEvent.SendEvents();
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreBoard.State.Replay)
			{
				PlaybackManager.TransformStates.SendData();
				PlaybackManager.TransformStates.SendMovementData(this._movementStreamList);
			}
			PlaybackManager.PlayerStats.SendUpdate();
			PlaybackManager.CombatStates.SendData();
			PlaybackManager.CombatFeedbacks.SendData();
		}

		public void OnCleanup(CleanupMessage msg)
		{
			GameHubBehaviour.Hub.Stream.Cleanup();
			this._streamObjects.Clear();
			this._streamObjectsList.Clear();
			this._movementStreamList.Clear();
			this.MovementStreamsById.Clear();
		}

		public void AddObject(int id, StreamObject stream)
		{
			this._streamObjects[id] = stream;
			if (!this._streamObjectsList.Contains(stream))
			{
				this._streamObjectsList.Add(stream);
			}
			GameHubBehaviour.Hub.Stream.AddObject(stream.Id);
		}

		public void RemoveObject(int id)
		{
			this._streamObjects.Remove(id);
			this._streamObjectsList.RemoveAll((StreamObject x) => x == null || x.Id.ObjId == id);
			GameHubBehaviour.Hub.Stream.Remove(id);
		}

		public void AddMovementStream(int id, MovementStream stream)
		{
			this.MovementStreamsById[id] = stream;
			if (!this._movementStreamList.Contains(stream))
			{
				this._movementStreamList.Add(stream);
			}
			GameHubBehaviour.Hub.Stream.AddObject(stream.Id);
		}

		public void RemoveMovementStream(int id)
		{
			this.MovementStreamsById.Remove(id);
			this._movementStreamList.RemoveAll((MovementStream x) => x == null || x.Id.ObjId == id);
			GameHubBehaviour.Hub.Stream.Remove(id);
		}

		public override void OnAfterDeserialize()
		{
			for (int i = 0; i < this._streamObjectsList.Count; i++)
			{
				StreamObject streamObject = this._streamObjectsList[i];
				this._streamObjects[streamObject.Id.ObjId] = streamObject;
			}
			for (int j = 0; j < this._movementStreamList.Count; j++)
			{
				MovementStream movementStream = this._movementStreamList[j];
				this.MovementStreamsById[movementStream.Id.ObjId] = movementStream;
			}
		}

		public int FrequencyHigh;

		private TimedUpdater _updaterHigh;

		public static readonly BitLogger Log = new BitLogger(typeof(UpdateStreamManager));

		private readonly Dictionary<int, StreamObject> _streamObjects = new Dictionary<int, StreamObject>();

		[SerializeField]
		private List<StreamObject> _streamObjectsList = new List<StreamObject>();

		private readonly List<MovementStream> _movementStreamList = new List<MovementStream>();

		public readonly Dictionary<int, MovementStream> MovementStreamsById = new Dictionary<int, MovementStream>();

		private bool _stopped = true;
	}
}
