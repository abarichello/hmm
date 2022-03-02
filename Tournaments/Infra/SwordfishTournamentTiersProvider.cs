using System;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Tournaments.Infra
{
	public class SwordfishTournamentTiersProvider : ITournamentTiersProvider
	{
		public SwordfishTournamentTiersProvider(ITournament tournamentService)
		{
			this._tournamentService = tournamentService;
		}

		public IObservable<TournamentTier[]> Get()
		{
			return Observable.Select<Tier[], TournamentTier[]>(SwordfishObservable.FromSwordfishCall<Tier[]>(delegate(SwordfishClientApi.ParameterizedCallback<Tier[]> success, SwordfishClientApi.ErrorCallback error)
			{
				this._tournamentService.GetAllTiers(null, success, error);
			}), new Func<Tier[], TournamentTier[]>(this.ConvertToTournamentTiers));
		}

		private TournamentTier[] ConvertToTournamentTiers(Tier[] tiers)
		{
			TournamentTier[] array = new TournamentTier[tiers.Length];
			for (int i = 0; i < tiers.Length; i++)
			{
				Tier tier = tiers[i];
				array[i] = this.ConverTournamentTier(tier);
			}
			return array;
		}

		private TournamentTier ConverTournamentTier(Tier tier)
		{
			return new TournamentTier
			{
				Name = tier.Name,
				Id = tier.Id,
				QueuName = ((TournamentTierBag)((JsonSerializeable<!0>)tier.Bag)).QueueName
			};
		}

		private readonly ITournament _tournamentService;
	}
}
