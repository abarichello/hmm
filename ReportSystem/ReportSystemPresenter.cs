using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.ReportSystem;
using HeavyMetalMachines.ReportSystem.DataTransferObjects;
using HeavyMetalMachines.Social.Avatar.Business;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UniRx;

namespace HeavymetalMachines.ReportSystem
{
	public class ReportSystemPresenter : IReportSystemPresenter
	{
		public ReportSystemPresenter(IViewLoader viewLoader, IViewProvider viewProvider, ILocalizeKey localizeKey, IGetFormattedPlayerTag getFormattedPlayerTag, IGetPlayerAvatarIconName getPlayerAvatarIconName, IReportCreator reportCreator, IGameTime gameTime, IGetCurrentMatch getCurrentMatch, IGetDisplayableNickName getDisplayableNickName, IGetPublisherPresentingData getPublisherPresentingData)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._localizeKey = localizeKey;
			this._getFormattedPlayerTag = getFormattedPlayerTag;
			this._getPlayerAvatarIconName = getPlayerAvatarIconName;
			this._reportCreator = reportCreator;
			this._gameTime = gameTime;
			this._getCurrentMatch = getCurrentMatch;
			this._getDisplayableNickName = getDisplayableNickName;
			this._getPublisherPresentingData = getPublisherPresentingData;
			this._toggleViews = new List<IReportSystemToggleView>();
			this._reportedPlayerIds = new HashSet<ReportSystemPresenter.ReportedPlayer>();
			this._hideSubject = new Subject<Unit>();
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_ReportSystem"), delegate(Unit _)
			{
				this._compositeDisposable = new CompositeDisposable();
			}), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		public IObservable<Unit> Dispose()
		{
			this._toggleViews.Clear();
			this._compositeDisposable.Dispose();
			this._compositeDisposable = null;
			if (this._hideDisposable != null)
			{
				this._hideDisposable.Dispose();
				this._hideDisposable = null;
			}
			return this._viewLoader.UnloadView("UI_ADD_ReportSystem");
		}

		public void Show(PlayerData playerData)
		{
			if (this._hideDisposable != null)
			{
				this._hideDisposable.Dispose();
				this._hideDisposable = null;
			}
			this._playerId = playerData.PlayerId;
			this._matchId = new Guid(GetCurrentMatchExtensions.Get(this._getCurrentMatch).MatchId);
			this._reportMotives = 0;
			this._view.MainCanvas.Enable();
			this._view.MainCanvasGroup.Interactable = true;
			this._view.WindowAnimator.SetBoolean("active", true);
			this._view.UiNavigationGroupHolder.AddGroup();
			this._view.ReportInputField.Text = string.Empty;
			this.SetupPlayer(playerData, this._view.ReportSystemPlayerView);
			this.ClearToggleViews();
			this.ClearFeedbacksAnimator();
			this.SetupButtonsOnShow();
			this._view.TogglesAndInputFieldCanvasGroup.Interactable = true;
			this._visible = true;
		}

		private void ClearFeedbacksAnimator()
		{
			this._view.FeedbacksAnimator.SetBoolean("confirmation", false);
			this._view.FeedbacksAnimator.SetBoolean("fail", false);
		}

		private void SetupButtonsOnShow()
		{
			ActivatableExtensions.Deactivate(this._view.OkButton);
			ActivatableExtensions.Activate(this._view.ReportButton);
			this._view.ReportButton.IsInteractable = false;
			ActivatableExtensions.Activate(this._view.CancelButton);
			this._view.CancelButton.IsInteractable = true;
		}

