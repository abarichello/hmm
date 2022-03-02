using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Playback;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Combat
{
	public class ScoreboardParser : KeyStateParser, IScoreboardDispatcher
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
			BombScoreboardState currentState = this._scoreBoard.CurrentState;
			bool isInOvertime = this._scoreBoard.IsInOvertime;
			this._scoreBoard.ReadFromBitStream(stream);
			this._gameTime.MatchTimer.ReadFromBitStream(stream);
			ScoreboardParser.Log.DebugFormat("Received scoreboard update={0} state={1} oldState={2}", new object[]
			{
				flag,
				this._scoreBoard.CurrentState,
				currentState
			});
			if (flag || currentState != this._scoreBoard.CurrentState)
			{
				ScoreboardParser.Log.InfoFormat("Current bomb game state changed to {0}", new object[]
				{
					this._scoreBoard.CurrentState
				});
				this._bombManager.PhaseChanged();
			}
			this._bombManager.MatchUpdated();
			if (!isInOvertime && this._scoreBoard.IsInOvertime)
			{
				this._bombManager.OvertimeStarted();
			}
		}

		public void Send()
		{
			BitStream stream = base.GetStream();
			stream.WriteBool(false);
			this._scoreBoard.WriteToBitStream(stream);
			GameHubObject.Hub.GameTime.MatchTimer.WriteToBitStream(stream);
			this._serverDispatcher.SendFrame(this.Type.Convert(), true, this._serverDispatcher.GetNextFrameId(), -1, stream.ToArray());
		}

		public void SendFull(byte to)
		{
			BitStream stream = base.GetStream();
			stream.WriteBool(true);
			this._scoreBoard.WriteToBitStream(stream);
			GameHubObject.Hub.GameTime.MatchTimer.WriteToBitStream(stream);
			this._serverDispatcher.SendSnapshot(to, this.Type.Convert(), this._serverDispatcher.GetNextFrameId(), -1, GameHubObject.Hub.GameTime.GetPlaybackTime(), stream.ToArray());
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(ScoreboardParser));

		[Inject]
		protected IScoreBoard _scoreBoard;

		[Inject]
		protected IBombManager _bombManager;

		[Inject]
		protected new IGameTime _gameTime;
	}
}
