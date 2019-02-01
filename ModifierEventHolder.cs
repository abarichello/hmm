using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class ModifierEventHolder : IBitStreamSerializable
	{
		public bool HasRecords { get; private set; }

		internal ModifierEventHolder.EventData GetData(int otherId, GadgetSlot slot)
		{
			ModifierEventHolder.EventData eventData = null;
			for (int i = 0; i < this._events.Count; i++)
			{
				ModifierEventHolder.EventData eventData2 = this._events[i];
				if (eventData2.OtherId == otherId)
				{
					if (eventData2.Slot == slot)
					{
						eventData = eventData2;
						break;
					}
				}
			}
			if (eventData != null)
			{
				return eventData;
			}
			eventData = new ModifierEventHolder.EventData(otherId, slot);
			this._events.Add(eventData);
			return eventData;
		}

		public void AddData(ModifierEvent evt)
		{
			this.HasRecords = true;
			ModifierEventHolder.EventData data = this.GetData(evt.OtherId, evt.Slot);
			data.Add(evt);
		}

		public void Clear()
		{
			for (int i = 0; i < this._events.Count; i++)
			{
				this._events[i].Clear();
			}
			this.HasRecords = false;
		}

		public void WriteToBitStream(BitStream stream)
		{
			for (int i = 0; i < this._events.Count; i++)
			{
				ModifierEventHolder.EventData eventData = this._events[i];
				if (eventData.Count > 0)
				{
					stream.WriteBool(true);
					stream.WriteCompressedInt(eventData.OtherId);
					stream.WriteGadgetSlot(eventData.Slot);
					eventData.WriteToBitStream(stream);
				}
			}
			stream.WriteBool(false);
		}

		public void ReadFromBitStream(BitStream stream)
		{
			while (stream.ReadBool())
			{
				int otherId = stream.ReadCompressedInt();
				GadgetSlot slot = stream.ReadGadgetSlot();
				ModifierEventHolder.EventData data = this.GetData(otherId, slot);
				data.ReadFromBitStream(stream);
			}
		}

		private const int EventListCapacity = 42;

		internal List<ModifierEventHolder.EventData> _events = new List<ModifierEventHolder.EventData>(42);

		internal class EventData : IBitStreamSerializable
		{
			public EventData(int otherId, GadgetSlot slot)
			{
				this.OtherId = otherId;
				this.Slot = slot;
			}

			public void Clear()
			{
				this.Count = 0;
				this.Effects.Clear();
				this.Amounts.Clear();
			}

			public void Add(ModifierEvent evt)
			{
				int num = this.Effects.IndexOf((int)evt.Effect);
				if (num >= 0)
				{
					List<float> amounts;
					int index;
					(amounts = this.Amounts)[index = num] = amounts[index] + evt.Amount;
					return;
				}
				this.Effects.Add((int)evt.Effect);
				this.Amounts.Add(evt.Amount);
				this.Count++;
			}

			public void GetEvent(ref ModifierEvent evt, int idx)
			{
				evt.OtherId = this.OtherId;
				evt.Slot = this.Slot;
				evt.Effect = (EffectKind)this.Effects[idx];
				evt.Amount = this.Amounts[idx];
			}

			public void WriteToBitStream(BitStream bs)
			{
				bs.WriteCompressedInt(this.Count);
				for (int i = 0; i < this.Count; i++)
				{
					bs.WriteEffectKind(this.Effects[i]);
					bs.WriteCompressedFloat(this.Amounts[i]);
				}
			}

			public void ReadFromBitStream(BitStream bs)
			{
				this.Clear();
				this.Count = bs.ReadCompressedInt();
				for (int i = 0; i < this.Count; i++)
				{
					this.Effects.Add(bs.ReadEffectKindAsInt());
					this.Amounts.Add(bs.ReadCompressedFloat());
				}
			}

			private const int ModifiersCapacity = 16;

			public readonly int OtherId;

			public readonly GadgetSlot Slot;

			public int Count;

			private List<int> Effects = new List<int>(16);

			private List<float> Amounts = new List<float>(16);
		}
	}
}
