using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.Presenting.Navigation;
using HeavyMetalMachines.Profile.View;
using HeavyMetalMachines.Social.Avatar.Business;
using HeavyMetalMachines.Social.Profile.Presenting;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Profile.Presenter
{
	public class LegacyProfilePresenter : IProfilePresenter, IPresenter
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.InitializeView();
			}), delegate(Unit _)
			{
				this._view.MainActivatable.SetActive(true);
			}), delegate(Unit _)
			{
				ActivatableExtensions.Deactivate(this._view.BackButton);
			}), delegate(Unit _)
			{
				ActivatableExtensions.Deactivate(this._view.AvatarChangeButton);
			}), delegate(Unit _)
			{
				this.ObserveProfileNodeEnter();
			}), delegate(Unit _)
			{
				this.UpdatePlayerTag();
			});
		}

		private void ObserveProfileNodeEnter()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<IPresenterNode, Unit>(this._mainMenuPresenterTree.PresenterTree.ObserveNodeEnter(this._mainMenuPresenterTree.ProfileNode), this.UpdatePlayerAvatarImage()));
			this._disposables.Add(disposable);
		}

		private void InitializeView()
		{
			this._disposables = new CompositeDisposable();
			this._view = this._viewProvider.Provide<ILegacyProfileView>(null);
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ReturnUnit(), (Unit _) => this.UpdatePlayerAvatarImage()), this._view.Show()), delegate(Unit _)
			{
				this.InitializeButtons();
				this._view.UiNavigationGroupHolder.AddGroup();
			});
		}

		private IObservable<Unit> UpdatePlayerAvatarImage()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.AvatarLoading.SetActive(true);
				return Observable.AsUnitObservable<string>(Observable.Do<string>(Observable.Do<string>(this._getPlayerAvatarIconName.Get(this._localPlayer.Player.PlayerId), new Action<string>(this.SetPortraitAndAvatarImageOnView)), delegate(string _)
				{
					ActivatableExtensions.Deactivate(this._view.AvatarLoading);
				}));
			});
		}

		private void SetPortraitAndAvatarImageOnView(string spriteName)
		{
			this._view.SetPortraitAndAvatarImage(spriteName, this._getLocalUserIconName.GetMediumSquare());
		}

		private void UpdatePlayerTag()
		{
			this._view.PlayerTagLabel.Text = this._getFormattedPlayerTag.Get(this._localPlayer.Player.PlayerTag);
		}

		private void InitializeButtons()
		{
			IDisposable disposable = ButtonExtensions.InitializeNavigationAndBiToNode(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree, this._mainMenuPresenterTree.MainMenuNode, this._buttonBILogger, ButtonName.ProfileBack);
			this._disposables.Add(disposable);
			IDisposable disposable2 = ButtonExtensions.InitializeNavigationAndBiToNode(this._view.AvatarChangeButton, this._mainMenuPresenterTree.PresenterTree, this._mainMenuPresenterTree.ProfileInventoryAvatarsNode, this._buttonBILogger, ButtonName.ProfileInventory);
			this._disposables.Add(disposable2);
			this.InitializeCopyPlayerTagButton();
		}

		private void InitializeCopyPlayerTagButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Repeat<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.CopyPlayerTagButton.OnClick()), delegate(Unit _)
			{
				this._copyPlayerTag.CopyToClipboard(this._localPlayer.Player);
			})));
			this._disposables.Add(disposable);
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(this._view.Hide(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._disposables.Dispose();
			}), delegate(Unit _)
			{
				this._view.MainActivatable.SetActive(false);
			});
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		[Inject]
		private IGetPlayerAvatarIconName _getPlayerAvatarIconName;

		[Inject]
		private IGetLocalUserPortraitBorderIconName _getLocalUserIconName;

		[Inject]
		private ILocalPlayerStorage _localPlayer;

		[Inject]
		private readonly ICopyPlayerTag _copyPlayerTag;

		[Inject]
		private readonly IGetFormattedPlayerTag _getFormattedPlayerTag;

		private CompositeDisposable _disposables;

		private ILegacyProfileView _view;
	}
}
