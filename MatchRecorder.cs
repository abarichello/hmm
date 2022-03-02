using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class MatchRecorder : GameHubObject
	{
		public MatchRecorder()
		{
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.DisableRecorder))
			{
				this.States = new NullMatchBuffer();
				this.KeyFrames = new NullMatchBuffer();
				return;
			}
			this.States = new MemoryMatchBuffer(10485760, 131072);
			this.KeyFrames = new MemoryMatchBuffer(52428800, 131072);
			this._enabled = true;
		}

		public IMatchBuffer States { get; private set; }

		public IMatchBuffer KeyFrames { get; private set; }

		public void SaveMatch()
		{
			if (!this._enabled)
			{
				MatchRecorder.Log.Debug("Match recording disabled.");
				return;
			}
			MemoryMatchBuffer memoryMatchBuffer = (MemoryMatchBuffer)this.States;
			MemoryMatchBuffer memoryMatchBuffer2 = (MemoryMatchBuffer)this.KeyFrames;
			MatchRecorder.Log.DebugFormat("Match sizes states={0} frames={1}", new object[]
			{
				memoryMatchBuffer,
				memoryMatchBuffer2
			});
			MatchInformation match = new MatchInformation
			{
				Version = "Release.15.00.250",
				MatchId = GameHubObject.Hub.Swordfish.Connection.ServerMatchId,
				Data = GameHubObject.Hub.Match,
				States = this.States,
				KeyFrames = this.KeyFrames
			};
			string text = MatchFile.WriteFile(match, GameHubObject.Hub.Config.GetValue(ConfigAccess.HORTADestFolder));
			this._enabled = false;
			MatchRecorder.Log.DebugFormat("Match saved to file={0}", new object[]
			{
				text
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MatchRecorder));

		private const int InitialStateBufferSize = 10485760;

		private const int InitialKeyFrameBufferSize = 52428800;

		private const int InitialFrameCount = 131072;

		private bool _enabled;
	}
}
