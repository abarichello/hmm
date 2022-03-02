using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Publishing.Presenting;
using UniRx;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Rankings
{
	public class UnityTournamentRankingScroller : ITournamentRankingScroller, IEnhancedScrollerDelegate, IScroller<TournamentRankingScrollerData>
	{
		public UnityTournamentRankingScroller(EnhancedScroller scroller, UnityTournamentRankingCellView topCellViewPrefab, UnityTournamentRankingCellView cellViewPrefab, ITeamNameRestriction teamNameRestriction, IGetDisplayablePublisherUserName getDisplayablePublisherUserName, bool shouldShowTooltip)
		{
			this._scroller = scroller;
			scroller.Delegate = this;
			this._topCellViewPrefab = topCellViewPrefab;
			this._cellViewPrefab = cellViewPrefab;
			this._teamNameRestriction = teamNameRestriction;
			this._getDisplayablePublisherUserName = getDisplayablePublisherUserName;
			this._shouldShowTooltip = shouldShowTooltip;
		}

		public void SetItems(IEnumerable<TournamentRankingScrollerData> item)
		{
			this._items = item.ToList<TournamentRankingScrollerData>();
			this._scroller.ReloadData(0f);
		}

		public void Clear()
		{
			if (this._items != null)
			{
				this._items.Clear();
				this._scroller.ReloadData(0f);
			}
		}

		public void JumpToIndex(int index)
		{
			this._scroller.JumpToDataIndex(index, 0f, 0f, true, EnhancedScroller.TweenType.immediate, 0f, null, EnhancedScroller.LoopJumpDirectionEnum.Closest);
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
			return (this._items != null) ? this._items.Count : 0;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return (!this._items[dataIndex].IsTopCell) ? this._cellViewPrefab.GetSize() : this._topCellViewPrefab.GetSize();
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			EnhancedScrollerCellView result;
			if (this._items[dataIndex].IsTopCell)
			{
				result = this.InitializeTopCellView(dataIndex, scroller.GetCellView(this._topCellViewPrefab) as ITournamentRankingTopCellView, this._items[dataIndex]);
			}
			else
			{
				result = this.InitializeCellView(scroller.GetCellView(this._cellViewPrefab) as ITournamentRankingCellView, this._items[dataIndex]);
			}
			return result;
		}

		private EnhancedScrollerCellView InitializeTopCellView(int dataIndex, ITournamentRankingTopCellView cellView, TournamentRankingScrollerData scrollerData)
		{
			switch (scrollerData.Place)
			{
			case 1:
				cellView.SetFirstPlace();
				break;
			case 2:
				cellView.SetSecondPlace();
				break;
			case 3:
				cellView.SetThirdPlace();
				break;
			default:
				cellView.Ranking2Text.Text = scrollerData.Place.ToString("0");
				cellView.ShowRanking2Text();
				break;
			}
			return this.InitializeCellView(cellView, scrollerData);
		}

		private EnhancedScrollerCellView InitializeCellView(ITournamentRankingCellView cellView, TournamentRankingScrollerData scrollerData)
		{
			cellView.RankingText.Text = scrollerData.Place.ToString("0");
			cellView.ScoreText.Text = scrollerData.Score;
			cellView.ClassificatoryText.Text = scrollerData.Classificatory;
			this.FetchAndFillTeamUserGeneratedContentPublisherUserName(cellView, scrollerData);
			cellView.TeamTagText.Text = this._teamNameRestriction.GetTeamTagGlobalRestriction(scrollerData.TeamTag);
			cellView.TeamNameText.Text = this._teamNameRestriction.GetTeamNameGlobalRestriction(scrollerData.TeamName);
			cellView.LoadTeamIcon(scrollerData.TeamIconAssetName);
			if (scrollerData.IsMyPosition)
			{
				cellView.SetAsMyTeam();
			}
			else
			{
				cellView.SetAsNotMyTeam();
			}
			if (scrollerData.IsDeleted)
			{
				cellView.SetAsDeleted();
			}
			else
			{
				cellView.SetAsNotDeleted();
			}
			if (!this._shouldShowTooltip)
			{
				cellView.DisableTeamMembersTooltip();
			}
			cellView.SetTeamMembersTooltip(this.GetTeamMembersTooltipText(scrollerData));
			if (this._shouldClassificatoryBeVisible)
			{
				cellView.SetClassificatoryVisible();
			}
			else
			{
				cellView.SetClassificatoryNotVisible();
			}
			cellView.SetTieBreakerTooltip(scrollerData.TieBreakerTooltipText);
			return cellView as EnhancedScrollerCellView;
		}

		private void FetchAndFillTeamUserGeneratedContentPublisherUserName(ITournamentRankingCellView cellView, TournamentRankingScrollerData scrollerData)
		{
			cellView.TeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel.Text = string.Empty;
			ObservableExtensions.Subscribe<string>(Observable.Do<string>(this._getDisplayablePublisherUserName.GetAsTeamUgcOwner(scrollerData.CurrentUgmUserUniversalId), delegate(string displayablePublisherUserName)
			{
				cellView.TeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel.Text = displayablePublisherUserName;
			}));
		}

		private string GetTeamMembersTooltipText(TournamentRankingScrollerData scrollerData)
		{
			string text = string.Empty;
			string[] teamMembersName = scrollerData.TeamMembersName;
			if (teamMembersName != null && teamMembersName.Length > 0)
			{
				text = teamMembersName[0];
				for (int i = 1; i < teamMembersName.Length; i++)
				{
					text = text + Environment.NewLine + teamMembersName[i];
				}
			}
			return text;
		}

		public void SetClassificatoryVisible()
		{
			this._shouldClassificatoryBeVisible = true;
		}

		public void SetClassificatoryNotVisible()
		{
			this._shouldClassificatoryBeVisible = false;
		}

		private readonly UnityTournamentRankingCellView _topCellViewPrefab;

		private readonly UnityTournamentRankingCellView _cellViewPrefab;

		private readonly ITeamNameRestriction _teamNameRestriction;

		private readonly IGetDisplayablePublisherUserName _getDisplayablePublisherUserName;

		private bool _shouldShowTooltip;

		private readonly EnhancedScroller _scroller;

		private List<TournamentRankingScrollerData> _items;

		private bool _shouldClassificatoryBeVisible;
	}
}
