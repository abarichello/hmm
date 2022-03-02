using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface ICompetitiveP2pService
	{
		IObservable<PlayerCompetitiveState> OnMyPlayerCompetitiveStateReceived { get; }

		void SetPlayerCompetitiveState(PlayerCompetitiveState playerCompetitiveState);
	}
}
