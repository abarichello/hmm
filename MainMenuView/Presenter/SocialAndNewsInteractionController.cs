using System;
using HeavyMetalMachines.PlayerSummary;
using HeavyMetalMachines.Social.Friends.Presenting.FriendsList;
using HeavyMetalMachines.VFX;
using UniRx;

namespace HeavyMetalMachines.MainMenuView.Presenter
{
	public class SocialAndNewsInteractionController : ISocialAndNewsInteractionController
	{
		public SocialAndNewsInteractionController(IFriendsListPresenter friendsListPresenter, IPlayerSummaryPresenter playerSummaryPresenter)
		{
			this._friendsListPresenter = friendsListPresenter;
			this._playerSummaryPresenter = playerSummaryPresenter;
			this._disposables = new CompositeDisposable();
		}

		public void Initialize()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._friendsListPresenter.ObserveShow(), delegate(Unit _)
			{
				SocialAndNewsInteractionController.TryShowChatWindow();
			}));
			this._disposables.Add(disposable);
			disposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._friendsListPresenter.ObserveHide(), delegate(Unit _)
			{
				SocialAndNewsInteractionController.TryCloseChatWindow();
			}));
			this._disposables.Add(disposable);
			disposable = ObservableExtensions.Subscribe<Unit>(this._playerSummaryPresenter.UINavigationOnFocusGain());
			this._disposables.Add(disposable);
		}

		private static void TryShowChatWindow()
		{
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
		}

		private static void TryCloseChatWindow()
		{
			SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<SocialModalGUI>();
		}

		private void TryCloseSocialWindow()
		{
			if (this._friendsListPresenter.IsVisible)
			{
				SingletonMonoBehaviour<PanelController>.Instance.ToggleModalWindow<SocialModalGUI>();
			}
		}

		public void Dispose()
		{
			this._disposables.Dispose();
		}

		private readonly IFriendsListPresenter _friendsListPresenter;

		private readonly IPlayerSummaryPresenter _playerSummaryPresenter;

		private readonly CompositeDisposable _disposables;
	}
}
