using System;
using System.Collections.Generic;
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

		public void Reset()
		{
			this.EventId = int.MinValue;
			this.Kind = EventScopeKind.None;
			this._eventClass = int.MinValue;
			this._content = null;
		}

		public static void FreeContent(IEventContent eventContent)
		{
			if (GameHubObject.Hub.Net.IsServer())
			{
				return;
			}
			if (eventContent == null)
			{
				return;
			}
			int key = EventData._classTypes[eventContent.GetType()];
			if (!EventData._eventContents.ContainsKey(key))
			{
				EventData._eventContents.Add(key, new Stack<IEventContent>());
			}
			EventData._eventContents[key].Push(eventContent);
		}

		public static IEventContent GetContentFromPool(int eventClass)
		{
			if (GameHubObject.Hub.Net.IsServer())
			{
				return (IEventContent)GameHubObject.Hub.Classes.CreateInstance(eventClass);
			}
			if (!EventData._eventContents.ContainsKey(eventClass))
			{
				EventData._eventContents.Add(eventClass, new Stack<IEventContent>());
			}
			if (EventData._eventContents[eventClass].Count > 0)
			{
				return EventData._eventContents[eventClass].Pop();
			}
			IEventContent eventContent = (IEventContent)GameHubObject.Hub.Classes.CreateInstance(eventClass);
			if (!EventData._classTypes.ContainsKey(eventContent.GetType()))
			{
				EventData._classTypes.Add(eventContent.GetType(), eventClass);
			}
			return eventContent;
		}

		public int EventClass
		{
			get
			{
				return this._eventClass;
			}
			set
			{
				this._eventClass = value;
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
				this._content = EventData.GetContentFromPool(this._eventClass);
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

		public static Dictionary<int, Stack<IEventContent>> _eventContents = new Dictionary<int, Stack<IEventContent>>(16);

		public static Dictionary<Type, int> _classTypes = new Dictionary<Type, int>(64);
	}
}
