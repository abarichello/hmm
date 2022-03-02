using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Players.Business;
using UniRx;

namespace HeavymetalMachines.Player.Infra
{
	public class SkipSwordfishLocalPlayerRestrictionsService : ILocalPlayerRestrictionsService
	{
		public IObservable<PlayerToPlayerRestriction[]> GetRestrictions()
		{
			return Observable.Return<PlayerToPlayerRestriction[]>(new PlayerToPlayerRestriction[0]);
		}

		public IObservable<PlayerToPlayerRestriction[]> GetRestrictions(long[] playerIds)
		{
			return Observable.Return<PlayerToPlayerRestriction[]>(new PlayerToPlayerRestriction[0]);
		}

		public IObservable<Unit> AddRestriction(PlayerToPlayerRestrictionType restrictionType, long playerId)
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> RemoveRestriction(PlayerToPlayerRestrictionType restrictionType, long playerId)
		{
			return Observable.ReturnUnit();
		}
	}
}
