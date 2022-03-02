using System;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tournaments.API;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Tournaments.Infra
{
	public class SwordfishTournamentSeasonProvider : ITournamentSeasonProvider
	{
		public SwordfishTournamentSeasonProvider(ITournament tournamentService, ILogger<SwordfishTournamentSeasonProvider> logger)
		{
			this._tournamentService = tournamentService;
			this._logger = logger;
		}

		public IObservable<TournamentSeason> GetCurrentSeason()
		{
			return Observable.Select<TournamentSeason, TournamentSeason>(SwordfishObservable.FromSwordfishCall<TournamentSeason>(delegate(SwordfishClientApi.ParameterizedCallback<TournamentSeason> success, SwordfishClientApi.ErrorCallback error)
			{
				this._tournamentService.GetCurrentSeason(null, success, error);
			}), new Func<TournamentSeason, TournamentSeason>(this.ConvertSeason));
		}

		private TournamentSeason ConvertSeason(TournamentSeason sfTournamentSeason)
		{
			if (sfTournamentSeason == null)
			{
				this._logger.Error("A current Tournament Season could not be retrieved from Swordfish. As such, tournament related operations on this client may become unstable. Please make sure there is at least one season registered that has started and has still not ended and then restart the game.");
				return null;
			}
			return new TournamentSeason
			{
				Id = sfTournamentSeason.Id,
				Number = sfTournamentSeason.Number,
				EndDate = sfTournamentSeason.EndDate,
				StartDate = sfTournamentSeason.StartDate
			};
		}

		private readonly ITournament _tournamentService;

		private readonly ILogger<SwordfishTournamentSeasonProvider> _logger;
	}
}
