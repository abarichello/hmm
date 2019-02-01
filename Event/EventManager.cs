using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using Pocketverse;
using Pocketverse.MuralContext;

namespace HeavyMetalMachines.Event
{
	public class EventManager : GameHubBehaviour, ICleanupListener, IKeyFrameParser
	{
		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				if (this._serverLine.Count > this.maxCount)
				{
				}
				while (this._serverLine.Count > 0)
				{
					EventData data = this._serverLine[0];
					this._serverLine.RemoveAt(0);
					this.InternalTrigger(data);
				}
				return;
			}
		}

		public int TriggerEvent(IEventContent content)
		{
			return this.TriggerEvent(content, null);
		}

		public int TriggerEvent(IEventContent content, Action<int> whenDone)
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || content == null)
			{
				return -1;
			}
			EventData eventData = new EventData();
			eventData.Content = content;
			eventData.EventId = GameHubBehaviour.Hub.PlaybackManager.NextId();
			eventData.Time = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			eventData.WhenDoneCallbackServer = whenDone;
			this._serverLine.Add(eventData);
			return eventData.EventId;
		}

		private void InternalTrigger(EventData data)
		{
			data.Content.EventTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (data.Kind != EventScopeKind.Effect && data.Kind != EventScopeKind.Pickup)
			{
				this.Send(data);
			}
			this.Trigger(data);
		}

		private void Trigger(EventData data)
		{
			if (data.Content == null)
			{
				return;
			}
			if (this._buffer.ContainsKey(data.EventId))
			{
				EventManager.Log.WarnFormat("Why triggering something that was already buffered?!", new object[0]);
				return;
			}
			switch (data.Kind)
			{
			case EventScopeKind.Player:
				this.Players.Trigger((PlayerEvent)data.Content, data.EventId);
				break;
			case EventScopeKind.Creep:
				this.Creeps.Trigger(data.Content, data.EventId);
				break;
			case EventScopeKind.Effect:
				this.Effects.Trigger(data);
				break;
			case EventScopeKind.Announcer:
				this.Annoucer.Trigger((AnnouncerEvent)data.Content, data.EventId);
				break;
			case EventScopeKind.Pickup:
				this.Pickups.Trigger(data.Content, data.EventId);
				break;
			case EventScopeKind.Bot:
				this.Bots.Trigger((BotAIEvent)data.Content, data.EventId);
				break;
			case EventScopeKind.Ping:
				this.PlayerPing.Trigger((PingEvent)data.Content, data.EventId);
				break;
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && data.Kind != EventScopeKind.Effect && data.Content.ShouldBuffer())
			{
				this.BufferEvent(data);
			}
			if (data.WhenDoneCallbackServer != null)
			{
				data.WhenDoneCallbackServer(data.EventId);
			}
		}

		public void ForgetEvent(int eventId)
		{
			EventData eventData;
			if (this._buffer.TryGetValue(eventId, out eventData))
			{
				this._buffer.Remove(eventId);
			}
			else
			{
				EventManager.Log.WarnFormat("Try to forget EventID {0} but it was not found", new object[]
				{
					eventId
				});
			}
		}

		public void CancelEvent(int eventId)
		{
			EventData eventData = this._serverLine.Find((EventData x) => x.EventId == eventId);
			if (eventData != null)
			{
				this._serverLine.Remove(eventData);
			}
		}

		public int CreateAndBufferEvent(IEventContent data)
		{
			EventData eventData = new EventData();
			eventData.EventId = GameHubBehaviour.Hub.PlaybackManager.NextId();
			eventData.Time = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			eventData.Content = data;
			this.BufferEvent(eventData);
			return eventData.EventId;
		}

		public void BufferEvent(EventData evt)
		{
			this._buffer[evt.EventId] = evt;
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this._buffer.Clear();
			this._serverLine.Clear();
		}

		public KeyFrameType Type
		{
			get
			{
				return KeyFrameType.ManagerEvent;
			}
		}

		public void Process(BitStream stream)
		{
			EventData eventData = new EventData();
			eventData.ReadFromBitStream(stream);
			this.Trigger(eventData);
		}

		public bool RewindProcess(IFrame frame)
		{
			EventData eventData = new EventData();
			eventData.ReadFromBitStream(frame.GetReadData());
			if (frame.PreviousFrameId != -1)
			{
				return true;
			}
			EventScopeKind kind = eventData.Kind;
			if (kind != EventScopeKind.Player && kind != EventScopeKind.Bot)
			{
				if (kind == EventScopeKind.Effect)
				{
					if (eventData.Content is EffectEvent && this.Effects.GetBaseFx(eventData.EventId) != null)
					{
						EffectRemoveEvent content = new EffectRemoveEvent
						{
							TargetEventId = eventData.EventId,
							DestroyReason = BaseFX.EDestroyReason.Default
						};
						eventData.PreviousId = eventData.EventId;
						eventData.Content = content;
						this.Effects.Trigger(eventData);
					}
				}
			}
			else
			{
				PlayerEvent playerEvent = eventData.Content as PlayerEvent;
				if (playerEvent != null)
				{
					PlayerEvent.Kind eventKind = playerEvent.EventKind;
					if (eventKind != PlayerEvent.Kind.Unspawn)
					{
						if (eventKind == PlayerEvent.Kind.Respawn)
						{
							playerEvent.EventKind = PlayerEvent.Kind.Unspawn;
						}
					}
					else
					{
						playerEvent.EventKind = PlayerEvent.Kind.Respawn;
					}
					this.Trigger(eventData);
				}
			}
			return false;
		}

		public void SendFullFrame(byte to)
		{
			this.SingleTo[0] = to;
			List<EventData> list = this._buffer.Values.ToList<EventData>();
			list.Sort(new Comparison<EventData>(this.EventsSort));
			for (int i = 0; i < list.Count; i++)
			{
				EventData eventData = list[i];
				BitStream writeStream = StaticBitStream.GetWriteStream();
				eventData.WriteToBitStream(writeStream);
				GameHubBehaviour.Hub.PlaybackManager.SendFullKeyFrame(to, this.Type, eventData.EventId, eventData.PreviousId, eventData.Time, writeStream.ToArray());
				EventManager.Log.InfoFormat("Sending full frame event={0}", new object[]
				{
					eventData
				});
			}
		}

		private int EventsSort(EventData x, EventData y)
		{
			return x.EventId.CompareTo(y.EventId);
		}

		public void Send(EventData e)
		{
			BitStream writeStream = StaticBitStream.GetWriteStream();
			e.WriteToBitStream(writeStream);
			GameHubBehaviour.Hub.PlaybackManager.SendKeyFrame(this.Type, true, e.EventId, e.PreviousId, writeStream.ToArray());
		}

		public static readonly BitLogger Log = new BitLogger(typeof(EventManager));

		private List<EventData> _serverLine = new List<EventData>();

		public EffectsManager Effects;

		public CreepSpawnManager Creeps;

		public PlayerSpawnManager Players;

		public BotAISpawnManager Bots;

		public PickupManager Pickups;

		public AnnouncerManager Annoucer;

		public PlayerPing PlayerPing;

		private int maxCount;

		private Dictionary<int, EventData> _buffer = new Dictionary<int, EventData>();

		private readonly byte[] SingleTo = new byte[1];
	}
}
