using System;
using System.Collections.Generic;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavymetalMachines.ReportSystem;
using HeavyMetalMachines.Social.Friends.Business.BlockedPlayers;
using HeavyMetalMachines.VoiceChat.Business;
using Hoplon.Input;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.MuteSystem
{
	public class MuteSystemPresenter : IMuteSystemPresenter
	{
		public MuteSystemPresenter(IViewLoader viewLoader, IViewProvider viewProvider, IGetCharacterData getCharacterData, IMatchPlayers matchPlayers, IGetCurrentMatch getCurrentMatch, IGetFormattedPlayerTag getFormattedPlayerTag, ILocalizeKey localizeKey, IMuteVoiceChatPlayer muteVoiceChatPlayer, IIsVoiceChatPlayerMuted isVoiceChatPlayerMuted, IIsPlayerRestrictedByTextChat isPlayerRestrictedByTextChat, DiContainer diContainer, ILogger<IMuteSystemPresenter> logger, IInputGetActiveDevicePoller inputGetActiveDevicePoller, IReportSystemPresenter reportSystemPresenter, IGetPublisherPresentingData getPublisherPresentingData, IGetDisplayableNickName getDisplayableNickName)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._getCharacterData = getCharacterData;
			this._matchPlayers = matchPlayers;
			this._getCurrentMatch = getCurrentMatch;
			this._getFormattedPlayerTag = getFormattedPlayerTag;
			this._localizeKey = localizeKey;
			this._muteVoiceChatPlayer = muteVoiceChatPlayer;
			this._isVoiceChatPlayerMuted = isVoiceChatPlayerMuted;
			this._isPlayerRestrictedByTextChat = isPlayerRestrictedByTextChat;
			this._diContainer = diContainer;
			this._logger = logger;
			this._inputGetActiveDevicePoller = inputGetActiveDevicePoller;
			this._reportSystemPresenter = reportSystemPresenter;
			this._getPublisherPresentingData = getPublisherPresentingData;
			this._getDisplayableNickName = getDisplayableNickName;
			this._cachedTextChatRestrictions = new Dictionary<long, bool>();
			this._currentSessionTextChatRestrictions = new Dictionary<long, bool>();
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(this._viewLoader.LoadView("UI_ADD_MuteSystem"), this.InitializeLocalPlayerRestrictions()), delegate(Unit _)
			{
				this.CacheTextChatRestrictions();
			}), delegate(Unit _)
			{
				this._compositeDisposable = new CompositeDisposable();
			}), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		private void CacheTextChatRestrictions()
		{
			this.CacheTextChatRestrictions(this._matchPlayers.Players);
			this.CacheTextChatRestrictions(this._matchPlayers.Narrators);
		}

		private void CacheTextChatRestrictions(List<PlayerData> players)
		{
			foreach (PlayerData playerData in players)
			{
				if (!playerData.IsCurrentPlayer)
				{
					this._cachedTextChatRestrictions[playerData.PlayerId] = this._isPlayerRestrictedByTextChat.IsPlayerRestricted(playerData.PlayerId);
				}
			}
		}

		public IObservable<Unit> Dispose()
		{
			this._compositeDisposable.Dispose();
			this._compositeDisposable = null;
			return this._viewLoader.UnloadView("UI_ADD_MuteSystem");
		}

		public void Show()
		{
			if (this._visible)
			{
				return;
			}
			if (this._hideDisposable != null)
			{
				this._hideDisposable.Dispose();
				this._hideDisposable = null;
			}
			this._view.MainCanvas.Enable();
			this._view.ContainerAnimator.SetBoolean("active", true);
			this._view.MainCanvasGroup.Interactable = true;
			this._view.UiNavigationGroupHolder.AddGroup();
			this._visible = true;
			this._currentSessionTextChatRestrictions.Clear();
		}

		public void Hide()
		{
			if (!this._visible)
			{
				return;
			}
			this._view.ContainerAnimator.SetBoolean("active", false);
			this._view.MainCanvasGroup.Interactable = false;
			this._view.UiNavigationGroupHolder.RemoveGroup();
			this._view.UiNavigationAxisSelector.ClearSelection();
			this._hideDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Delay<Unit>(Observable.ReturnUnit(), TimeSpan.FromSeconds((double)this._view.GetOutAnimationLength()), Scheduler.MainThreadIgnoreTimeScale), delegate(Unit _)
			{
				this._view.MainCanvas.Disable();
			}));
			this._visible = false;
			this.TryToSendModifiedTextChatRestrictions();
		}

		private void TryToSendModifiedTextChatRestrictions()
		{
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			foreach (KeyValuePair<long, bool> keyValuePair in this._currentSessionTextChatRestrictions)
			{
				long key = keyValuePair.Key;
				bool value = keyValuePair.Value;
				if (this._cachedTextChatRestrictions[key] != value)
				{
					if (value)
					{
						list.Add(key);
					}
					else
					{
						list2.Add(key);
					}
				}
			}
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ReturnUnit(), this.SendPlayerTextChatRestrictions(list, delegate(long playerId)
			{
				this._logger.InfoFormat("Add Text Chat Restriction for player {0}", new object[]
				{
					playerId
				});
				return this._setTextChatPlayerRestriction.AddRestriction(playerId);
			})), this.SendPlayerTextChatRestrictions(list2, delegate(long playerId)
			{
				this._logger.InfoFormat("Remove Text Chat Restriction for player {0}", new object[]
				{
					playerId
				});
				return this._setTextChatPlayerRestriction.RemoveRestriction(playerId);
			})), delegate(Unit _)
			{
				this.CacheTextChatRestrictions();
			}));
		}

		private IObservable<Unit> SendPlayerTextChatRestrictions(List<long> playersToUnRestrict, Func<long, IObservable<Unit>> action)
		{
			if (playersToUnRestrict.Count == 0)
			{
				return Observable.ReturnUnit();
			}
			return Observable.Concat<Unit>(Observable.Select<long, IObservable<Unit>>(Observable.ToObservable<long>(playersToUnRestrict), action));
		}

		public bool Visible
		{
			get
			{
				return this._visible;
			}
		}

		private IObservable<Unit> InitializeLocalPlayerRestrictions()
		{
			this._isPlayerBlocked = this._diContainer.Resolve<IIsPlayerBlocked>();
			this._blockPlayer = this._diContainer.Resolve<IBlockPlayer>();
			this._unblockPlayer = this._diContainer.Resolve<IUnblockPlayer>();
			this._localPlayerRestrictionsInitializer = this._diContainer.Resolve<ILocalPlayerRestrictionsInitializer>();
			this._setTextChatPlayerRestriction = this._diContainer.Resolve<ISetTextChatPlayerRestriction>();
			if (this._matchPlayers.Players.Count > 1)
			{
				List<long> list = new List<long>();
				foreach (PlayerData playerData in this._matchPlayers.Players)
				{
					if (!playerData.IsCurrentPlayer)
					{
						list.Add(playerData.PlayerId);
					}
				}
				return this._localPlayerRestrictionsInitializer.Initialize(list.ToArray());
			}
			return Observable.ReturnUnit();
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<IMuteSystemPresenterView>(null);
			this._view.MainCanvas.Disable();
			this._view.MainCanvasGroup.Interactable = false;
			this.HideSubtitle();
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.ExitButton.OnClick(), delegate(Unit _)
			{
				this.Hide();
			});
			this._compositeDisposable.Add(disposable);
			this.InitializePlayerSlots();
			this._view.UiNavigationAxisSelector.Rebuild();
		}

		private void InitializePlayerSlots()
		{
			List<PlayerData> sortedPlayerDataList = this.GetSortedPlayerDataList();
			List<PlayerData> allyPlayerDataList = this.GetAllyPlayerDataList(sortedPlayerDataList);
			List<PlayerData> enemyPlayerDataList = this.GetEnemyPlayerDataList(sortedPlayerDataList);
			List<PlayerData> narratorPlayerDataList = this.GetNarratorPlayerDataList();
			int num = 0;
			foreach (PlayerData playerData in allyPlayerDataList)
			{
				this.InitializePlayerSlot(playerData);
				num++;
			}
			if (allyPlayerDataList.Count > 0 && enemyPlayerDataList.Count > 0)
			{
				this._view.AddSeparator(num);
				num++;
			}
			foreach (PlayerData playerData2 in enemyPlayerDataList)
			{
				this.InitializePlayerSlot(playerData2);
				num++;
			}
			if (narratorPlayerDataList.Count > 0)
			{
				bool flag = allyPlayerDataList.Count > 0 || enemyPlayerDataList.Count > 0;
				if (flag)
				{
					this._view.AddSeparator(num);
					num++;
				}
				this._view.ShowNarratorTitle(num);
				num++;
			}
			foreach (PlayerData playerData3 in narratorPlayerDataList)
			{
				this.InitializePlayerSlot(playerData3);
			}
		}

		private List<PlayerData> GetSortedPlayerDataList()
		{
			List<PlayerData> list = new List<PlayerData>(this._matchPlayers.Players);
			int instanceId = this._matchPlayers.CurrentPlayerData.PlayerCarId.GetInstanceId();
			HudUtils.PlayerDataComparer comparer = new HudUtils.PlayerDataComparer(instanceId, HudUtils.PlayerDataComparer.PlayerDataComparerType.InstanceId);
			list.Sort(comparer);
			return list;
		}

		private List<PlayerData> GetAllyPlayerDataList(List<PlayerData> playerDatas)
		{
			List<PlayerData> list = new List<PlayerData>(playerDatas.Count);
			foreach (PlayerData playerData in playerDatas)
			{
				if (playerData.Team == this._matchPlayers.CurrentPlayerData.Team && !playerData.IsCurrentPlayer)
				{
					list.Add(playerData);
				}
			}
			return list;
		}

		private List<PlayerData> GetEnemyPlayerDataList(List<PlayerData> playerDatas)
		{
			List<PlayerData> list = new List<PlayerData>(playerDatas.Count);
			foreach (PlayerData playerData in playerDatas)
			{
				if (playerData.Team != this._matchPlayers.CurrentPlayerData.Team)
				{
					list.Add(playerData);
				}
			}
			return list;
		}

		private List<PlayerData> GetNarratorPlayerDataList()
		{
			List<PlayerData> list = new List<PlayerData>(this._matchPlayers.Narrators.Count);
			Match? ifExisting = this._getCurrentMatch.GetIfExisting();
			if (ifExisting != null && ifExisting.Value.Mode == 5)
			{
				foreach (PlayerData playerData in this._matchPlayers.Narrators)
				{
					if (playerData.PlayerId != this._matchPlayers.CurrentPlayerData.PlayerId)
					{
						list.Add(playerData);
					}
				}
			}
			return list;
		}

		private void InitializePlayerSlot(PlayerData playerData)
		{
			IMuteSystemPlayerSlotView muteSystemPlayerSlotView = this._view.AddPlayerSlot();
			string formattedNickName = this._getDisplayableNickName.GetFormattedNickName(playerData.PlayerId, playerData.Name);
			muteSystemPlayerSlotView.PlayerNameLabel.Text = string.Format("{0} {1}", formattedNickName, this._getFormattedPlayerTag.Get(new long?(playerData.PlayerTag)));
			this.InitializePlayerSlotCharacterName(muteSystemPlayerSlotView, playerData);
			this.InitializePlayerSlotTeamStatus(muteSystemPlayerSlotView, playerData);
			this.InitializePlayerSlotCharacterIcon(muteSystemPlayerSlotView, playerData);
			this.InitializePsnInfo(playerData, muteSystemPlayerSlotView);
			this.InitializeVoiceButtons(playerData, muteSystemPlayerSlotView);
			this.InitializeOtherActionsButtons(playerData, muteSystemPlayerSlotView);
			this.InitializeBlockButtons(playerData, muteSystemPlayerSlotView);
			this.InitializeReportButton(playerData, muteSystemPlayerSlotView);
		}

		private void InitializePlayerSlotCharacterName(IMuteSystemPlayerSlotView playerSlotView, PlayerData playerData)
		{
			ILabel characterNameLabel = playerSlotView.CharacterNameLabel;
			if (playerData.IsNarrator)
			{
				characterNameLabel.IsActive = false;
				return;
			}
			characterNameLabel.IsActive = true;
			characterNameLabel.Text = playerData.GetCharacterLocalizedName();
		}

		private void InitializePlayerSlotTeamStatus(IMuteSystemPlayerSlotView playerSlotView, PlayerData playerData)
		{
			if (playerData.IsNarrator)
			{
				playerSlotView.SetCharacterNameLabelAsNarrator();
				return;
			}
			if (playerData.Team == this._matchPlayers.CurrentPlayerData.Team)
			{
				playerSlotView.SetCharacterNameLabelAsAlly();
			}
			else
			{
				playerSlotView.SetCharacterNameLabelAsEnemy();
			}
		}

		private void InitializePlayerSlotCharacterIcon(IMuteSystemPlayerSlotView playerSlotView, PlayerData playerData)
		{
			string imageName;
			if (playerData.IsNarrator)
			{
				imageName = playerSlotView.GetEmptyCharIconName();
			}
			else
			{
				imageName = this.GetCharacterIconName(playerData.CharacterItemType.Id);
			}
			IDynamicImage characterIconDynamicImage = playerSlotView.CharacterIconDynamicImage;
			Color color = characterIconDynamicImage.Color;
			color.A = 1f;
			characterIconDynamicImage.Color = color;
			characterIconDynamicImage.SetImageName(imageName);
		}

		private string GetCharacterIconName(Guid characterId)
		{
			return this._getCharacterData.Get(characterId).SmallIconName;
		}

		private void InitializePsnInfo(PlayerData playerData, IMuteSystemPlayerSlotView playerSlotView)
		{
			Publisher publisherById = Publishers.GetPublisherById(playerData.PublisherId);
			PublisherPresentingData publisherPresentingData = this._getPublisherPresentingData.Get(publisherById);
			if (publisherPresentingData.ShouldShowPublisherUserName)
			{
				playerSlotView.PsnIdIconActivatable.SetActive(true);
				playerSlotView.PsnIdLabel.Text = playerData.PublisherUserName;
			}
			else
			{
				playerSlotView.PsnIdIconActivatable.SetActive(false);
			}
		}

		private void InitializeVoiceButtons(PlayerData playerData, IMuteSystemPlayerSlotView playerSlotView)
		{
			if (this.IsPlayerBlocked(playerData) || playerData.IsNarrator)
			{
				ActivatableExtensions.Deactivate(playerSlotView.MuteVoiceButton);
				ActivatableExtensions.Deactivate(playerSlotView.UnMuteVoiceButton);
			}
			else
			{
				this.InitializeButtonPairActivation(playerSlotView.MuteVoiceButton, playerSlotView.UnMuteVoiceButton, this._isVoiceChatPlayerMuted.IsMuted(playerData.ConvertToPlayer()));
			}
			this.InitializeVoiceButtonClick(playerData, playerSlotView);
			this.InitializeButtonHoverAndSelection(MuteSystemSubtitleType.MuteVoice, playerSlotView.MuteVoiceHoverable, playerSlotView.MuteVoiceSelectable, MuteSystemSubtitleType.UnMuteVoice, playerSlotView.UnMuteVoiceHoverable, playerSlotView.UnMuteVoiceSelectable);
		}

		private void InitializeVoiceButtonClick(PlayerData playerData, IMuteSystemPlayerSlotView playerSlotView)
		{
			IPlayer player = playerData.ConvertToPlayer();
			this.CreateButtonAction(playerSlotView, playerSlotView.MuteVoiceButton, playerSlotView.UnMuteVoiceButton, this._muteVoiceChatPlayer.Mute(player));
			this.CreateButtonAction(playerSlotView, playerSlotView.UnMuteVoiceButton, playerSlotView.MuteVoiceButton, this._muteVoiceChatPlayer.UnMute(player));
		}

		private void InitializeOtherActionsButtons(PlayerData playerData, IMuteSystemPlayerSlotView playerSlotView)
		{
			if (this.IsPlayerBlocked(playerData))
			{
				ActivatableExtensions.Deactivate(playerSlotView.MuteOtherActionsButton);
				ActivatableExtensions.Deactivate(playerSlotView.UnMuteOtherActionsButton);
			}
			else
			{
				this.InitializeButtonPairActivation(playerSlotView.MuteOtherActionsButton, playerSlotView.UnMuteOtherActionsButton, this._isPlayerRestrictedByTextChat.IsPlayerRestricted(playerData.PlayerId));
			}
			this.InitializeOtherActionsButtonClick(playerData, playerSlotView);
			this.InitializeButtonHoverAndSelection(MuteSystemSubtitleType.MuteOtherActions, playerSlotView.MuteOtherActionsHoverable, playerSlotView.MuteOtherActionsSelectable, MuteSystemSubtitleType.UnMuteOtherActions, playerSlotView.UnMuteOtherActionsHoverable, playerSlotView.UnMuteOtherActionsSelectable);
		}

		private void InitializeOtherActionsButtonClick(PlayerData playerData, IMuteSystemPlayerSlotView playerSlotView)
		{
			long playerId = playerData.PlayerId;
			this.CreateButtonAction(playerSlotView, playerSlotView.MuteOtherActionsButton, playerSlotView.UnMuteOtherActionsButton, this.AddTextChatRestriction(playerId));
			this.CreateButtonAction(playerSlotView, playerSlotView.UnMuteOtherActionsButton, playerSlotView.MuteOtherActionsButton, this.RemoveTextChatRestriction(playerId));
		}

		private IObservable<Unit> AddTextChatRestriction(long playerId)
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._currentSessionTextChatRestrictions[playerId] = true;
			});
		}

		private IObservable<Unit> RemoveTextChatRestriction(long playerId)
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._currentSessionTextChatRestrictions[playerId] = false;
			});
		}

		private void InitializeBlockButtons(PlayerData playerData, IMuteSystemPlayerSlotView playerSlotView)
		{
			bool flag = this.IsPlayerBlocked(playerData);
			this.InitializeButtonPairActivation(playerSlotView.BlockButton, playerSlotView.UnBlockButton, flag);
			if (flag)
			{
				ActivatableExtensions.Deactivate(playerSlotView.BlockButton);
				ActivatableExtensions.Activate(playerSlotView.UnBlockButton);
				playerSlotView.UnBlockButton.IsInteractable = false;
				playerSlotView.UnBlockHoverable.Interactable = false;
				ActivatableExtensions.Activate(playerSlotView.BlockInfoActivatable);
			}
			else
			{
				ActivatableExtensions.Deactivate(playerSlotView.BlockButton);
				ActivatableExtensions.Deactivate(playerSlotView.UnBlockButton);
				ActivatableExtensions.Deactivate(playerSlotView.BlockInfoActivatable);
			}
			this.InitializeBlockButtonClick(playerData, playerSlotView);
			this.InitializeButtonHoverAndSelection(MuteSystemSubtitleType.Block, playerSlotView.BlockHoverable, playerSlotView.BlockSelectable, MuteSystemSubtitleType.UnBlock, playerSlotView.UnBlockHoverable, playerSlotView.UnBlockSelectable);
		}

		private void InitializeBlockButtonClick(PlayerData playerData, IMuteSystemPlayerSlotView playerSlotView)
		{
			long playerId = playerData.PlayerId;
			this.CreateButtonAction(playerSlotView, playerSlotView.BlockButton, playerSlotView.UnBlockButton, this._blockPlayer.Block(playerId));
			this.CreateButtonAction(playerSlotView, playerSlotView.UnBlockButton, playerSlotView.BlockButton, this._unblockPlayer.Unblock(playerId));
		}

		private void InitializeButtonPairActivation(IButton button, IButton oppositeButton, bool isMutedOrBlocked)
		{
			if (!isMutedOrBlocked)
			{
				ActivatableExtensions.Activate(button);
				ActivatableExtensions.Deactivate(oppositeButton);
			}
			else
			{
				ActivatableExtensions.Deactivate(button);
				ActivatableExtensions.Activate(oppositeButton);
			}
		}

		private void CreateButtonAction(IMuteSystemPlayerSlotView playerSlotView, IButton button, IButton opposeButton, IObservable<Unit> action)
		{
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Repeat<Unit>(Observable.DoOnError<Unit>(Observable.DoOnCompleted<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.First<Unit>(button.OnClick()), delegate(Unit _)
			{
				ActivatableExtensions.Deactivate(button);
				ActivatableExtensions.Activate(opposeButton);
				opposeButton.IsInteractable = false;
				this.TrySelectButton(opposeButton, playerSlotView);
			}), action), delegate()
			{
				opposeButton.IsInteractable = true;
			}), delegate(Exception e)
			{
				ActivatableExtensions.Activate(button);
				button.IsInteractable = true;
				ActivatableExtensions.Deactivate(opposeButton);
				opposeButton.IsInteractable = true;
				this.TrySelectButton(button, playerSlotView);
				this._logger.Error(e.Message);
			}))));
		}

		private void TrySelectButton(IButton button, IMuteSystemPlayerSlotView playerSlotView)
		{
			if (this._inputGetActiveDevicePoller.GetActiveDevice() == 3)
			{
				this._view.TryToSelect(button, playerSlotView);
			}
		}

		private void InitializeButtonHoverAndSelection(MuteSystemSubtitleType activeSubtitleType, IHoverable activeHoverable, ISelectable activeSelectable, MuteSystemSubtitleType inactiveSubtitleType, IHoverable inactiveHoverable, ISelectable inactiveSelectable)
		{
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				activeHoverable.OnHoverIn(),
				activeSelectable.OnSelect()
			}), delegate(Unit _)
			{
				this.ShowSubtitle(activeSubtitleType);
			})));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				inactiveHoverable.OnHoverIn(),
				inactiveSelectable.OnSelect()
			}), delegate(Unit _)
			{
				this.ShowSubtitle(inactiveSubtitleType);
			})));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				activeHoverable.OnHoverOut(),
				activeSelectable.OnDeselect(),
				inactiveHoverable.OnHoverOut(),
				inactiveSelectable.OnDeselect()
			}), delegate(Unit _)
			{
				this.HideSubtitle();
			})));
		}

		private void InitializeReportButton(PlayerData playerData, IMuteSystemPlayerSlotView playerSlotView)
		{
			ActivatableExtensions.Activate(playerSlotView.ReportButton);
			playerSlotView.ReportButton.IsInteractable = !this._reportSystemPresenter.IsPlayerReported(playerData.PlayerId);
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				playerSlotView.ReportHoverable.OnHoverIn(),
				playerSlotView.ReportSelectable.OnSelect()
			}), delegate(Unit _)
			{
				this.ShowSubtitle(MuteSystemSubtitleType.Report);
			})));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				playerSlotView.ReportHoverable.OnHoverOut(),
				playerSlotView.ReportSelectable.OnDeselect()
			}), delegate(Unit _)
			{
				this.HideSubtitle();
			})));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Repeat<Unit>(Observable.DoOnError<Unit>(Observable.DoOnCompleted<Unit>(Observable.First<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.First<Unit>(playerSlotView.ReportButton.OnClick()), delegate(Unit _)
			{
				playerSlotView.ReportButton.IsInteractable = false;
				this._reportSystemPresenter.Show(playerData);
			}), this._reportSystemPresenter.ObserveHide())), delegate()
			{
				playerSlotView.ReportButton.IsInteractable = !this._reportSystemPresenter.IsPlayerReported(playerData.PlayerId);
			}), delegate(Exception e)
			{
				this._logger.Error(e.Message);
			}))));
		}

		private void ShowSubtitle(MuteSystemSubtitleType subtitleType)
		{
			ActivatableExtensions.Activate(this._view.MuteSystemSubtitleView.MainActivatable);
			string text;
			string text2;
			this._view.MuteSystemSubtitleView.GetSubtitleDrafts(subtitleType, out text, out text2);
			this._view.MuteSystemSubtitleView.IconSpriteImage.Sprite = this._view.MuteSystemSubtitleView.GetIconSprite(subtitleType);
			if (string.IsNullOrEmpty(text2))
			{
				this._view.MuteSystemSubtitleView.SubTitleLabel.IsActive = false;
			}
			else
			{
				this._view.MuteSystemSubtitleView.SubTitleLabel.IsActive = true;
				this._view.MuteSystemSubtitleView.SubTitleLabel.Text = this._localizeKey.Get(text2, TranslationContext.MuteWindow);
			}
			this._view.MuteSystemSubtitleView.TitleLabel.Text = this._localizeKey.Get(text, TranslationContext.MuteWindow);
		}

		private void HideSubtitle()
		{
			ActivatableExtensions.Deactivate(this._view.MuteSystemSubtitleView.MainActivatable);
		}

		private bool IsPlayerBlocked(PlayerData playerData)
		{
			return this._isPlayerBlocked.IsBlocked(playerData.PlayerId);
		}

		private const string SceneName = "UI_ADD_MuteSystem";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly IGetCharacterData _getCharacterData;

		private readonly IMatchPlayers _matchPlayers;

		private readonly IGetCurrentMatch _getCurrentMatch;

		private readonly IGetFormattedPlayerTag _getFormattedPlayerTag;

		private readonly ILocalizeKey _localizeKey;

		private readonly IMuteVoiceChatPlayer _muteVoiceChatPlayer;

		private readonly IIsVoiceChatPlayerMuted _isVoiceChatPlayerMuted;

		private readonly IIsPlayerRestrictedByTextChat _isPlayerRestrictedByTextChat;

		private readonly DiContainer _diContainer;

		private readonly ILogger<IMuteSystemPresenter> _logger;

		private readonly IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		private readonly IReportSystemPresenter _reportSystemPresenter;

		private readonly IGetPublisherPresentingData _getPublisherPresentingData;

		private readonly IGetDisplayableNickName _getDisplayableNickName;

		private IIsPlayerBlocked _isPlayerBlocked;

		private IBlockPlayer _blockPlayer;

		private IUnblockPlayer _unblockPlayer;

		private ILocalPlayerRestrictionsInitializer _localPlayerRestrictionsInitializer;

		private ISetTextChatPlayerRestriction _setTextChatPlayerRestriction;

		private CompositeDisposable _compositeDisposable;

		private IMuteSystemPresenterView _view;

		private IDisposable _hideDisposable;

		private bool _visible;

		private readonly Dictionary<long, bool> _cachedTextChatRestrictions;

		private readonly Dictionary<long, bool> _currentSessionTextChatRestrictions;
	}
}
