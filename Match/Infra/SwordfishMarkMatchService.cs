using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Arena.DataTransferObjects;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.Match.Infra
{
	public class SwordfishMarkMatchService : IMarkMatchService
	{
		public SwordfishMarkMatchService(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<Unit> MarkPlayerHasStarted(long playerId, MatchKind matchKind)
		{
			return this.GetCustomWsSubject(playerId, matchKind, "MarkPlayerHasStartedMatchKind");
		}

		public IObservable<Unit> MarkPlayerHasDone(long playerId, MatchKind matchKind)
		{
			return this.GetCustomWsSubject(playerId, matchKind, "MarkPlayerHasDoneMatchKind");
		}

		public IObservable<Unit> GetCustomWsSubject(long playerId, MatchKind matchKind, string customWsApiName)
		{
			MarkMatchKindBag markMatchBag = new MarkMatchKindBag();
			markMatchBag.PlayerId = playerId;
			markMatchBag.SerializedMachKind = matchKind;
			return SwordfishObservable.FromNetResultSwordfishCall(delegate(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
			{
				this._customWs.ExecuteCustomWSWithReturn(null, customWsApiName, markMatchBag.ToString(), onSuccess, onError);
			});
		}

		private ICustomWS _customWs;
	}
}
