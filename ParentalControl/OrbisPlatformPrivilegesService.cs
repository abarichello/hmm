using System;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.ParentalControl
{
	public class OrbisPlatformPrivilegesService : IPlatformPrivilegesService
	{
		public OrbisPlatformPrivilegesService(IGetAllChatRestrictionIsEnabled getAllChatRestrictionIsEnabled, IGetUGCRestrictionIsEnabled getUgcRestrictionIsEnabled, IChatRestrictionDialogPresenter chatRestrictionDialogPresenter, IUGCRestrictionDialogPresenter ugcRestrictionDialogPresenter, ILogger<OrbisPlatformPrivilegesService> logger)
		{
			this._getAllChatRestrictionIsEnabled = getAllChatRestrictionIsEnabled;
			this._getUgcRestrictionIsEnabled = getUgcRestrictionIsEnabled;
			this._chatRestrictionDialogPresenter = chatRestrictionDialogPresenter;
			this._ugcRestrictionDialogPresenter = ugcRestrictionDialogPresenter;
			this._logger = logger;
		}

		public IObservable<bool> CheckPrivilege(Privileges privilege)
		{
			return Observable.Defer<bool>(delegate()
			{
				if (privilege == 6)
				{
					return this.ShowUgcRestrictionDialog();
				}
				if (privilege != 2)
				{
					return Observable.Return(true);
				}
				return this.ShowChatRestrictionDialog();
			});
		}

		private IObservable<bool> ShowChatRestrictionDialog()
		{
			bool globalRestriction = this._getAllChatRestrictionIsEnabled.GetGlobalRestriction();
			if (globalRestriction)
			{
				return Observable.Select<Unit, bool>(Observable.DoOnSubscribe<Unit>(this._chatRestrictionDialogPresenter.Show(), delegate()
				{
					this._logger.Info("Showing chat restriction dialog because global restriction is enabled.");
				}), (Unit _) => false);
			}
			return Observable.DoOnSubscribe<bool>(Observable.Return(true), delegate()
			{
				this._logger.Info("Not showing chat restriction dialog because global restriction is not enabled.");
			});
		}

		private IObservable<bool> ShowUgcRestrictionDialog()
		{
			bool globalRestriction = this._getUgcRestrictionIsEnabled.GetGlobalRestriction();
			if (globalRestriction)
			{
				return Observable.Select<bool, bool>(this._ugcRestrictionDialogPresenter.ShowAndReturnIsRestricted(), (bool isRestricted) => !isRestricted);
			}
			return Observable.Return(true);
		}

		private readonly IGetAllChatRestrictionIsEnabled _getAllChatRestrictionIsEnabled;

		private readonly IGetUGCRestrictionIsEnabled _getUgcRestrictionIsEnabled;

		private readonly IChatRestrictionDialogPresenter _chatRestrictionDialogPresenter;

		private readonly IUGCRestrictionDialogPresenter _ugcRestrictionDialogPresenter;

		private readonly ILogger<OrbisPlatformPrivilegesService> _logger;
	}
}
