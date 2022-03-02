using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.ParentalControl
{
	public class DurangoUgcRestrictionDialogPresenter : IUGCRestrictionDialogPresenter
	{
		public DurangoUgcRestrictionDialogPresenter(IUser userService, ISwordfishWsService swordfishWsService)
		{
			this._userService = userService;
			this._swordfishWsService = swordfishWsService;
		}

		public IObservable<bool> ShowAndReturnIsRestricted()
		{
			return Observable.Select<bool, bool>(this._swordfishWsService.Execute<bool>(delegate(SwordfishClientApi.ParameterizedCallback<bool> success, SwordfishClientApi.ErrorCallback error)
			{
				this._userService.HavePrivilege(null, Platform.Current.GetPrivilegeId(6), success, error);
			}), (bool hasPrivilege) => !hasPrivilege);
		}

		private readonly IUser _userService;

		private readonly ISwordfishWsService _swordfishWsService;
	}
}
