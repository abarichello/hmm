using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Swordfish.API;
using Hoplon.Metrics.Data;
using Pocketverse;
using Pocketverse.MuralContext;
using Pocketverse.Util;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.UpdateStream
{
	public class UpdateStreamManager : GameHubBehaviour, ICleanupListener, ISerializationCallbackReceiver, IUpdateManager
	{
		public IList<MovementStream> AllStreams
		{
			get
			{
				return this._movementStreamList;
			}
		}

		private void Awake()
		{
			this._updaterHigh = new TimedUpdater(this.FrequencyHigh, true, true);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._histogramData = new ExecutionFrequencyHistogramData(1.0, this.UpdateHistogramBuckets);
				ObservableExtensions.Subscribe<ScoreBoardState>(Observable.Do<ScoreBoardState>(this._scoreBoard.StateChangedObservation, new Action<ScoreBoardState>(this.CheckState)));
			}
		}

		private void CheckState(ScoreBoardState state)
		{
			BombScoreboardState currentState = state.CurrentState;
			if (currentState != BombScoreboardState.BombDelivery && currentState != BombScoreboardState.PreReplay)
			{
				this._histogramData.StopRunning();
			}
			else
			{
				this._histogramData.StartRunning();
			}
		}

		public bool IsRunning()
		{
			return !this._stopped;
		}

		public void SetRunning(bool running)
		{
			this._stopped = !running;
			if (GameHubBehaviour.Hub.Net.IsServer() && !running)
			{
				this._histogramData.StopRunning();
				SerializedHistogramData serializedHistogramData = new SerializedHistogramData();
				serializedHistogramData.Data = this._histogramData.Data;
				serializedHistogramData.Buckets = Array.ConvertAll<int, string>(this._histogramData.BucketCallCounts, (int input) => input.ToString());
				SerializedHistogramData serializedHistogramData2 = serializedHistogramData;
				this._swordfishLogProvider.GetSwordfishLog().BILogServerMsg(17, string.Format("Update Stream Manager execution frequency data={0}", serializedHistogramData2.Serialize()), false);
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
				return;
			}
			if (this._updaterHigh.ShouldHalt())
			{
				return;
			}
			this._histogramData.ProcessExecuted();
			this._modifierEventDispatcher.SendEvents();
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreboardState.Replay)
			{
				this._transformDispatcher.SendMovementData(this._movementStreamList);
			}
			this._statsDispatcher.SendUpdate();
			this._combatStatesDispatcher.SendData();
			this._combatFeedbackDispatcher.SendData();
		}

		public void OnCleanup(CleanupMessage msg)
		{
			GameHubBehaviour.Hub.Stream.Cleanup();
			this._movementStreamList.Clear();
			this.MovementStreamsById.Clear();
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
			for (int i = 0; i < this._movementStreamList.Count; i++)
			{
				MovementStream movementStream = this._movementStreamList[i];
				this.MovementStreamsById[movementStream.Id.ObjId] = movementStream;
			}
		}

		public int FrequencyHigh;

		[NonSerialized]
		public int[] UpdateHistogramBuckets = Enumerable.Range(0, 31).ToArray<int>();

		private TimedUpdater _updaterHigh;

		public static readonly BitLogger Log = new BitLogger(typeof(UpdateStreamManager));

		[Inject]
		private ICombatFeedbackDispatcher _combatFeedbackDispatcher;

		[Inject]
		private ICombatStatesDispatcher _combatStatesDispatcher;

		[Inject]
		private IModifierEventDispatcher _modifierEventDispatcher;

		[Inject]
		private ITransformDispatcher _transformDispatcher;

		[Inject]
		private IStatsDispatcher _statsDispatcher;

		[Inject]
		private IScoreBoard _scoreBoard;

		[Inject]
		private ISwordfishLogProvider _swordfishLogProvider;

		private ExecutionFrequencyHistogramData _histogramData;

		private readonly List<MovementStream> _movementStreamList = new List<MovementStream>();

		public readonly Dictionary<int, MovementStream> MovementStreamsById = new Dictionary<int, MovementStream>();

		private bool _stopped = true;
	}
}
