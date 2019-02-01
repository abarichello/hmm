using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class ScoreboardParser : KeyStateParser
	{
		public override StateType Type
		{
			get
			{
				return StateType.Scoreboard;
			}
		}

		public override void Update(BitStream stream)
		{
			bool flag = stream.ReadBool();
			BombScoreBoard.State currentState = GameHubObject.Hub.BombManager.ScoreBoard.CurrentState;
			bool isInOvertime = GameHubObject.Hub.BombManager.ScoreBoard.IsInOvertime;
			GameHubObject.Hub.BombManager.ScoreBoard.ReadFromBitStream(stream);
			GameHubObject.Hub.GameTime.MatchTimer.ReadFromBitStream(stream);
			if (flag || currentState != GameHubObject.Hub.BombManager.ScoreBoard.CurrentState)
			{
				GameHubObject.Hub.BombManager.PhaseChanged();
			}
			GameHubObject.Hub.BombManager.MatchUpdated();
			if (!isInOvertime && GameHubObject.Hub.BombManager.ScoreBoard.IsInOvertime)
			{
				GameHubObject.Hub.BombManager.OvertimeStarted();
			}
		}

		public void Send()
		{
			BitStream stream = base.GetStream();
			stream.WriteBool(false);
			GameHubObject.Hub.BombManager.ScoreBoard.WriteToBitStream(stream);
			GameHubObject.Hub.GameTime.MatchTimer.WriteToBitStream(stream);
			GameHubObject.Hub.PlaybackManager.SendState(this.Type, stream.ToArray());
		}

		public void SendFull(byte to)
		{
			BitStream stream = base.GetStream();
			stream.WriteBool(true);
			GameHubObject.Hub.BombManager.ScoreBoard.WriteToBitStream(stream);
			GameHubObject.Hub.GameTime.MatchTimer.WriteToBitStream(stream);
			GameHubObject.Hub.PlaybackManager.SendFullState(to, this.Type, stream.ToArray());
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ScoreboardParser));
	}
}
