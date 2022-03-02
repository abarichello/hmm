using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using ClientAPI.Utils;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavymetalMachines.Player.Infra
{
	public class SwordfishGetPublisherUserName : IGetPublisherUserName
	{
		public SwordfishGetPublisherUserName(IUser user)
		{
			this._user = user;
		}

		public IObservable<string> Get(string universalId)
		{
			if (string.IsNullOrEmpty(universalId))
			{
				return Observable.Return<string>(string.Empty);
			}
			if (UniversalIdUtil.IsXboxLiveId(universalId))
			{
				return SwordfishObservable.FromSwordfishCall<string>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
				{
					this._user.GetXboxLiveOnlineId(null, universalId, success, error);
				});
			}
			if (UniversalIdUtil.IsPsnId(universalId))
			{
				return SwordfishObservable.FromSwordfishCall<string>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
				{
					this._user.GetPsnOnlineId(null, universalId, success, error);
				});
			}
			return Observable.Return<string>(string.Empty);
		}

		private readonly IUser _user;
	}
}
