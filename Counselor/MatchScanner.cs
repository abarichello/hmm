using System;
using HeavyMetalMachines.Combat;
using Hoplon.SensorSystem;
using Pocketverse;

namespace HeavyMetalMachines.Counselor
{
	public class MatchScanner : GameHubObject, IScanner
	{
		public MatchScanner(SensorController context, string matchPointName, string roundName, string bombDeliveryDeltaTimeId, string arenaIndexId)
		{
			this._matchPointId = context.GetHash(matchPointName);
			this._roundId = context.GetHash(roundName);
			this._bombDeliveryDeltaTimeId = context.GetHash(bombDeliveryDeltaTimeId);
			this._arenaIndexId = context.GetHash(arenaIndexId);
			this._context = context;
			GameHubObject.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
		}

		private void BombManagerOnListenToPhaseChange(BombScoreBoard.State state)
		{
			if (state == BombScoreBoard.State.BombDelivery)
			{
				int num = GameHubObject.Hub.BombManager.Rules.BombScoreTarget - 1;
				bool flag = GameHubObject.Hub.BombManager.ScoreBoard.BombScoreBlue >= num || GameHubObject.Hub.BombManager.ScoreBoard.BombScoreRed >= num;
				this._context.SetParameter(this._matchPointId, (!flag) ? 0f : 1f);
				this._context.SetParameter(this._roundId, (float)GameHubObject.Hub.BombManager.Round);
				this._bombDeliveryDeltaTime = (float)GameHubObject.Hub.GameTime.GetPlaybackTime();
			}
			this._context.SetParameter(this._arenaIndexId, (float)GameHubObject.Hub.Match.ArenaIndex);
		}

		public void UpdateContext(SensorController context)
		{
			float num;
			this._context.GetParameter(this._context.MainClockId, out num);
			this._context.SetParameter(this._bombDeliveryDeltaTimeId, (float)GameHubObject.Hub.GameTime.GetPlaybackTime() - this._bombDeliveryDeltaTime);
		}

		public void Reset()
		{
		}

		private int _matchPointId;

		private int _roundId;

		private int _bombDeliveryDeltaTimeId;

		private int _arenaIndexId;

		private float _bombDeliveryDeltaTime;

		private SensorController _context;
	}
}
