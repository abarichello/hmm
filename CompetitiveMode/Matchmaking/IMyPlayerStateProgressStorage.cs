using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface IMyPlayerStateProgressStorage
	{
		PlayerCompetitiveState? InitialState { get; set; }

		PlayerCompetitiveState? FinalState { get; set; }

		IObservable<PlayerCompetitiveProgress> OnProgressSet { get; }

		void Clear();
	}
}
