using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Schedules
{
	public class UnityTournamentStepScroller : IEnhancedScrollerDelegate, IScroller<TournamentStepCellData>
	{
		public UnityTournamentStepScroller(EnhancedScroller scroller, UnityTournamentStepCellView cellViewPrefab)
		{
			this._scroller = scroller;
			scroller.Delegate = this;
			this._cellViewPrefab = cellViewPrefab;
			this._onSelectRanking = new Subject<int>();
		}

		public void SetItems(IEnumerable<TournamentStepCellData> item)
		{
			this._stepViews = item.ToList<TournamentStepCellData>();
			this._scroller.ReloadData(0f);
		}

		public void Clear()
		{
			if (this._stepViews != null)
			{
				this._stepViews.Clear();
				this._scroller.ReloadData(0f);
			}
		}

		public void JumpToIndex(int index)
		{
			if (this._stepViews.Count > 3)
			{
				if (index >= this._stepViews.Count - 2)
				{
					this._scroller.ScrollPosition = this._scroller.ScrollSize;
				}
				else
				{
					this._scroller.JumpToDataIndex(index, 0.342f, 0f, true, EnhancedScroller.TweenType.immediate, 0f, null, EnhancedScroller.LoopJumpDirectionEnum.Closest);
				}
			}
		}

		public IObservable<IItemView> OnViewShown { get; private set; }

		public IObservable<IItemView> OnViewHidden { get; private set; }

		public IObservable<Unit> ObserveBottomReached
		{
			get
			{
				return this._scroller.ObserveBottomReached();
			}
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return (this._stepViews != null) ? this._stepViews.Count : 0;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return this._cellViewPrefab.GetSize();
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UnityTournamentStepCellView unityTournamentStepCellView = scroller.GetCellView(this._cellViewPrefab) as UnityTournamentStepCellView;
			this.InitializeCellView(dataIndex, unityTournamentStepCellView);
			return unityTournamentStepCellView;
		}

		private void InitializeCellView(int dataIndex, ITournamentStepCellView cellView)
		{
			TournamentStepCellData tournamentStepCellData = this._stepViews[dataIndex];
			cellView.DataIndex = dataIndex;
			cellView.PositionLabel.Text = string.Format("{0}ª", dataIndex + 1);
			cellView.TitleLabel.Text = tournamentStepCellData.Title;
			cellView.DateLabel.Text = tournamentStepCellData.StartDate;
			cellView.DayOfWeekLabel.Text = tournamentStepCellData.DayOfWeek;
			cellView.PeriodLabel.Text = tournamentStepCellData.PeriodTime;
			if (tournamentStepCellData.IsOld)
			{
				cellView.SetAsOldStep();
			}
			else if (tournamentStepCellData.IsHighlighted)
			{
				cellView.SetAsClosestStep();
			}
			else
			{
				cellView.SetAsFutureStep();
			}
			ObservableExtensions.Subscribe<int>(cellView.ObserveRankingButtonClick(), new Action<int>(this.OnSelectRanking));
		}

		private void OnSelectRanking(int dataIndex)
		{
			this._onSelectRanking.OnNext(dataIndex);
		}

		public IObservable<int> ObserveStepRankingSelection()
		{
			return this._onSelectRanking;
		}

		private readonly UnityTournamentStepCellView _cellViewPrefab;

		private readonly EnhancedScroller _scroller;

		private readonly Subject<int> _onSelectRanking;

		private List<TournamentStepCellData> _stepViews;
	}
}
