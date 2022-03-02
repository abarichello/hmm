using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class CombatStatesSnapshotData : ISnapshotData<CombatStatesSnapshotData>
	{
		public CombatStatesSnapshotData()
		{
			this._frames = new List<IFrame>();
			this._combatDataSnapshots = new Dictionary<int, CombatDataSnapshotData>();
			this._combatAttributesSnapshots = new Dictionary<int, CombatAttributesSnapshotData>();
			this._gadgetDataSnapshots = new Dictionary<int, GadgetDataSnapshotData>();
			this._spawnControllerSnapshots = new Dictionary<int, SpawnControllerSnapshotData>();
			this._gridGameSnapshots = new Dictionary<int, GridGamePlayerSnapshotData>();
			this._combatDataStream = new SnapshotUpdateStream<CombatDataSnapshotData>();
			this._combatAttributesStream = new SnapshotUpdateStream<CombatAttributesSnapshotData>();
			this._gadgetDataStream = new SnapshotUpdateStream<GadgetDataSnapshotData>();
			this._spawnControllerStream = new SnapshotUpdateStream<SpawnControllerSnapshotData>();
			this._gridGameStream = new SnapshotStateUpdateStream<GridGamePlayerSnapshotData>();
		}

		public IFrame BaseFrame { get; private set; }

		public IList<IFrame> Frames
		{
			get
			{
				return this._frames;
			}
		}

		public void Init(IFrame baseFrame, CombatStatesSnapshotData lastSnapshot)
		{
			this.BaseFrame = baseFrame;
			if (lastSnapshot == null)
			{
				this.ApplyFrame(this.BaseFrame);
				return;
			}
			foreach (KeyValuePair<int, CombatDataSnapshotData> keyValuePair in lastSnapshot._combatDataSnapshots)
			{
				this._combatDataSnapshots[keyValuePair.Key] = new CombatDataSnapshotData(keyValuePair.Value);
			}
			foreach (KeyValuePair<int, CombatAttributesSnapshotData> keyValuePair2 in lastSnapshot._combatAttributesSnapshots)
			{
				this._combatAttributesSnapshots[keyValuePair2.Key] = new CombatAttributesSnapshotData(keyValuePair2.Value);
			}
			foreach (KeyValuePair<int, GadgetDataSnapshotData> keyValuePair3 in lastSnapshot._gadgetDataSnapshots)
			{
				this._gadgetDataSnapshots[keyValuePair3.Key] = new GadgetDataSnapshotData(keyValuePair3.Value);
			}
			foreach (KeyValuePair<int, SpawnControllerSnapshotData> keyValuePair4 in lastSnapshot._spawnControllerSnapshots)
			{
				this._spawnControllerSnapshots[keyValuePair4.Key] = new SpawnControllerSnapshotData(keyValuePair4.Value);
			}
			foreach (KeyValuePair<int, GridGamePlayerSnapshotData> keyValuePair5 in lastSnapshot._gridGameSnapshots)
			{
				this._gridGameSnapshots[keyValuePair5.Key] = new GridGamePlayerSnapshotData(keyValuePair5.Value);
			}
			foreach (IFrame frame in lastSnapshot._frames)
			{
				this.ApplyFrame(frame);
			}
			this.ApplyFrame(this.BaseFrame);
		}

		private void ApplyFrame(IFrame frame)
		{
			BitStream readData = frame.GetReadData();
			this._combatDataStream.ReadStream(readData, this._combatDataSnapshots);
			this._combatAttributesStream.ReadStream(readData, this._combatAttributesSnapshots);
			this._gadgetDataStream.ReadStream(readData, this._gadgetDataSnapshots);
			this._spawnControllerStream.ReadStream(readData, this._spawnControllerSnapshots);
			this._gridGameStream.ReadStream(readData, this._gridGameSnapshots);
		}

		public void Restore(int targetTime, IFrameProcessContext ctx, ICombatStatesFeature feature)
		{
			this._combatStatesFeature = feature;
			ctx.EnqueueAction(this.BaseFrame.Time, new Action(this.RestoreSnapshot));
			for (int i = 0; i < this._frames.Count; i++)
			{
				IFrame frame = this._frames[i];
				if (frame.Time > targetTime)
				{
					break;
				}
				ctx.AddToExecutionQueue(frame.FrameId);
			}
		}

		private void RestoreSnapshot()
		{
			foreach (KeyValuePair<int, CombatDataSnapshotData> keyValuePair in this._combatDataSnapshots)
			{
				ICombatDataSerialData combatData = this._combatStatesFeature.GetCombatData(keyValuePair.Key);
				if (combatData != null)
				{
					combatData.Apply(keyValuePair.Value);
				}
			}
			foreach (KeyValuePair<int, CombatAttributesSnapshotData> keyValuePair2 in this._combatAttributesSnapshots)
			{
				ICombatAttributesSerialData combatAttributes = this._combatStatesFeature.GetCombatAttributes(keyValuePair2.Key);
				if (combatAttributes != null)
				{
					combatAttributes.Apply(keyValuePair2.Value);
				}
			}
			foreach (KeyValuePair<int, GadgetDataSnapshotData> keyValuePair3 in this._gadgetDataSnapshots)
			{
				IGadgetDataSerialData gadgetData = this._combatStatesFeature.GetGadgetData(keyValuePair3.Key);
				if (gadgetData != null)
				{
					gadgetData.Apply(keyValuePair3.Value);
				}
			}
			foreach (KeyValuePair<int, SpawnControllerSnapshotData> keyValuePair4 in this._spawnControllerSnapshots)
			{
				ISpawnControllerSerialData spawnControllerData = this._combatStatesFeature.GetSpawnControllerData(keyValuePair4.Key);
				if (spawnControllerData != null)
				{
					spawnControllerData.Apply(keyValuePair4.Value);
				}
			}
			foreach (KeyValuePair<int, GridGamePlayerSnapshotData> keyValuePair5 in this._gridGameSnapshots)
			{
				BombGridController.IGridGamePlayerSerialData gridGamePlayerData = this._combatStatesFeature.GetGridGamePlayerData(keyValuePair5.Key);
				if (gridGamePlayerData != null)
				{
					gridGamePlayerData.Apply(keyValuePair5.Value);
				}
			}
		}

		private readonly List<IFrame> _frames;

		private ICombatStatesFeature _combatStatesFeature;

		private readonly Dictionary<int, CombatDataSnapshotData> _combatDataSnapshots;

		private readonly Dictionary<int, CombatAttributesSnapshotData> _combatAttributesSnapshots;

		private readonly Dictionary<int, GadgetDataSnapshotData> _gadgetDataSnapshots;

		private readonly Dictionary<int, SpawnControllerSnapshotData> _spawnControllerSnapshots;

		private readonly Dictionary<int, GridGamePlayerSnapshotData> _gridGameSnapshots;

		private readonly SnapshotUpdateStream<CombatDataSnapshotData> _combatDataStream;

		private readonly SnapshotUpdateStream<CombatAttributesSnapshotData> _combatAttributesStream;

		private readonly SnapshotUpdateStream<GadgetDataSnapshotData> _gadgetDataStream;

		private readonly SnapshotUpdateStream<SpawnControllerSnapshotData> _spawnControllerStream;

		private readonly SnapshotStateUpdateStream<GridGamePlayerSnapshotData> _gridGameStream;
	}
}
