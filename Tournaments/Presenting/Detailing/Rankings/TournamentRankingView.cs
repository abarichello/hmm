using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Publishing.Presenting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Rankings
{
	public class TournamentRankingView : MonoBehaviour, ITournamentRankingView
	{
		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroupUnity;
			}
		}

		public IButton ScoreRulesButton
		{
			get
			{
				return this._scoreRulesButtonUnity;
			}
		}

		public IButton RefreshButton
		{
			get
			{
				return this._refreshButtonUnity;
			}
		}

		public IButton FinalsButton
		{
			get
			{
				return this._finalsButtonUnity;
			}
		}

		public IDropdown<int> RangeDropdown
		{
			get
			{
				return this._rangeDropdownUnity;
			}
		}

		public IDropdown<int> StepsDropdown
		{
			get
			{
				return this._stepsDropdownUnity;
			}
		}

		public ILabel CalendarTitleLabel
		{
			get
			{
				return this._calendarTitleLabelUnity;
			}
		}

		public ILabel UpdateInfoLabel
		{
			get
			{
				return this._updateIndoLabelUnity;
			}
		}

		public ITournamentRankingScroller RankingScroller
		{
			get
			{
				return this._rankingScroller;
			}
		}

		public IActivatable InfoGroup
		{
			get
			{
				return this._infoGroupGameObject;
			}
		}

		public IActivatable TitlesGroup
		{
			get
			{
				return this._titlesGroupGameObject;
			}
		}

		private void Start()
		{
			this._viewProvider.Bind<ITournamentRankingView>(this, null);
			bool shouldShowTooltip = this._getHostPlatform.GetCurrent() == 0;
			this._rankingScroller = new UnityTournamentRankingScroller(this._enhancedScroller, this._rankingTopCellViewPrefab, this._rankingCellViewPrefab, this._teamNameRestriction, this._getDisplayablePublisherUserName, shouldShowTooltip);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ITournamentRankingView>(null);
		}

		public void SetupScrollerAsColiseum()
		{
			this._finalsButtonGameObject.SetActive(false);
			this._classificatoryTitleGameObject.SetActive(false);
			this.SetScrollViewHorizontalPosition(355);
			this.RankingScroller.SetClassificatoryNotVisible();
		}

		public void ShowLoading()
		{
			this._loadingGameObject.SetActive(true);
		}

		public void HideLoading()
		{
			this._loadingGameObject.SetActive(false);
		}

		public void ShowFeedback(string message)
		{
			this._feedbackGameObject.SetActive(true);
			this._feedbackText.text = message;
		}

		public void HideFeedback()
		{
			this._feedbackGameObject.SetActive(false);
		}

		public void SetPointsTitleText(string translatedText)
		{
			this._pointsTitleText.text = translatedText;
		}

		private void SetScrollViewHorizontalPosition(int position)
		{
			Vector2 anchoredPosition = this._scrollViewRectTransform.anchoredPosition;
			anchoredPosition.x = (float)position;
			this._scrollViewRectTransform.anchoredPosition = anchoredPosition;
		}

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroupUnity;

		[SerializeField]
		private UnityButton _scoreRulesButtonUnity;

		[SerializeField]
		private UnityButton _refreshButtonUnity;

		[SerializeField]
		private UnityButton _finalsButtonUnity;

		[SerializeField]
		private IntUnityDropdown _rangeDropdownUnity;

		[SerializeField]
		private IntUnityDropdown _stepsDropdownUnity;

		[SerializeField]
		private UnityLabel _calendarTitleLabelUnity;

		[SerializeField]
		private UnityLabel _updateIndoLabelUnity;

		[SerializeField]
		private EnhancedScroller _enhancedScroller;

		[SerializeField]
		private UnityTournamentRankingCellView _rankingTopCellViewPrefab;

		[SerializeField]
		private UnityTournamentRankingCellView _rankingCellViewPrefab;

		[SerializeField]
		private RectTransform _scrollViewRectTransform;

		[SerializeField]
		private GameObject _classificatoryTitleGameObject;

		[SerializeField]
		private GameObject _finalsButtonGameObject;

		[SerializeField]
		private GameObject _loadingGameObject;

		[SerializeField]
		private GameObject _feedbackGameObject;

		[SerializeField]
		private Text _feedbackText;

		[SerializeField]
		private Text _pointsTitleText;

		[SerializeField]
		private GameObjectActivatable _infoGroupGameObject;

		[SerializeField]
		private GameObjectActivatable _titlesGroupGameObject;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private ITeamNameRestriction _teamNameRestriction;

		[Inject]
		private IGetHostPlatform _getHostPlatform;

		[Inject]
		private IGetDisplayablePublisherUserName _getDisplayablePublisherUserName;

		private UnityTournamentRankingScroller _rankingScroller;
	}
}