		private void SetupPlayer(PlayerData playerData, IReportSystemPlayerView playerView)
		{
			playerView.PlayerNameLabel.Text = this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(playerData.PlayerId, playerData.Name, new long?(playerData.PlayerTag));
			playerView.PlayerTagLabel.Text = this._getFormattedPlayerTag.Get(new long?(playerData.PlayerTag));
			if (!playerData.IsBot)
			{
				Publisher publisherById = Publishers.GetPublisherById(playerData.PublisherId);
				PublisherPresentingData publisherPresentingData = this._getPublisherPresentingData.Get(publisherById);
				if (publisherPresentingData.ShouldShowPublisherUserName)
				{
					playerView.PsnIdLabel.Text = playerData.PublisherUserName;
				}
			}
			playerView.PsnIdIconActivatable.SetActive(false);
			ObservableExtensions.Subscribe<string>(Observable.Do<string>(this._getPlayerAvatarIconName.Get(playerData.PlayerId), delegate(string imageName)
			{
				playerView.PlayerAvatarImage.SetImageName(imageName);
			}));
			string imageName2;
			PortraitDecoratorGui.TryToGetAssetName(playerData.Customizations, PortraitDecoratorGui.PortraitSpriteType.Corner, out imageName2);
			playerView.PlayerPortraitImage.SetImageName(imageName2);
		}

