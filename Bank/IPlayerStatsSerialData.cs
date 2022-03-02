using System;
using HeavyMetalMachines.Playback.Snapshot;

namespace HeavyMetalMachines.Bank
{
	public interface IPlayerStatsSerialData : IBaseStreamSerialData<IPlayerStatsSerialData>
	{
		int TimedScrap { get; }

		int TotalScrapCollected { get; }

		int OtherScrap { get; }

		int ReliableScrap { get; }

		int ScrapSpent { get; }

		int ScrapCollected { get; }

		int Level { get; }

		int CreepKills { get; }

		int Kills { get; }

		int Deaths { get; }

		int Assists { get; }

		int BombsDelivered { get; }

		float DamageDealtToPlayers { get; }

		float DamageReceived { get; }

		float HealingProvided { get; }

		float BombPossessionTime { get; }

		float DebuffTime { get; }

		bool Disconnected { get; }
	}
}
