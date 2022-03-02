using System;
using System.Globalization;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.Battlepass
{
	public class SwordfishGetBattlepassSeasonProvider : IGetBattlepassSeasonProvider
	{
		public SwordfishGetBattlepassSeasonProvider(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<Unit> FetchSeason()
		{
			return Observable.AsUnitObservable<BattlepassSeason>(Observable.Do<BattlepassSeason>(Observable.Select<SerializableBattlepassConfiguration, BattlepassSeason>(this._customWs.ExecuteAsObservable("GetBattlepassSeasonDate", string.Empty), new Func<SerializableBattlepassConfiguration, BattlepassSeason>(this.ConvertToBattlepassSeason)), new Action<BattlepassSeason>(this.SetBattlepassSeason)));
		}

		private BattlepassSeason ConvertToBattlepassSeason(SerializableBattlepassConfiguration sfBattlepassConfiguration)
		{
			return new BattlepassSeason
			{
				StartSeasonDateTime = DateTime.Parse(sfBattlepassConfiguration.StartSeasonTimeString, CultureInfo.InvariantCulture.DateTimeFormat),
				EndSeasonDateTime = DateTime.Parse(sfBattlepassConfiguration.EndSeasonTimeString, CultureInfo.InvariantCulture.DateTimeFormat)
			};
		}

		public BattlepassSeason GetSeason()
		{
			return this._battlepassSeason;
		}

		private void SetBattlepassSeason(BattlepassSeason battlepassSeason)
		{
			this._battlepassSeason = battlepassSeason;
		}

		private readonly ICustomWS _customWs;

		private BattlepassSeason _battlepassSeason;
	}
}
