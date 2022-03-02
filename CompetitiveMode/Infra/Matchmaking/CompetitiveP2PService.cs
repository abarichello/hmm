using System;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Infra.Matchmaking
{
	public class CompetitiveP2PService : ICompetitiveP2pService
	{
		public IObservable<PlayerCompetitiveState> OnMyPlayerCompetitiveStateReceived
		{
			get
			{
				return this._onMyPlayerCompetitiveStateReceived;
			}
		}

		public void SetPlayerCompetitiveState(PlayerCompetitiveState playerCompetitiveState)
		{
			this._onMyPlayerCompetitiveStateReceived.OnNext(playerCompetitiveState);
		}

		private readonly Subject<PlayerCompetitiveState> _onMyPlayerCompetitiveStateReceived = new Subject<PlayerCompetitiveState>();
	}
}
