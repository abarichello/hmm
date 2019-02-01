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
			if (this._buffer.FramesLeft == 0)
			{
				this.Component.EndGame();
				this.Running = false;
			}
		}

		private void UpdateFrames(FrameCheck check)
		{
			IFrame frame;
			while (this._buffer.ReadNext(check, out frame))
			{
				this.ProcessKeyFrame(frame);
			}
		}

		private void ProcessKeyFrame(IFrame frame)
		{
			KeyStateParser stateParser = PlaybackManager.GetStateParser((StateType)frame.Type);
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
	}
}
