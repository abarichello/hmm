using System;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Login;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishUserAccessControlService : IUserAccessControlService
	{
		public SwordfishUserAccessControlService(SwordfishClientApi swordfishClientApi)
		{
			this._swordfishClientApi = swordfishClientApi;
		}

		public IObservable<string> ObserveUserAccessClosed()
		{
			return Observable.Select<UserAccessControlMessage, string>(Observable.FromEvent<SwordfishClientApi.UserAccessControlDelegate, UserAccessControlMessage>((Action<UserAccessControlMessage> handler) => delegate(UserAccessControlMessage message)
			{
				handler(message);
			}, delegate(SwordfishClientApi.UserAccessControlDelegate handler)
			{
				this._swordfishClientApi.UserAccessControlCallback += handler;
			}, delegate(SwordfishClientApi.UserAccessControlDelegate handler)
			{
				this._swordfishClientApi.UserAccessControlCallback -= handler;
			}), (UserAccessControlMessage message) => message.Message);
		}

		private readonly SwordfishClientApi _swordfishClientApi;
	}
}
