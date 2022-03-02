using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Social;
using HeavyMetalMachines.ToggleableFeatures;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public class CompetitiveRankingScrollerDelegate : IScroller<CompetitiveRankingPlayerScrollerData>, IEnhancedScrollerDelegate
	{
		public CompetitiveRankingScrollerDelegate(EnhancedScroller scroller, UnityCompetitiveRankingPlayerView playerViewPrefab, IDisableLabelForFeatureNotEnable disableLabelForFeatureNotEnable, IGetFormattedPlayerTag getFormattedPlayerTag, IGetPublisherUserName getPublisherUserName, IGetDisplayableNickName getDisplayableNickName, IBadNameCensor badNameCensor)
		{
			this._scroller = scroller;
			this._scroller.Delegate = this;
			this._playerViewPrefab = playerViewPrefab;
			this._getFormattedPlayerTag = getFormattedPlayerTag;
			this._getPublisherUserName = getPublisherUserName;
			this._getDisplayableNickName = getDisplayableNickName;
			this._badNameCensor = badNameCensor;
			this._disposables = new CompositeDisposable();
			EnhancedScroller scroller2 = this._scroller;
			scroller2.cellViewVisibilityChanged = (CellViewVisibilityChangedDelegate)Delegate.Combine(scroller2.cellViewVisibilityChanged, new CellViewVisibilityChangedDelegate(this.CellViewVisibilityChanged));
			this._viewHidden = new Subject<IItemView>();
			this._viewShown = new Subject<IItemView>();
		}

		public void SetItems(IEnumerable<CompetitiveRankingPlayerScrollerData> data)
		{
			this._data = data.ToArray<CompetitiveRankingPlayerScrollerData>();
			this._dataViews = new UnityCompetitiveRankingPlayerView[this._data.Length];
			this._scroller.ReloadData(0f);
		}

		public void Clear()
		{
			this._data = new CompetitiveRankingPlayerScrollerData[0];
			this._lastEndDataIndex = null;
			this._scroller.ReloadData(0f);
		}

		public void JumpToIndex(int index)
		{
			throw new NotImplementedException();
		}

		public IObservable<IItemView> OnViewShown
		{
			get
			{
				return this._viewShown;
			}
		}

		public IObservable<IItemView> OnViewHidden
		{
			get
			{
				return this._viewHidden;
			}
		}

		public IObservable<Unit> ObserveBottomReached
		{
			get
			{
				return this._scroller.ObserveBottomReached();
			}
		}

		private void CellViewVisibilityChanged(EnhancedScrollerCellView cellview)
		{
			if (cellview.active)
			{
				this._viewShown.OnNext(cellview as IItemView);
			}
			else
			{
				this._viewHidden.OnNext(cellview as IItemView);
			}
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Length;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return this._playerViewPrefab.Height;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UnityCompetitiveRankingPlayerView unityCompetitiveRankingPlayerView = scroller.GetCellView(this._playerViewPrefab) as UnityCompetitiveRankingPlayerView;
			CompetitiveRankingPlayerScrollerData competitiveRankingPlayerScrollerData = this._data[dataIndex];
			unityCompetitiveRankingPlayerView.Model = this.CreatePlayer(competitiveRankingPlayerScrollerData);
			unityCompetitiveRankingPlayerView.PositionLabel.Text = competitiveRankingPlayerScrollerData.Position;
			unityCompetitiveRankingPlayerView.ScoreLabel.Text = competitiveRankingPlayerScrollerData.Score;
			unityCompetitiveRankingPlayerView.PlayerNameLabel.Text = competitiveRankingPlayerScrollerData.PlayerName;
			unityCompetitiveRankingPlayerView.SocialContextMenuButtonView.SetViewId("ranked " + competitiveRankingPlayerScrollerData.PlayerTag);
			unityCompetitiveRankingPlayerView.PlayerTagLabel.Text = this._getFormattedPlayerTag.Get(new long?(competitiveRankingPlayerScrollerData.PlayerTag));
			unityCompetitiveRankingPlayerView.SubdivisionDynamicImage.SetImageName(competitiveRankingPlayerScrollerData.SubdivisionImageName);
			this._disposables.Add(ObservableExtensions.Subscribe<Unit>(unityCompetitiveRankingPlayerView.SocialContextMenuButtonView.Button.OnClick()));
			this._dataViews[dataIndex] = unityCompetitiveRankingPlayerView;
			if (competitiveRankingPlayerScrollerData.IsLocalPlayer)
			{
				unityCompetitiveRankingPlayerView.SetAsLocalPlayer();
			}
			else
			{
				unityCompetitiveRankingPlayerView.SetAsOtherPlayer();
			}
			if (this._lastEndDataIndex == null || dataIndex < this._lastEndDataIndex)
			{
				unityCompetitiveRankingPlayerView.AnimateComingFromAbove();
			}
			else
			{
				unityCompetitiveRankingPlayerView.AnimateComingFromBelow();
			}
			if (this._scroller.StartDataIndex > 0 || this._scroller.EndDataIndex > 0)
			{
				this._lastEndDataIndex = new int?(this._scroller.EndDataIndex);
			}
			return unityCompetitiveRankingPlayerView;
		}

		private IIdentifiable CreatePlayer(CompetitiveRankingPlayerScrollerData data)
		{
			return new Player
			{
				PlayerId = data.PlayerId,
				Nickname = data.PlayerName,
				PlayerTag = new long?(data.PlayerTag)
			};
		}

		public IObservable<Unit> FadeOutAllItems()
		{
			IObservable<Unit> observable = Observable.ReturnUnit();
			if (this._scroller.NumberOfCells == 0)
			{
				return observable;
			}
			for (int i = this._scroller.StartDataIndex; i <= this._scroller.EndDataIndex; i++)
			{
				observable = Observable.Merge<Unit>(observable, new IObservable<Unit>[]
				{
					this._dataViews[i].AnimateFadingOut()
				});
			}
			return Observable.Last<Unit>(observable);
		}

		public void Dispose()
		{
			this._disposables.Dispose();
		}

		private readonly EnhancedScroller _scroller;

		private readonly UnityCompetitiveRankingPlayerView _playerViewPrefab;

		private CompetitiveRankingPlayerScrollerData[] _data = new CompetitiveRankingPlayerScrollerData[0];

		private UnityCompetitiveRankingPlayerView[] _dataViews = new UnityCompetitiveRankingPlayerView[0];

		private readonly IGetFormattedPlayerTag _getFormattedPlayerTag;

		private readonly IGetPublisherUserName _getPublisherUserName;

		private readonly IGetDisplayableNickName _getDisplayableNickName;

		private readonly IBadNameCensor _badNameCensor;

		private readonly CompositeDisposable _disposables;

		private readonly Subject<IItemView> _viewShown;

		private readonly Subject<IItemView> _viewHidden;

		private int? _lastEndDataIndex;
	}
}
