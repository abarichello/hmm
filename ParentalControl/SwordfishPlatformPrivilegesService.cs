using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.ParentalControl
{
	public class SwordfishPlatformPrivilegesService : IPlatformPrivilegesService
	{
		public SwordfishPlatformPrivilegesService(ISwordfishWsService swordfishWsService, IUser userServices)
		{
			this._swordfishWsService = swordfishWsService;
			this._userServices = userServices;
		}

		public IObservable<bool> CheckPrivilege(Privileges privilege)
		{
			int privilegeId = this.GetPrivilegeId(privilege);
			return this._swordfishWsService.Execute<bool>(delegate(SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
			{
				this._userServices.HavePrivilege(null, privilegeId, callback, errorCallback);
			});
		}

		private int GetPrivilegeId(Privileges privilege)
		{
			return Platform.Current.GetPrivilegeId(privilege);
		}

		private readonly ISwordfishWsService _swordfishWsService;

		private readonly IUser _userServices;
	}
}
