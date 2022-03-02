using System;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.Social.Avatar.Infra
{
	public class SwordfishPlayerAvatarProvider : IPlayerAvatarProvider
	{
		public SwordfishPlayerAvatarProvider(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<Guid> GetAvatarItemIdFromPlayerId(long playerId)
		{
			return Observable.Select<NetResult, Guid>(this._customWs.ExecuteAsObservable("GetAvatarItemIdFromPlayerId", playerId.ToString()), (NetResult result) => new Guid(result.Msg));
		}

		private readonly ICustomWS _customWs;
	}
}
