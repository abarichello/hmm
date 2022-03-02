using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class GadgetLevelSnapshotData : ISnapshotData<GadgetLevelSnapshotData>
	{
		public GadgetLevelSnapshotData()
		{
			this._frames = new List<IFrame>();
			this._combatGadgetLevels = new Dictionary<int, GadgetLevelSnapshotData.CombatGadgetLevelsBucket>();
		}

		public IFrame BaseFrame { get; private set; }

		public IList<IFrame> Frames
		{
			get
			{
				return this._frames;
			}
		}

		public void Init(IFrame baseFrame, GadgetLevelSnapshotData lastSnapshot)
		{
			this.BaseFrame = baseFrame;
			if (lastSnapshot == null)
			{
				this.ApplyFrame(this.BaseFrame);
				return;
			}
			foreach (KeyValuePair<int, GadgetLevelSnapshotData.CombatGadgetLevelsBucket> keyValuePair in lastSnapshot._combatGadgetLevels)
			{
				this._combatGadgetLevels[keyValuePair.Key] = new GadgetLevelSnapshotData.CombatGadgetLevelsBucket(keyValuePair.Value);
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
			int key = readData.ReadCompressedInt();
			int num = readData.ReadCompressedInt();
			GadgetLevelData value = default(GadgetLevelData);
			GadgetLevelSnapshotData.CombatGadgetLevelsBucket combatGadgetLevelsBucket;
			if (!this._combatGadgetLevels.TryGetValue(key, out combatGadgetLevelsBucket))
			{
				GadgetLevelSnapshotData.CombatGadgetLevelsBucket combatGadgetLevelsBucket2 = new GadgetLevelSnapshotData.CombatGadgetLevelsBucket();
				this._combatGadgetLevels[key] = combatGadgetLevelsBucket2;
				combatGadgetLevelsBucket = combatGadgetLevelsBucket2;
			}
			for (int i = 0; i < num; i++)
			{
				value.ReadFromStream(readData);
				combatGadgetLevelsBucket.GadgetLevels[value.Slot] = value;
			}
		}

		public void Restore(int targetTime, IFrameProcessContext ctx, IIdentifiableCollection allObjects)
		{
			foreach (KeyValuePair<int, GadgetLevelSnapshotData.CombatGadgetLevelsBucket> keyValuePair in this._combatGadgetLevels)
			{
				Identifiable @object = allObjects.GetObject(keyValuePair.Key);
				if (@object == null)
				{
					GadgetLevelSnapshotData.Log.ErrorFormat("Failed to find object id={0} to read {1} upgrades for.", new object[]
					{
						keyValuePair.Key,
						keyValuePair.Value.GadgetLevels.Count
					});
				}
				else
				{
					CombatObject bitComponent = @object.GetBitComponent<CombatObject>();
					if (bitComponent == null)
					{
						GadgetLevelSnapshotData.Log.ErrorFormat("Failed to find combat for object id={0} to read {1} upgrades for.", new object[]
						{
							keyValuePair.Key,
							keyValuePair.Value.GadgetLevels.Count
						});
						break;
					}
					foreach (KeyValuePair<GadgetSlot, GadgetLevelData> keyValuePair2 in keyValuePair.Value.GadgetLevels)
					{
						GadgetLevelData value = keyValuePair2.Value;
						GadgetBehaviour gadget = bitComponent.GetGadget(value.Slot);
						gadget.ClientSetLevel(value.UpgradeName, value.Level);
					}
				}
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GadgetLevelSnapshotData));

		private readonly List<IFrame> _frames;

		private Dictionary<int, GadgetLevelSnapshotData.CombatGadgetLevelsBucket> _combatGadgetLevels;

		private class CombatGadgetLevelsBucket
		{
			public CombatGadgetLevelsBucket()
			{
				this._gadgetLevels = new Dictionary<GadgetSlot, GadgetLevelData>();
			}

			public CombatGadgetLevelsBucket(GadgetLevelSnapshotData.CombatGadgetLevelsBucket other)
			{
				this._gadgetLevels = new Dictionary<GadgetSlot, GadgetLevelData>(other._gadgetLevels.Count);
				foreach (KeyValuePair<GadgetSlot, GadgetLevelData> keyValuePair in other._gadgetLevels)
				{
					this._gadgetLevels[keyValuePair.Key] = keyValuePair.Value;
				}
			}

			public Dictionary<GadgetSlot, GadgetLevelData> GadgetLevels
			{
				get
				{
					return this._gadgetLevels;
				}
			}

			private readonly Dictionary<GadgetSlot, GadgetLevelData> _gadgetLevels;
		}
	}
}