		public void Hide()
		{
			if (!this._visible)
			{
				return;
			}
			this._view.WindowAnimator.SetBoolean("active", false);
			this._view.UiNavigationGroupHolder.RemoveGroup();
			this._hideDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Delay<Unit>(Observable.ReturnUnit(), TimeSpan.FromSeconds(this._view.GetOutAnimationLength()), Scheduler.MainThreadIgnoreTimeScale), delegate(Unit _)
			{
				this.ClearFeedbacksAnimator();
				this._view.MainCanvas.Disable();
				this._view.MainCanvasGroup.Interactable = false;
				this._hideSubject.OnNext(Unit.Default);
			}));
			this._visible = false;
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		public bool Visible
		{
			get
			{
				return this._visible;
			}
		}

		public bool IsPlayerReported(long playerId)
		{
			ReportSystemPresenter.ReportedPlayer item = new ReportSystemPresenter.ReportedPlayer
			{
				MatchId = new Guid(GetCurrentMatchExtensions.Get(this._getCurrentMatch).MatchId),
				PlayerId = playerId
			};
			return this._reportedPlayerIds.Contains(item);
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<IReportSystemPresenterView>(null);
			this._view.MainCanvas.Disable();
			this._view.MainCanvasGroup.Interactable = false;
			this.CreateToggles();
			this.InitializeReportInputField();
			this.InitializeButtons();
			this.UpdateButtonState();
			this._view.UiNavigationAxisSelector.Rebuild();
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				this.Hide();
			})));
		}

		private void CreateToggles()
		{
			foreach (KeyValuePair<ReportMotive, string> keyValuePair in ReportSystemDrafts.MotiveDrafts)
			{
				ReportMotive reportMotive = keyValuePair.Key;
				string value = keyValuePair.Value;
				IReportSystemToggleView reportSystemToggleView = this._view.CreateToggleView();
				reportSystemToggleView.ReportToggle.IsOn = false;
				reportSystemToggleView.ReportToggleLabel.Text = this._localizeKey.Get(value, TranslationContext.ReportSystem);
				this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(reportSystemToggleView.ReportToggle.OnToggleOn(), delegate(Unit _)
				{
					this._reportMotives |= reportMotive;
				}), delegate(Unit _)
				{
					this.UpdateButtonState();
				})));
				this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(reportSystemToggleView.ReportToggle.OnToggleOff(), delegate(Unit _)
				{
					this._reportMotives &= ~reportMotive;
				}), delegate(Unit _)
				{
					this.UpdateButtonState();
				})));
				this._toggleViews.Add(reportSystemToggleView);
			}
		}

		private void InitializeReportInputField()
		{
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<string>(Observable.Do<string>(this._view.ReportInputField.OnValueChanged(), delegate(string _)
			{
				this.UpdateButtonState();
			})));
		}

		private void InitializeButtons()
		{
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<bool>(Observable.Repeat<bool>(Observable.Do<bool>(Observable.ContinueWith<Unit, bool>(Observable.Do<Unit>(Observable.First<Unit>(this._view.ReportButton.OnClick()), delegate(Unit _)
			{
				this.SetupButtonsOnSendReport();
			}), this.SendReport()), new Action<bool>(this.OnReportEnd)))));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.CancelButton.OnClick(), delegate(Unit _)
			{
				this.Hide();
			})));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.OkButton.OnClick(), delegate(Unit _)
			{
				this.Hide();
			})));
		}

		private void SetupButtonsOnSendReport()
		{
			this._view.ReportButton.IsInteractable = false;
			this._view.CancelButton.IsInteractable = false;
			ActivatableExtensions.Activate(this._view.LoadingActivatable);
		}

		private void OnReportEnd(bool isCompleted)
		{
			this._view.TogglesAndInputFieldCanvasGroup.Interactable = false;
			ActivatableExtensions.Deactivate(this._view.ReportButton);
			ActivatableExtensions.Deactivate(this._view.CancelButton);
			ActivatableExtensions.Deactivate(this._view.LoadingActivatable);
			ActivatableExtensions.Activate(this._view.OkButton);
			if (isCompleted)
			{
				this._reportedPlayerIds.Add(new ReportSystemPresenter.ReportedPlayer
				{
					PlayerId = this._playerId,
					MatchId = this._matchId
				});
				this._view.FeedbacksAnimator.SetBoolean("confirmation", true);
			}
			else
			{
				this._view.FeedbacksAnimator.SetBoolean("fail", true);
			}
		}

		private IObservable<bool> SendReport()
		{
			return Observable.Defer<bool>(delegate()
			{
				ReportWindowBag reportWindowBag = new ReportWindowBag
				{
					MatchTime = this._gameTime.MatchTimer.GetTimeSeconds(),
					ReportText = this._view.ReportInputField.Text
				};
				reportWindowBag.SetMotives(this._reportMotives);
				ReportRequestBag reportRequestBag = new ReportRequestBag
				{
					MatchId = this._matchId,
					TargetPlayerId = this._playerId,
					WindowBag = reportWindowBag
				};
				return this._reportCreator.CreateReport(reportRequestBag);
			});
		}

		private void ClearToggleViews()
		{
			foreach (IReportSystemToggleView reportSystemToggleView in this._toggleViews)
			{
				reportSystemToggleView.ReportToggle.IsOn = false;
			}
		}

		private void UpdateButtonState()
		{
			this._view.ReportButton.IsInteractable = (this._reportMotives != 0);
		}

		private const string SceneName = "UI_ADD_ReportSystem";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly ILocalizeKey _localizeKey;

		private readonly IGetFormattedPlayerTag _getFormattedPlayerTag;

		private readonly IGetPlayerAvatarIconName _getPlayerAvatarIconName;

		private readonly IReportCreator _reportCreator;

		private readonly IGameTime _gameTime;

		private readonly IGetCurrentMatch _getCurrentMatch;

		private readonly IGetDisplayableNickName _getDisplayableNickName;

		private readonly IGetPublisherPresentingData _getPublisherPresentingData;

		private readonly Subject<Unit> _hideSubject;

		private IReportSystemPresenterView _view;

		private CompositeDisposable _compositeDisposable;

		private IDisposable _hideDisposable;

		private bool _visible;

		private readonly List<IReportSystemToggleView> _toggleViews;

		private readonly HashSet<ReportSystemPresenter.ReportedPlayer> _reportedPlayerIds;

		private ReportMotive _reportMotives;

		private long _playerId;

		private Guid _matchId;

		private struct ReportedPlayer
		{
			public bool Equals(ReportSystemPresenter.ReportedPlayer other)
			{
				return this.PlayerId == other.PlayerId && this.MatchId.Equals(other.MatchId);
			}

			public override bool Equals(object obj)
			{
				return !object.ReferenceEquals(null, obj) && obj is ReportSystemPresenter.ReportedPlayer && this.Equals((ReportSystemPresenter.ReportedPlayer)obj);
			}

			public override int GetHashCode()
			{
				return this.PlayerId.GetHashCode() * 397 ^ this.MatchId.GetHashCode();
			}

			public long PlayerId;

			public Guid MatchId;
		}
	}
}
