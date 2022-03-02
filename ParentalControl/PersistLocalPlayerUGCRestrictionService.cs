using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.ParentalControl
{
	public class PersistLocalPlayerUGCRestrictionService : IPersistLocalPlayerUGCRestrictionService
	{
		public PersistLocalPlayerUGCRestrictionService(IPlayerToAllPlayersRestriction playerToAllPlayersRestriction, ILogger<PersistLocalPlayerUGCRestrictionService> logger)
		{
			this._playerToAllPlayersRestriction = playerToAllPlayersRestriction;
			this._logger = logger;
		}

		public IObservable<Unit> Persist(bool ugcEnabled)
		{
			return Observable.Do<Unit>(SwordfishObservable.FromSwordfishCall(delegate(SwordfishClientApi.Callback success, SwordfishClientApi.ErrorCallback error)
			{
				this._playerToAllPlayersRestriction.SetPlayerRestriction(null, ugcEnabled, success, error);
			}), delegate(Unit _)
			{
				this._logger.DebugFormat("Persist UGC value = {0}", new object[]
				{
					ugcEnabled
				});
			});
		}

		private readonly IPlayerToAllPlayersRestriction _playerToAllPlayersRestriction;

		private readonly ILogger<PersistLocalPlayerUGCRestrictionService> _logger;
	}
}
