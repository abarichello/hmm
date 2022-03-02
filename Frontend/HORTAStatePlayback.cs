using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class HORTAStatePlayback : GameHubBehaviour
	{
		public bool Running { get; set; }

		internal void Init(HORTAComponent comp, IMatchBuffer matchBuffer)
		{
			this.Component = comp;
			this._buffer = matchBuffer;
			this._bufferReader = new IndexedMatchBufferReader(this._buffer);
		}

		private void Awake()
		{
			this.Running = false;
		}

		private void Update()
		{
			if (!this.Running)
			{
				return;
			}
			this.UpdateFrames(new FrameCheck(this.CheckFrameOlderThanCurrentTime));
			if (this._bufferReader.FramesLeft == 0)
			{
				this.Component.EndGame();
				this.Running = false;
			}
		}

		private void UpdateFrames(FrameCheck check)
		{
			IFrame frame;
			while (this._bufferReader.ReadNext(check, out frame))
			{
				this.ProcessKeyFrame(frame);
			}
		}

		private void ProcessKeyFrame(IFrame frame)
		{
			HORTAStatePlayback.Log.DebugFormat("Processing state frame={0} time={1}", new object[]
			{
				frame.FrameId,
				frame.Time,
				(StateType)frame.Type
			});
			IKeyStateParser stateParser = PlaybackManager.GetStateParser((StateType)frame.Type);
			if (stateParser != null)
			{
				stateParser.Update(frame.GetReadData());
				return;
			}
			HORTAStatePlayback.Log.WarnFormat("Can't process unknown StateType:{0}. Skipping this state.", new object[]
			{
				(StateType)frame.Type
			});
		}

		internal void RunInitialStates()
		{
			this.UpdateFrames(new FrameCheck(this.CheckFrameOnTimeZero));
		}

		private bool CheckFrameOlderThanCurrentTime(IFrame frame)
		{
			return frame.Time <= GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private bool CheckFrameOnTimeZero(IFrame frame)
		{
			return frame.Time == 0;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTAStatePlayback));

		public HORTAComponent Component;

		private IMatchBuffer _buffer;

		private IMatchBufferReader _bufferReader;
	}
}
