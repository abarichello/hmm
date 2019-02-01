using System;
using Pocketverse;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class EventData : GameHubObject, IBitStreamSerializable
	{
		public IEventContent Content
		{
			get
			{
				return this._content;
			}
			set
			{
				if (value == null)
				{
					this._eventClass = -1;
					this._content = null;
					this.Kind = EventScopeKind.None;
					return;
				}
				this._eventClass = GameHubObject.Hub.Classes.GetClassId(value);
				if (this._eventClass == -1)
				{
					this._content = null;
					this.Kind = EventScopeKind.None;
					return;
				}
				this._content = value;
				this.Kind = this._content.GetKind();
			}
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.EventId);
			bs.WriteCompressedInt((int)this.Kind);
			bs.WriteCompressedInt(this._eventClass);
			if (this._eventClass >= 0)
			{
				bs.WriteCompressedInt(this._content.EventTime);
				((IBitStreamSerializable)this._content).WriteToBitStream(bs);
			}
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.EventId = bs.ReadCompressedInt();
			this.Kind = (EventScopeKind)bs.ReadCompressedInt();
			this._eventClass = bs.ReadCompressedInt();
			if (this._eventClass >= 0)
			{
				this._content = (IEventContent)GameHubObject.Hub.Classes.CreateInstance(this._eventClass);
				this._content.EventTime = bs.ReadCompressedInt();
				((IBitStreamSerializable)this._content).ReadFromBitStream(bs);
			}
		}

		public override string ToString()
		{
			return string.Format("Event={0} Slot={1} Data={2}", this.EventId, this.Kind, this.Content);
		}

		public int EventId;

		public int PreviousId = -1;

		public int Time;

		public EventScopeKind Kind;

		public Action<int> WhenDoneCallbackServer;

		private int _eventClass;

		private IEventContent _content;
	}
}
