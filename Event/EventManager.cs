using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Achievements;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Playback;
using Hoplon.Logging;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Event
{
	public class EventManager : GameHubBehaviour, ICleanupListener, IKeyFrameParser, IEventManagerDispatcher
	{
		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				if (this._serverLine.Count > this.maxCount)
				{
					EventManager.Log.DebugFormat("Greatest server line={0}", new object[]
					{
						this.maxCount = this._serverLine.Count
					});
				}
				while (this._serverLine.Count > 0)
				{
					EventData data = this._serverLine[0];
					this._serverLine.RemoveAt(0);
					this.InternalTrigger(data);
				}
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
				this._eventDispatcher.Send(data);
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
			case EventScopeKind.Effect:
				this.Effects.Trigger(data);
				break;
			case EventScopeKind.Announcer:
			{
				AnnouncerEvent announcerEvent = (AnnouncerEvent)data.Content;
				this.Annoucer.Trigger(announcerEvent, data.EventId);
				this.TryToIncrementAchievement(announcerEvent);
				break;
			}
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

		public static void FreeEventData(EventData data)
		{
			EventManager._eventDataPool.Push(data);
		}

		public static EventData GetEventDataFromPool()
		{
			if (EventManager._eventDataPool.Count > 0)
			{
				return EventManager._eventDataPool.Pop();
			}
			return new EventData();
		}

		public void Process(BitStream stream)
		{
			EventData eventDataFromPool = EventManager.GetEventDataFromPool();
			eventDataFromPool.Reset();
			eventDataFromPool.ReadFromBitStream(stream);
			this.Trigger(eventDataFromPool);
		}

		public bool RewindProcess(IFrame frame)
		{
			EventData eventData = new EventData();
			eventData.ReadFromBitStream(frame.GetReadData());
			if (frame.PreviousFrameId != -1)
			{
				return true;
			}
			switch (eventData.Kind)
			{
			case EventScopeKind.Player:
			case EventScopeKind.Bot:
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
				break;
			}
			case EventScopeKind.Effect:
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
				break;
			case EventScopeKind.Pickup:
			{
				PickupDropEvent pickupDropEvent = eventData.Content as PickupDropEvent;
				if (pickupDropEvent != null && this.Pickups.GetPickUpByEventID(eventData.EventId) != null)
				{
					PickupRemoveEvent pickup = new PickupRemoveEvent
					{
						PickupId = PickupManager.GetPickupId(eventData.EventId),
						Reason = SpawnReason.Replay
					};
					this.Pickups.Trigger(pickup, -1);
				}
				break;
			}
			}
			return false;
		}

		public void FastForward(IFrame frame, IFrameProcessContext ctx)
		{
			EventData eventData = new EventData();
			eventData.ReadFromBitStream(frame.GetReadData());
			if (frame.PreviousFrameId == -1)
			{
				ctx.AddToExecutionQueue(frame.FrameId);
				return;
			}
			if (ctx.RemoveFromExecutionQueue(frame.PreviousFrameId))
			{
				return;
			}
			ctx.AddToExecutionQueue(frame.FrameId);
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
				this._serverDispatcher.SendSnapshot(to, this.Type.Convert(), eventData.EventId, eventData.PreviousId, eventData.Time, writeStream.ToArray());
				EventManager.Log.DebugFormat("Sending full frame event={0}", new object[]
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
			this._serverDispatcher.SendFrame(this.Type.Convert(), true, e.EventId, e.PreviousId, writeStream.ToArray());
		}

		private void TryToIncrementAchievement(AnnouncerEvent announcerEvent)
		{
			if (GameHubBehaviour.Hub.Net.IsServer() || GameHubBehaviour.Hub.User.IsNarrator)
			{
				return;
			}
			if (EventManager.IsQuadricide(announcerEvent))
			{
				this.IncrementQuadricideAchievement();
			}
		}

		private static bool IsQuadricide(AnnouncerEvent announcerEvent)
		{
			switch (announcerEvent.AnnouncerEventKind)
			{
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayer:
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayerWithAssists:
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironment:
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironmentWithAssists:
				if (announcerEvent.KillerTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam && announcerEvent.CurrentKillingSpree == 4)
				{
					return true;
				}
				break;
			}
			return false;
		}

		private void IncrementQuadricideAchievement()
		{
			EventManager.Log.Info("IncrementQuadricideAchievement");
			AchievementIncrement achievementIncrement = new AchievementIncrement
			{
				AchievementObjective = 5,
				Increment = 1
			};
			ObservableExtensions.Subscribe<Unit>(this._diContainer.Resolve<IIncrementAchievement>().Increment(new AchievementIncrement[]
			{
				achievementIncrement
			}));
		}

		public static readonly BitLogger Log = new BitLogger(typeof(EventManager));

		private List<EventData> _serverLine = new List<EventData>();

		[Inject]
		private IServerPlaybackDispatcher _serverDispatcher;

		[Inject]
		private IEventManagerDispatcher _eventDispatcher;

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private ILogger<EventManager> _logger;

		public EffectsManager Effects;

		public PlayerSpawnManager Players;

		public BotAISpawnManager Bots;

		public PickupManager Pickups;

		public AnnouncerManager Annoucer;

		public PlayerPing PlayerPing;

		private int maxCount;

		private Dictionary<int, EventData> _buffer = new Dictionary<int, EventData>();

		private static Stack<EventData> _eventDataPool = new Stack<EventData>(1024);

		private readonly byte[] SingleTo = new byte[1];
	}
}
