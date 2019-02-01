using System;
using Pocketverse;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class GlobalEvent : IEventContent, IBitStreamSerializable
	{
		public int EventTime { get; set; }

		public EventScopeKind GetKind()
		{
			return EventScopeKind.None;
		}

		public bool ShouldBuffer()
		{
			return false;
		}

		public void WriteToBitStream(BitStream bs)
		{
		}

		public void ReadFromBitStream(BitStream bs)
		{
		}
	}
}
