using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using UniRx;

namespace HeavyMetalMachines.Tournaments.Infra
{
	public class SwordfishTournamentProvider : ITournamentProvider
	{
		public SwordfishTournamentProvider(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<TournamentConfiguration[]> GetAllActive()
		{
			IObservable<SerializableTournamentConfigurationCollection> observable = this._customWs.ExecuteAsObservable("GetAllActiveTournaments", string.Empty);
			if (SwordfishTournamentProvider.<>f__mg$cache0 == null)
			{
				SwordfishTournamentProvider.<>f__mg$cache0 = new Func<SerializableTournamentConfigurationCollection, TournamentConfiguration[]>(SwordfishTournamentProvider.Deserialize);
			}
			return Observable.Select<SerializableTournamentConfigurationCollection, TournamentConfiguration[]>(observable, SwordfishTournamentProvider.<>f__mg$cache0);
		}

		public IObservable<Tournament[]> GetAllActiveWithTeamStatus(Guid teamId)
		{
			IObservable<SerializableTournamentConfigurationAndTeamStatusCollection> observable = this._customWs.ExecuteAsObservable("GetAllActiveTournamentTeamStatus", teamId.ToString());
			if (SwordfishTournamentProvider.<>f__mg$cache1 == null)
			{
				SwordfishTournamentProvider.<>f__mg$cache1 = new Func<SerializableTournamentConfigurationAndTeamStatusCollection, Tournament[]>(SwordfishTournamentProvider.Deserialize);
			}
			return Observable.Select<SerializableTournamentConfigurationAndTeamStatusCollection, Tournament[]>(observable, SwordfishTournamentProvider.<>f__mg$cache1);
		}

		private static TournamentConfiguration[] Deserialize(SerializableTournamentConfigurationCollection tournamentCollection)
		{
			IEnumerable<SerializableTournamentConfiguration> tournamentConfigurations = tournamentCollection.TournamentConfigurations;
			if (SwordfishTournamentProvider.<>f__mg$cache2 == null)
			{
				SwordfishTournamentProvider.<>f__mg$cache2 = new Func<SerializableTournamentConfiguration, TournamentConfiguration>(TournamentConversions.ToModel);
			}
			return tournamentConfigurations.Select(SwordfishTournamentProvider.<>f__mg$cache2).ToArray<TournamentConfiguration>();
		}

		private static Tournament[] Deserialize(SerializableTournamentConfigurationAndTeamStatusCollection tournamentCollection)
		{
			IEnumerable<SerializableTournamentConfigurationAndTeamStatus> tournamentConfigurationAndTeamStatuses = tournamentCollection.TournamentConfigurationAndTeamStatuses;
			if (SwordfishTournamentProvider.<>f__mg$cache3 == null)
			{
				SwordfishTournamentProvider.<>f__mg$cache3 = new Func<SerializableTournamentConfigurationAndTeamStatus, Tournament>(TournamentConversions.Deserialize);
			}
			return tournamentConfigurationAndTeamStatuses.Select(SwordfishTournamentProvider.<>f__mg$cache3).ToArray<Tournament>();
		}

		private readonly ICustomWS _customWs;

		[CompilerGenerated]
		private static Func<SerializableTournamentConfigurationCollection, TournamentConfiguration[]> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<SerializableTournamentConfigurationAndTeamStatusCollection, Tournament[]> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<SerializableTournamentConfiguration, TournamentConfiguration> <>f__mg$cache2;

		[CompilerGenerated]
		private static Func<SerializableTournamentConfigurationAndTeamStatus, Tournament> <>f__mg$cache3;
	}
}
