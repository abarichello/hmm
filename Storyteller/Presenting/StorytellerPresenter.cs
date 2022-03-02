using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientAPI.Objects;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.DataTransferObjects.Server;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.Regions.Business;
using HeavyMetalMachines.Regions.Infra;
using HeavyMetalMachines.Social;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Social.Groups.Models;
using HeavyMetalMachines.Tournaments.API;
using Hoplon.Input;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public class StorytellerPresenter : IStorytellerPresenter, IPresenter
	{
		public StorytellerPresenter(IMainMenuPresenterTree mainMenuPresenterTree, IViewLoader viewLoader, IViewProvider viewProvider, IStorytellerGameserverSearch gameServerSearch, IStorytellerQueueProvider queues, IRegionService regions, IStorytellerTranslationProvider translation, IStorytellerMatchConnection matchConnection, IGroupStorage groupState, IGetThenObserveMatchmakingQueueState queueState, IClientButtonBILogger buttonBiLogger, ILocalizeKey localizeKey, IAntiSnipingChecks antiSnipingChecks, ILogger<StorytellerPresenter> logger, IInputGetActiveDevicePoller activeDevicePoller, IObserveCrossplayChange observePsnCrossplayChange, IGetPublisherPresentingData getPublisherPresentingData, IBadNameCensor badNameCensor, IGetDisplayableNickName getDisplayableNickName)
		{
			this._buttonBiLogger = buttonBiLogger;
			this._localizeKey = localizeKey;
			this._antiSnipingChecks = antiSnipingChecks;
			this._logger = logger;
			this._activeDevicePoller = activeDevicePoller;
			this._observePsnCrossplayChange = observePsnCrossplayChange;
			this._getPublisherPresentingData = getPublisherPresentingData;
			this._badNameCensor = badNameCensor;
			this._getDisplayableNickName = getDisplayableNickName;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._gameServerSearch = gameServerSearch;
			this._queues = queues;
			this._regions = regions;
			this._translation = translation;
			this._matchConnection = matchConnection;
			this._groupState = groupState;
			this._queueState = queueState;
			this._hideSubject = new Subject<Unit>();
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_Storyteller"), delegate(Unit _)
			{
				this.InitializeView();
			}), delegate(Unit _)
			{
				this.InitializeUiNavigation();
			});
		}

		private void InitializeView()
		{
			this._disposables = new CompositeDisposable();
			this._view = this._viewProvider.Provide<IStorytellerPresenterView>(null);
			this.InitializeDropdowns();
			this.InitializeBackButton();
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<bool, Unit>(Observable.Do<bool>(this._observePsnCrossplayChange.Observe(), new Action<bool>(this.ObserveCrossplayChange)), (bool _) => this.SearchMatches()));
			this._disposables.Add(disposable);
		}

		private void ObserveCrossplayChange(bool psnCrossplayEnable)
		{
			this._regionRefresh.Dispose();
			this.InitializeDropdowns();
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(this._view.PlayIntroAnim(), delegate(Unit _)
			{
				this.ShowView();
				this._view.UiNavigationGroupHolder.AddGroup();
				this.SetUiNavigationFocusOnTopGroup();
				if (this._activeDevicePoller.GetActiveDevice() == 3)
				{
					this._view.UiNavigationPrioritySelection();
				}
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ReturnUnit(), (Unit _) => this._view.PlayOutroAnim()), this._viewLoader.UnloadView("UI_ADD_Storyteller")), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
				this._hideSubject.OnCompleted();
				this._view.UiNavigationGroupHolder.RemoveGroup();
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.DisposeAllExceptBackButton();
				this._disposables.Dispose();
			});
		}

		private void InitializeUiNavigation()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<AxisSelectorEdge>(Observable.Do<AxisSelectorEdge>(this._view.UiNavigationTopGroupAxisSelector.ObserveOnEdgeReached(), new Action<AxisSelectorEdge>(this.OnTopGroupNavigationEdgeDown)));
			this._disposables.Add(disposable);
			disposable = ObservableExtensions.Subscribe<AxisSelectorEdge>(Observable.Do<AxisSelectorEdge>(this._view.UiNavigationMatchListAxisSelector.ObserveOnEdgeReached(), new Action<AxisSelectorEdge>(this.OnMatchListGroupNavigationEdgeUp)));
			this._disposables.Add(disposable);
		}

		private void SetUiNavigationFocusOnTopGroup()
		{
			this._view.UiNavigationMatchListSubGroupHolder.SubGroupFocusRelease();
			this._view.UiNavigationTopSubGroupHolder.SubGroupFocusGet();
		}

		private void TrySetUINavigationFocusOnMatchList()
		{
			if (!this._isFoundMatch)
			{
				return;
			}
			this._view.UiNavigationTopSubGroupHolder.SubGroupFocusRelease();
			this._view.UiNavigationMatchListSubGroupHolder.SubGroupFocusGet();
		}

		private void OnTopGroupNavigationEdgeDown(AxisSelectorEdge edge)
		{
			if (edge == 1)
			{
				this.TrySetUINavigationFocusOnMatchList();
			}
		}

		private void OnMatchListGroupNavigationEdgeUp(AxisSelectorEdge edge)
		{
			if (edge == null)
			{
				this.SetUiNavigationFocusOnTopGroup();
			}
		}

		private void DisposeAllExceptBackButton()
		{
			if (this._regionRefresh != null)
			{
				this._regionRefresh.Dispose();
				this._regionRefresh = null;
			}
			if (this._searchDisposable != null)
			{
				this._searchDisposable.Dispose();
				this._searchDisposable = null;
			}
			if (this._groupDisposable != null)
			{
				this._groupDisposable.Dispose();
				this._groupDisposable = null;
			}
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private void InitializeDropdowns()
		{
			this._regionRefresh = ObservableExtensions.Subscribe<Region[]>(Observable.Do<Region[]>(Observable.StartWith<Region[]>(this._regions.OnRegionsRefresh(), this._regions.GetAllRegions()), new Action<Region[]>(this.InitializeRegionsDropdown)));
			this._view.QueuesDropdown.ClearOptions();
			this._searchableQueues = this._queues.GetQueues();
			this._view.QueuesDropdown.AddOptions((from queue in this._searchableQueues
			select queue.QueueName).ToList<string>(), (from queue in this._searchableQueues
			select queue.LocalizedName).ToList<string>());
		}

		private void InitializeRegionsDropdown(Region[] regions)
		{
			this._view.RegionsDropdown.ClearOptions();
			IEnumerable<string> source = from region in regions
			select region.Name;
			IEnumerable<string> source2 = from region in regions
			select this._translation.GetTranslatedRegion(region.Name);
			this._view.RegionsDropdown.AddOptions(source.ToList<string>(), source2.ToList<string>());
			this._view.RegionsDropdown.SelectedOption = this._regions.GetBestRegion();
		}

		private void ShowView()
		{
			this._view.Title.Setup(this._translation.GetTranslatedTitle(), HmmUiText.TextStyles.UpperCase, string.Empty, HmmUiText.TextStyles.Default, string.Empty, HmmUiText.TextStyles.Default, false);
			this._view.MainCanvas.Enable();
			this._view.SearchField.SetupSubmitWithEnterOnlyMode();
			ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(Observable.SelectMany<GameServerRunningInfo, bool>(Observable.Do<GameServerRunningInfo>(this._view.GameServerConnection.OnConnectToMatch(), delegate(GameServerRunningInfo _)
			{
				this.ShowConnectingToServer();
			}), (GameServerRunningInfo server) => this._matchConnection.ConnectToServer(server)), new Action<bool>(this.HideConnectingToServer)));
			this._groupDisposable = ObservableExtensions.Subscribe<Group>(Observable.Do<Group>(this._groupState.OnGroupChanged, new Action<Group>(this.OnGroupStateChanged)));
			this._disposables.Add(ObservableExtensions.Subscribe<MatchmakingQueueState>(Observable.Do<MatchmakingQueueState>(this._queueState.GetThenObserve(), new Action<MatchmakingQueueState>(this.OnQueueStateChanged))));
			this.StartMatchSearches();
		}

		private void StartMatchSearches()
		{
			this._searchDisposable = ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this.SearchMatches(), Observable.Repeat<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.First<Unit>(this.OnRefreshNeeded()), (Unit _) => this.SearchMatches()))));
		}

		private void StopMatchSearches()
		{
			if (this._searchDisposable != null)
			{
				this._searchDisposable.Dispose();
				this._searchDisposable = null;
			}
		}

		private void OnGroupStateChanged(Group group)
		{
			bool state = group != null;
			this._view.GameServersScroller.UpdateIsLocalPlayerInGroupState(state);
		}

		private void OnQueueStateChanged(MatchmakingQueueState queueState)
		{
			bool state = queueState.Step != 0;
			this._view.GameServersScroller.UpdateIsLocalPlayerInQueueState(state);
		}

		private void InitializeBackButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.First<Unit>(this._view.BackButton.OnClick()), (Unit _) => this.NavigateBackwards()));
			this._disposables.Add(disposable);
		}

		private IObservable<Unit> NavigateBackwards()
		{
			this._buttonBiLogger.LogButtonClick(ButtonName.StorytellerBack);
			this._view.MainCanvasGroup.Interactable = false;
			return this._mainMenuPresenterTree.PresenterTree.NavigateBackwards();
		}

		private IObservable<Unit> OnRefreshNeeded()
		{
			return Observable.Merge<Unit>(Observable.Merge<Unit>(Observable.Merge<Unit>(Observable.Merge<Unit>(Observable.Merge<Unit>(Observable.Merge<Unit>(Observable.AsUnitObservable<long>(Observable.Timer(TimeSpan.FromSeconds(240.0))), new IObservable<Unit>[]
			{
				this._view.RefreshButton.OnClick()
			}), new IObservable<Unit>[]
			{
				Observable.AsUnitObservable<string>(this._view.QueuesDropdown.OnSelectionChanged())
			}), new IObservable<Unit>[]
			{
				Observable.AsUnitObservable<string>(this._view.RegionsDropdown.OnSelectionChanged())
			}), new IObservable<Unit>[]
			{
				Observable.AsUnitObservable<Unit>(this._view.SearchButton.OnClick())
			}), new IObservable<Unit>[]
			{
				Observable.AsUnitObservable<string>(this._view.SearchField.OnSubmitWithEnter())
			}), new IObservable<Unit>[]
			{
				Observable.AsUnitObservable<string>(this._view.OnVirtualKeyboardClose())
			});
		}

		private string GetSearchQuery()
		{
			string text = this._view.SearchField.Text;
			if (text.Length >= 3)
			{
				return text.ToString(CultureInfo.InvariantCulture);
			}
			if (text.Length > 0)
			{
				this._view.PlaySearchFailedByMinCharsAnim();
			}
			return string.Empty;
		}

		private IObservable<Unit> SearchMatches()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (this.ShouldHideMatches())
				{
					this._view.NoResultsLabel.Text = this._localizeKey.Get("METAL_LEAGUE_SPECTATOR_FEEDBACK_INFO", TranslationContext.MainMenuGui);
					ActivatableExtensions.Activate(this._view.NoResultsGroup);
					this._view.GameServersScroller.SetItems(new IStorytellerMatchInfo[0]);
					return Observable.ReturnUnit();
				}
				this.ShowLoading();
				return Observable.DoOnCompleted<Unit>(Observable.AsUnitObservable<List<IStorytellerMatchInfo>>(Observable.Do<List<IStorytellerMatchInfo>>(Observable.ContinueWith<List<IStorytellerMatchInfo>, List<IStorytellerMatchInfo>>(Observable.Select<GameServerRunningInfo[], List<IStorytellerMatchInfo>>(Observable.Catch<GameServerRunningInfo[], Exception>(this._gameServerSearch.SearchServer(this.CreateMatchSearchRequest()), new Func<Exception, IObservable<GameServerRunningInfo[]>>(this.LogErrorAndReturnEmptyArray)), new Func<GameServerRunningInfo[], List<IStorytellerMatchInfo>>(this.CreateMatchesInfo)), new Func<List<IStorytellerMatchInfo>, IObservable<List<IStorytellerMatchInfo>>>(this.UpdateMemberNames)), new Action<List<IStorytellerMatchInfo>>(this.DisplayMatches))), new Action(this.HideLoading));
			});
		}

		private bool ShouldHideMatches()
		{
			StorytellerSearchableQueue storytellerSearchableQueue = this._searchableQueues.First((StorytellerSearchableQueue queue) => queue.QueueName == this._view.QueuesDropdown.SelectedOption);
			return storytellerSearchableQueue.IsTournamentTier && this._antiSnipingChecks.ShouldHideTournamentTierMatches(storytellerSearchableQueue.TournamentTier.Id);
		}

		private IObservable<GameServerRunningInfo[]> LogErrorAndReturnEmptyArray(Exception exception)
		{
			this._logger.ErrorFormat("Failed to get gameservers", new object[]
			{
				exception
			});
			return Observable.Return<GameServerRunningInfo[]>(new GameServerRunningInfo[0]);
		}

		private void ShowLoading()
		{
			this._view.GameServersScroller.Clear();
			this._view.DisableSearchInteractions();
			this._view.ShowLoadingMatches();
			ActivatableExtensions.Deactivate(this._view.NoResultsGroup);
		}

		private void HideLoading()
		{
			this.SetUiNavigationFocusOnTopGroup();
			this._view.HideLoadingMatches();
			this._view.EnableSearchInteractions();
		}

		private MatchSearchBag CreateMatchSearchRequest()
		{
			return new MatchSearchBag
			{
				Region = this._view.RegionsDropdown.SelectedOption,
				QueueName = this._view.QueuesDropdown.SelectedOption,
				SearchFilter = this.GetSearchQuery()
			};
		}

		private void DisplayMatches(List<IStorytellerMatchInfo> matches)
		{
			this._view.GameServersScroller.SetItems(matches);
			this._view.UiNavigationMatchListAxisSelector.Rebuild();
			this._isFoundMatch = (matches.Count > 0);
			if (matches.Count == 0)
			{
				this._view.NoResultsLabel.Text = this._localizeKey.Get("FEEDBACK_NO_RESULTS_FOUND", TranslationContext.Storyteller);
				ActivatableExtensions.Activate(this._view.NoResultsGroup);
			}
		}

		private List<IStorytellerMatchInfo> CreateMatchesInfo(GameServerRunningInfo[] servers)
		{
			List<IStorytellerMatchInfo> list = new List<IStorytellerMatchInfo>(servers.Length);
			foreach (GameServerRunningInfo gameServerRunningInfo in servers)
			{
				ServerStatusBag serverStatusBag = (ServerStatusBag)((JsonSerializeable<!0>)gameServerRunningInfo.GameServerStatus);
				if (serverStatusBag.ServerPhase != null)
				{
					IStorytellerMatchInfo storytellerMatchInfo = this._view.CreateMatchInfo();
					storytellerMatchInfo.MaxStorytellers = 2;
					storytellerMatchInfo.ServerInfo = gameServerRunningInfo;
					storytellerMatchInfo.ServerBag = serverStatusBag;
					storytellerMatchInfo.TranslatedPhase = this._translation.GetTranslatedServerPhase(serverStatusBag.ServerPhase);
					storytellerMatchInfo.IsLocalPlayerInGroup = (this._groupState.Group != null);
					storytellerMatchInfo.IsLocalPlayerInQueue = (this._queueState.Get().Step != 0);
					list.Add(storytellerMatchInfo);
					StorytellerMatchMember[] array = new StorytellerMatchMember[serverStatusBag.RedTeam.Length];
					for (int j = 0; j < serverStatusBag.RedTeam.Length; j++)
					{
						array[j] = new StorytellerMatchMember
						{
							PlayerId = serverStatusBag.RedTeamPlayerIds[j],
							PlayerName = serverStatusBag.RedTeamPlayerNames[j],
							PlayerTag = new long?(serverStatusBag.RedTeamPlayerTags[j]),
							PublisherId = serverStatusBag.RedTeamPublisherIds[j],
							PublisherUsername = serverStatusBag.RedTeamPublisherUserNames[j],
							UniversalId = serverStatusBag.RedTeam[j]
						};
						this.UpdatePublisherUsername(array[j]);
					}
					StorytellerMatchMember[] array2 = new StorytellerMatchMember[serverStatusBag.RedTeam.Length];
					for (int k = 0; k < serverStatusBag.BluTeam.Length; k++)
					{
						array2[k] = new StorytellerMatchMember
						{
							PlayerId = serverStatusBag.BluTeamPlayerIds[k],
							PlayerName = serverStatusBag.BluTeamPlayerNames[k],
							PlayerTag = new long?(serverStatusBag.BluTeamPlayerTags[k]),
							PublisherId = serverStatusBag.BluTeamPublisherIds[k],
							PublisherUsername = serverStatusBag.BluTeamPublisherUserNames[k],
							UniversalId = serverStatusBag.BluTeam[k]
						};
						this.UpdatePublisherUsername(array2[k]);
					}
					storytellerMatchInfo.RedMembers = array;
					storytellerMatchInfo.BlueMembers = array2;
				}
			}
			List<IStorytellerMatchInfo> list2 = list;
			if (StorytellerPresenter.<>f__mg$cache0 == null)
			{
				StorytellerPresenter.<>f__mg$cache0 = new Comparison<IStorytellerMatchInfo>(StorytellerPresenter.ServerSort);
			}
			list2.Sort(StorytellerPresenter.<>f__mg$cache0);
			return list;
		}

		private IObservable<List<IStorytellerMatchInfo>> UpdateMemberNames(List<IStorytellerMatchInfo> list)
		{
			return Observable.ContinueWith<Unit, List<IStorytellerMatchInfo>>(Observable.LastOrDefault<Unit>(Observable.Merge<Unit>(list.SelectMany((IStorytellerMatchInfo matchInfo) => matchInfo.BlueMembers.Concat(matchInfo.RedMembers)).Select(new Func<StorytellerMatchMember, IObservable<Unit>>(this.UpdateMemberName)))), (Unit _) => Observable.Return<List<IStorytellerMatchInfo>>(list));
		}

		private void UpdatePublisherUsername(StorytellerMatchMember member)
		{
			if (member.IsBot)
			{
				return;
			}
			Publisher publisherById = Publishers.GetPublisherById(member.PublisherId);
			PublisherPresentingData publisherPresentingData = this._getPublisherPresentingData.Get(publisherById);
			if (publisherPresentingData.ShouldShowPublisherUserName)
			{
				member.PublisherUsername = this._badNameCensor.TryCensor(member.PublisherUsername);
			}
			member.ShowPublisherUsername = publisherPresentingData.ShouldShowPublisherUserName;
		}

		private IObservable<Unit> UpdateMemberName(StorytellerMatchMember member)
		{
			if (this.IsBot(member.UniversalId))
			{
				return Observable.ReturnUnit();
			}
			IObservable<string> observable = (member.PlayerTag == null) ? this._getDisplayableNickName.GetLatestFormattedNickName(new DisplayableNicknameParameters
			{
				PlayerId = member.PlayerId,
				PlayerName = member.PlayerName,
				UniversalId = member.UniversalId
			}) : this._getDisplayableNickName.GetLatestFormattedNickNameWithPlayerTag(new DisplayableNicknameWithTagParameters
			{
				PlayerId = member.PlayerId,
				PlayerName = member.PlayerName,
				PlayerTag = member.PlayerTag,
				UniversalId = member.UniversalId
			});
			return Observable.AsUnitObservable<string>(Observable.Do<string>(observable, delegate(string name)
			{
				member.PlayerName = name;
			}));
		}

		private bool IsBot(string userId)
		{
			return userId == "-1";
		}

		private static int ServerSort(IStorytellerMatchInfo one, IStorytellerMatchInfo other)
		{
			ServerStatusBag.ServerPhaseKind serverPhase = one.ServerBag.ServerPhase;
			ServerStatusBag.ServerPhaseKind serverPhase2 = other.ServerBag.ServerPhase;
			if (serverPhase != serverPhase2)
			{
				return serverPhase.CompareTo(serverPhase2);
			}
			return -one.ServerBag.GetDate().CompareTo(other.ServerBag.GetDate());
		}

		private void ShowConnectingToServer()
		{
			this._view.MainCanvasGroup.Interactable = false;
			this._view.ShowWaitingWindow();
			this.StopMatchSearches();
		}

		private void HideConnectingToServer(bool hasConnectedToGameServer)
		{
			this._view.HideWaitingWindow();
			if (hasConnectedToGameServer)
			{
				this.DisposeAllExceptBackButton();
			}
			else
			{
				this._view.MainCanvasGroup.Interactable = true;
				this.StartMatchSearches();
			}
		}

		private const string SceneName = "UI_ADD_Storyteller";

		private const int AutoRefreshDelaySeconds = 240;

		private const int MinimumSearchLength = 3;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly IStorytellerGameserverSearch _gameServerSearch;

		private readonly IStorytellerMatchConnection _matchConnection;

		private readonly IRegionService _regions;

		private readonly IStorytellerQueueProvider _queues;

		private readonly IStorytellerTranslationProvider _translation;

		private readonly IGroupStorage _groupState;

		private readonly IGetThenObserveMatchmakingQueueState _queueState;

		private readonly ISubject<Unit> _hideSubject;

		private readonly IClientButtonBILogger _buttonBiLogger;

		private readonly ILocalizeKey _localizeKey;

		private readonly IAntiSnipingChecks _antiSnipingChecks;

		private readonly ILogger<StorytellerPresenter> _logger;

		private readonly IInputGetActiveDevicePoller _activeDevicePoller;

		private readonly IObserveCrossplayChange _observePsnCrossplayChange;

		private readonly IGetPublisherPresentingData _getPublisherPresentingData;

		private readonly IBadNameCensor _badNameCensor;

		private readonly IGetDisplayableNickName _getDisplayableNickName;

		private StorytellerSearchableQueue[] _searchableQueues;

		private IStorytellerPresenterView _view;

		private IDisposable _searchDisposable;

		private IDisposable _regionRefresh;

		private IDisposable _groupDisposable;

		private CompositeDisposable _disposables;

		private bool _isFoundMatch;

		[CompilerGenerated]
		private static Comparison<IStorytellerMatchInfo> <>f__mg$cache0;
	}
}
