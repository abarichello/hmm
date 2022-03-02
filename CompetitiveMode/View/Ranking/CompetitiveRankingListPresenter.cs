using System;
using System.Collections.Generic;
using System.Linq;
using ClientAPI.Utils;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Social.ContextMenu.Presenting;
using Hoplon.Localization.TranslationTable;
using Hoplon.Time;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public class CompetitiveRankingListPresenter : ICompetitiveRankingListPresenter
	{
		public CompetitiveRankingListPresenter(IViewProvider viewProvider, ICurrentTime currentTime, ILocalizeKey translation, ICompetitiveDivisionsBadgeNameBuilder competitiveDivisionsBadgeNameBuilder, IGetCompetitiveDivisions getCompetitiveDivisions, IObserveCrossplayChange observeCrossplayChange, IGetDisplayableNickName getDisplayableNickName, DiContainer diContainer)
		{
			this._viewProvider = viewProvider;
			this._currentTime = currentTime;
			this._translation = translation;
			this._competitiveDivisionsBadgeNameBuilder = competitiveDivisionsBadgeNameBuilder;
			this._getCompetitiveDivisions = getCompetitiveDivisions;
			this._observeCrossplayChange = observeCrossplayChange;
			this._getDisplayableNickName = getDisplayableNickName;
			this._diContainer = diContainer;
			this._socialContextMenuButtonPresenters = new Dictionary<long, ISocialContextMenuButtonPresenter>();
		}

		public Func<int, IObservable<PlayerCompetitiveRankingPosition[]>> GetRankings { get; set; }

		public int RankingPlayersCount { get; set; }

		public IObservable<Unit> OnListRefreshed
		{
			get
			{
				return this._listRefreshedSubject;
			}
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		private void InitializeView()
		{
			this._disposables = new CompositeDisposable();
			this._view = this._viewProvider.Provide<ICompetitiveRankingListView>(null);
			this._view.LastUpdatedLabel.Text = string.Empty;
			ActivatableExtensions.Deactivate(this._view.EmptyRankingGroup);
			this._rankingUpdate = ObservableExtensions.Subscribe<Unit>(Observable.Switch<Unit>(Observable.Select<Unit, IObservable<Unit>>(Observable.ContinueWith<Unit, Unit>(Observable.Defer<Unit>(new Func<IObservable<Unit>>(this.RefreshRanking)), Observable.Merge<Unit>(this._view.RefreshButton.OnClick(), new IObservable<Unit>[]
			{
				Observable.AsUnitObservable<bool>(this._observeCrossplayChange.Observe())
			})), (Unit _) => this.RefreshRanking())));
			this.InitializeSocialContextMenuButton();
		}

		private void InitializeSocialContextMenuButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<IItemView>(Observable.Do<IItemView>(this._view.RankingScroller.OnViewShown, new Action<IItemView>(this.HandleViewShown)));
			this._disposables.Add(disposable);
			IDisposable disposable2 = ObservableExtensions.Subscribe<IItemView>(Observable.Do<IItemView>(this._view.RankingScroller.OnViewHidden, new Action<IItemView>(this.HandleViewHidden)));
			this._disposables.Add(disposable2);
		}

		private void HandleViewShown(IItemView view)
		{
			this.ConfigureContextMenu(view.Model as IPlayer);
		}

		private void HandleViewHidden(IItemView view)
		{
			this.UnbindContextMenu(view.Model as IPlayer);
		}

		private void UnbindContextMenu(IPlayer player)
		{
			IViewProvider viewProvider = this._diContainer.Resolve<IViewProvider>();
			try
			{
				string contextMenuViewId = this.GetContextMenuViewId(player);
				viewProvider.Unbind<ISocialContextMenuButtonView>(contextMenuViewId);
			}
			catch (AssertionException ex)
			{
				Debug.LogWarning(ex.Message);
			}
		}

		private string GetContextMenuViewId(IPlayer player)
		{
			return "ranked " + player.PlayerTag;
		}

		private void ConfigureContextMenu(IPlayer player)
		{
			ISocialContextMenuButtonPresenter orCreateSocialContextMenuButtonPresenter = this.GetOrCreateSocialContextMenuButtonPresenter(player.PlayerTag.Value);
			IViewProvider viewProvider = this._diContainer.Resolve<IViewProvider>();
			try
			{
				string contextMenuViewId = this.GetContextMenuViewId(player);
				ISocialContextMenuButtonView view = viewProvider.Provide<ISocialContextMenuButtonView>(contextMenuViewId);
				orCreateSocialContextMenuButtonPresenter.SetView(view);
				orCreateSocialContextMenuButtonPresenter.SetPlayer(player);
				IDisposable disposable = ObservableExtensions.Subscribe<Unit>(orCreateSocialContextMenuButtonPresenter.Initialize());
				this._disposables.Add(disposable);
			}
			catch (AssertionException ex)
			{
				Debug.LogWarning(ex.Message);
			}
		}

		private ISocialContextMenuButtonPresenter GetOrCreateSocialContextMenuButtonPresenter(long playerTag)
		{
			ISocialContextMenuButtonPresenter socialContextMenuButtonPresenter;
			if (!this._socialContextMenuButtonPresenters.TryGetValue(playerTag, out socialContextMenuButtonPresenter))
			{
				socialContextMenuButtonPresenter = this._diContainer.Resolve<ISocialContextMenuButtonPresenter>();
				this._socialContextMenuButtonPresenters.Add(playerTag, socialContextMenuButtonPresenter);
			}
			return socialContextMenuButtonPresenter;
		}

		private IObservable<Unit> RefreshRanking()
		{
			return Observable.AsUnitObservable<PlayerCompetitiveRankingPosition[]>(Observable.Do<PlayerCompetitiveRankingPosition[]>(Observable.Delay<PlayerCompetitiveRankingPosition[]>(Observable.Do<PlayerCompetitiveRankingPosition[]>(Observable.Do<PlayerCompetitiveRankingPosition[]>(Observable.Do<PlayerCompetitiveRankingPosition[]>(Observable.ContinueWith<PlayerCompetitiveRankingPosition[], PlayerCompetitiveRankingPosition[]>(Observable.ContinueWith<Unit, PlayerCompetitiveRankingPosition[]>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.SetupBeforeRefreshRanking();
			}), (Unit _) => this._view.AnimateFadeOutScrollerItems()), delegate(Unit _)
			{
				this._view.RankingScroller.Clear();
			}), (Unit _) => this.GetRankings(this.RankingPlayersCount)), new Func<PlayerCompetitiveRankingPosition[], IObservable<PlayerCompetitiveRankingPosition[]>>(this.UpdateNicknames)), new Action<PlayerCompetitiveRankingPosition[]>(this.AddRankingsToScroller)), new Action<PlayerCompetitiveRankingPosition[]>(this.ShowEmptyRankingMessageIfNeeded)), delegate(PlayerCompetitiveRankingPosition[] _)
			{
				this.SetupAfterRefreshRanking();
			}), TimeSpan.FromSeconds(1.0)), delegate(PlayerCompetitiveRankingPosition[] _)
			{
				this.Gambs();
			}));
		}

		private void SetupBeforeRefreshRanking()
		{
			this._view.LastUpdatedLabel.Text = string.Empty;
			ActivatableExtensions.Activate(this._view.LoadingActivatable);
			this._view.RefreshButton.IsInteractable = false;
			this._view.DisableRankingScroll();
			ActivatableExtensions.Deactivate(this._view.EmptyRankingGroup);
		}

		private void Gambs()
		{
			((UnityCompetitiveRankingListView)this._view)._axisSelector.RebuildAndSelect();
		}

		private void SetupAfterRefreshRanking()
		{
			this._view.EnableRankingScroll();
			ActivatableExtensions.Deactivate(this._view.LoadingActivatable);
			this._view.RefreshButton.IsInteractable = true;
			this.UpdateLastUpdatedLabel();
			this._listRefreshedSubject.OnNext(Unit.Default);
		}

		private void UpdateLastUpdatedLabel()
		{
			DateTime dateTime = this._currentTime.Now();
			this._view.LastUpdatedLabel.Text = this._translation.GetFormatted("RANKING_HINT_UPDATE", TranslationContext.Ranked, new object[]
			{
				dateTime.ToString("HH:mm")
			});
		}

		private void AddRankingsToScroller(PlayerCompetitiveRankingPosition[] playerCompetitiveRanks)
		{
			CompetitiveRankingPlayerScrollerData[] items = this.ConvertToPlayerData(playerCompetitiveRanks).ToArray<CompetitiveRankingPlayerScrollerData>();
			this._view.RankingScroller.SetItems(items);
		}

		private IObservable<PlayerCompetitiveRankingPosition[]> UpdateNicknames(PlayerCompetitiveRankingPosition[] playerCompetitiveRanks)
		{
			return Observable.ContinueWith<Unit, PlayerCompetitiveRankingPosition[]>(Observable.Merge<Unit>(playerCompetitiveRanks.Select(new Func<PlayerCompetitiveRankingPosition, IObservable<Unit>>(this.UpdateNickname))), (Unit _) => Observable.Return<PlayerCompetitiveRankingPosition[]>(playerCompetitiveRanks));
		}

		private IObservable<Unit> UpdateNickname(PlayerCompetitiveRankingPosition rank)
		{
			return Observable.AsUnitObservable<string>(Observable.Do<string>(this._getDisplayableNickName.GetLatestFormattedNickName(new DisplayableNicknameParameters
			{
				PlayerId = rank.PlayerId,
				PlayerName = rank.Name,
				UniversalId = rank.UniversalId
			}), delegate(string name)
			{
				rank.Name = name;
			}));
		}

		private void ShowEmptyRankingMessageIfNeeded(PlayerCompetitiveRankingPosition[] playerCompetitiveRankingsPosition)
		{
			if (playerCompetitiveRankingsPosition == null || playerCompetitiveRankingsPosition.Length == 0)
			{
				ActivatableExtensions.Activate(this._view.EmptyRankingGroup);
			}
		}

		private IEnumerable<CompetitiveRankingPlayerScrollerData> ConvertToPlayerData(IList<PlayerCompetitiveRankingPosition> playerCompetitiveRanks)
		{
			CompetitiveRankingPlayerScrollerData[] array = new CompetitiveRankingPlayerScrollerData[playerCompetitiveRanks.Count];
			Division[] divisions = this._getCompetitiveDivisions.Get();
			for (int i = 0; i < playerCompetitiveRanks.Count; i++)
			{
				array[i] = this.ConvertToPlayerData(playerCompetitiveRanks[i], divisions);
			}
			return array;
		}

		private CompetitiveRankingPlayerScrollerData ConvertToPlayerData(PlayerCompetitiveRankingPosition playerCompetitiveRank, Division[] divisions)
		{
			CompetitiveRank rank = playerCompetitiveRank.Rank;
			string subdivisionImageName;
			if (rank.TopPlacementPosition != null)
			{
				subdivisionImageName = this._competitiveDivisionsBadgeNameBuilder.GetTopDivisionBadgeFileName(30);
			}
			else
			{
				Division division = divisions[rank.Division];
				subdivisionImageName = this._competitiveDivisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(division, rank.Subdivision, 30);
			}
			bool isPsnUser = UniversalIdUtil.IsPsnId(playerCompetitiveRank.UniversalId);
			return new CompetitiveRankingPlayerScrollerData
			{
				Score = rank.Score.ToString(),
				Position = playerCompetitiveRank.Position.ToString(),
				DivisionIndex = rank.Division,
				SubdivisionIndex = rank.Subdivision,
				PlayerId = playerCompetitiveRank.PlayerId,
				PlayerName = playerCompetitiveRank.Name,
				PlayerTag = playerCompetitiveRank.PlayerTag,
				IsLocalPlayer = playerCompetitiveRank.IsLocalPlayer,
				SubdivisionImageName = subdivisionImageName,
				IsPsnUser = isPsnUser,
				UniversalId = playerCompetitiveRank.UniversalId
			};
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(this._view.FadeInAnimation.Play(), delegate(Unit _)
			{
				this._shownSubject.OnNext(Unit.Default);
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(this._view.FadeOutAnimation.Play(), delegate(Unit _)
			{
				this._view.RankingScroller.Clear();
			});
		}

		public void Dispose()
		{
			this._rankingUpdate.Dispose();
			this._disposables.Dispose();
			this._disposables = null;
		}

		private readonly IViewProvider _viewProvider;

		private readonly ICurrentTime _currentTime;

		private readonly ILocalizeKey _translation;

		private readonly IGetCompetitiveDivisions _getCompetitiveDivisions;

		private readonly IObserveCrossplayChange _observeCrossplayChange;

		private readonly IGetDisplayableNickName _getDisplayableNickName;

		private readonly DiContainer _diContainer;

		private readonly Subject<Unit> _shownSubject = new Subject<Unit>();

		private readonly Subject<Unit> _listRefreshedSubject = new Subject<Unit>();

		private readonly ICompetitiveDivisionsBadgeNameBuilder _competitiveDivisionsBadgeNameBuilder;

		private ICompetitiveRankingListView _view;

		private IDisposable _rankingUpdate;

		private CompositeDisposable _disposables;

		private readonly Dictionary<long, ISocialContextMenuButtonPresenter> _socialContextMenuButtonPresenters;
	}
}
