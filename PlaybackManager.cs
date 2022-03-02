using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Counselor;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines
{
	[RemoteClass]
	public class PlaybackManager : GameHubBehaviour, MatchController.GameOverMessage.IGameOverListener, IServerPlaybackDispatcher, IBitComponent
	{
		public static IKeyStateParser GetStateParser(StateType index)
		{
			return PlaybackManager.StateParsers[index];
		}

		private void Awake()
		{
			PlaybackManager.StateParsers[StateType.Players] = GameHubBehaviour.Hub.Players;
			PlaybackManager.StateParsers[StateType.Teams] = GameHubBehaviour.Hub.Teams;
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._recorder = new MatchRecorder();
			}
		}

		public int NextId()
		{
			this._currentFrameId++;
			return this._currentFrameId;
		}

		public void SendKeyFrame(byte keyframetype, bool reliable, int frameId, int previousFrameId, byte[] data)
		{
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._recorder.KeyFrames.AddFrame(keyframetype, frameId, previousFrameId, playbackTime, data);
			IPlaybackManagerDispatch playbackManagerDispatch = (!reliable) ? this.Dispatch(GameHubBehaviour.Hub.SendAll) : this.DispatchReliable(GameHubBehaviour.Hub.SendAll);
			playbackManagerDispatch.AddKeyframe(keyframetype, frameId, previousFrameId, playbackTime, data);
		}

		public void SendFullKeyFrame(byte to, byte keyframetype, int frameId, int previousFrameId, int time, byte[] data)
		{
			if (to == 1)
			{
				this._recorder.KeyFrames.AddFrame(keyframetype, frameId, previousFrameId, time, data);
				this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).AddKeyframe(keyframetype, frameId, previousFrameId, time, data);
				return;
			}
			this.DispatchReliable(new byte[]
			{
				to
			}).AddKeyframe(keyframetype, frameId, previousFrameId, time, data);
		}

		public void SendState(StateType stateType, byte[] data)
		{
			this._recorder.States.AddFrame((byte)stateType, this._currentStateId++, -1, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), data);
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).UpdateState((byte)stateType, data);
		}

		public void SendFullState(byte to, StateType stateType, byte[] data)
		{
			if (to == 1)
			{
				this._recorder.States.AddFrame((byte)stateType, this._currentStateId++, -1, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), data);
				this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).UpdateState((byte)stateType, data);
				return;
			}
			this.DispatchReliable(new byte[]
			{
				to
			}).UpdateState((byte)stateType, data);
		}

		[RemoteMethod]
		private void AddKeyframe(byte keyframetype, int frameId, int previousFrameId, int time, byte[] data)
		{
			this._playback.AddStateKeyFrame(keyframetype, frameId, previousFrameId, time, data);
		}

		[RemoteMethod]
		private void UpdateState(byte statetype, byte[] data)
		{
			BitStream data2 = new BitStream(data);
			PlaybackManager.GetStateParser((StateType)statetype).Update(data2);
		}

		public void OnGameOver(MatchController.GameOverMessage msg)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._recorder.SaveMatch();
			}
		}

		public int GetNextFrameId()
		{
			return this.NextId();
		}

		public void SendFrame(FrameKind kind, bool reliable, int frameId, int previousFrameId, byte[] data)
		{
			if (kind.IsKeyFrameType())
			{
				KeyFrameType keyframetype = kind.ToKeyFrameType();
				this.SendKeyFrame((byte)keyframetype, reliable, frameId, previousFrameId, data);
				return;
			}
			StateType stateType = kind.ToStateType();
			this.SendState(stateType, data);
		}

		public void SendSnapshot(byte to, FrameKind kind, int frameId, int previousFrameId, int time, byte[] data)
		{
			if (kind.IsKeyFrameType())
			{
				KeyFrameType keyframetype = kind.ToKeyFrameType();
				this.SendFullKeyFrame(to, (byte)keyframetype, frameId, previousFrameId, time, data);
				return;
			}
			StateType stateType = kind.ToStateType();
			this.SendFullState(to, stateType, data);
		}

		private int OID
		{
			get
			{
				if (!this._identifiable)
				{
					this._identifiable = base.GetComponent<Identifiable>();
				}
				return this._identifiable.ObjId;
			}
		}

		public byte Sender { get; set; }

		public IPlaybackManagerAsync Async()
		{
			return this.Async(0);
		}

		public IPlaybackManagerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PlaybackManagerAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPlaybackManagerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlaybackManagerDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPlaybackManagerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlaybackManagerDispatch(this.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		protected IFuture Delayed
		{
			get
			{
				return this._delayed;
			}
		}

		protected void Delay(IFuture future)
		{
			this._delayed = future;
		}

		public object Invoke(int classId, short methodId, object[] args, BitStream bitstream = null)
		{
			this._delayed = null;
			if (methodId == 8)
			{
				byte keyframetype = bitstream.ReadByte();
				int frameId = bitstream.ReadCompressedInt();
				int previousFrameId = bitstream.ReadCompressedInt();
				int time = bitstream.ReadCompressedInt();
				byte[] data = bitstream.CachedReadByteArray();
				this.AddKeyframe(keyframetype, frameId, previousFrameId, time, data);
				ByteArrayCache.Free(data);
				return null;
			}
			if (methodId != 9)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			byte statetype = bitstream.ReadByte();
			byte[] data2 = bitstream.ReadByteArray();
			this.UpdateState(statetype, data2);
			return null;
		}

		[Inject]
		private IPlayback _playback;

		public static readonly TransformParser TransformStates = new TransformParser();

		public static readonly StateDataParser CombatStates = new StateDataParser();

		public static readonly CombatFeedbackParser CombatFeedbacks = new CombatFeedbackParser();

		public static readonly ModifierEventParser ModifierEvent = new ModifierEventParser();

		public static readonly CollisionParser Collision = new CollisionParser();

		public static readonly GadgetLevelParser GadgetLevel = new GadgetLevelParser();

		public static readonly BombInstanceParser BombInstance = new BombInstanceParser();

		public static readonly BombDetonationParser BombDetonation = new BombDetonationParser();

		public static readonly GadgetEventParser GadgetEvent = new GadgetEventParser();

		public static readonly StatsParser PlayerStats = new StatsParser();

		public static readonly ScoreboardParser Scoreboard = new ScoreboardParser();

		public static readonly CounselorParser Counselor = new CounselorParser();

		private static readonly Dictionary<StateType, IKeyStateParser> StateParsers = new Dictionary<StateType, IKeyStateParser>
		{
			{
				StateType.PlayerStats,
				PlaybackManager.PlayerStats
			},
			{
				StateType.Scoreboard,
				PlaybackManager.Scoreboard
			},
			{
				StateType.Counselor,
				PlaybackManager.Counselor
			}
		};

		private MatchRecorder _recorder;

		private int _currentStateId;

		private int _currentFrameId;

		public const int StaticClassId = 1009;

		private Identifiable _identifiable;

		[ThreadStatic]
		private PlaybackManagerAsync _async;

		[ThreadStatic]
		private PlaybackManagerDispatch _dispatch;

		private IFuture _delayed;
	}
}
