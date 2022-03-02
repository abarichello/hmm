using System;
using Assets.Standard_Assets.Scripts.Infra.UnityUI;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.UnityUI;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing
{
	public class TournamentPresenterView : MonoBehaviour, ITournamentPresenterView
	{
		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroup;
			}
		}

		public ITitle Title
		{
			get
			{
				return this._titleInfo;
			}
		}

		public IButton JoinButton
		{
			get
			{
				return this._joinButton;
			}
		}

		public IButton UnsubscribeFromTournamentButton
		{
			get
			{
				return this._unsubscribeFromTournamentButton;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IButton TeamRulesButton
		{
			get
			{
				return this._teamRulesButton;
			}
		}

		public IToggle TeamToggle
		{
			get
			{
				return this._teamToggle;
			}
		}

		public IToggle HomeToggle
		{
			get
			{
				return this._homeToggle;
			}
		}

		public IToggle RankingToggle
		{
			get
			{
				return this._rankingToggle;
			}
		}

		public ILabel TeamToggleLabel
		{
			get
			{
				return this._teamToggleLabel;
			}
		}

		public ILabel ScheduleToggleLabel
		{
			get
			{
				return this._scheduleToggleLabel;
			}
		}

		public ILabel RankingToggleLabel
		{
			get
			{
				return this._rankingToggleLabel;
			}
		}

		public ILabel TimeLabel
		{
			get
			{
				return this._timeLabel;
			}
		}

		public ILabel TimeDescriptionLabel
		{
			get
			{
				return this._timeDescriptionLabel;
			}
		}

		public ILabel TournamentTierLabel
		{
			get
			{
				return this._tournamentTierLabel;
			}
		}

		public ILabel RegionLabel
		{
			get
			{
				return this._regionLabel;
			}
		}

		public ILabel VictoriesCriteriaValueLabel
		{
			get
			{
				return this._victoriesCriteriaValueLabel;
			}
		}

		public ILabel CompetitiveCriteriaValueLabel
		{
			get
			{
				return this._competitiveCriteriaValueLabel;
			}
		}

		public ILabel CriteriaOperatorLabel
		{
			get
			{
				return this._criteriaOperatorLabel;
			}
		}

		public IDynamicImage LogoImage
		{
			get
			{
				return this._logoImage;
			}
		}

		public ILabel LogoNameLabel
		{
			get
			{
				return this._logoNameLabel;
			}
		}

		public IAnimation ShowAnimation
		{
			get
			{
				return this._showAnimation;
			}
		}

		public IAnimation HideAnimation
		{
			get
			{
				return this._hideAnimation;
			}
		}

		public IAnimation TeamTabShowAnimation
		{
			get
			{
				return this._teamTabShowAnimation;
			}
		}

		public IAnimation TeamTabHideAnimation
		{
			get
			{
				return this._teamTabHideAnimation;
			}
		}

		public IAnimation StepsTabShowFromLeftAnimation
		{
			get
			{
				return this._stepsTabShowFromLeftAnimation;
			}
		}

		public IAnimation StepsTabShowFromRightAnimation
		{
			get
			{
				return this._stepsTabShowFromRightAnimation;
			}
		}

		public IAnimation StepsTabHideToLeftAnimation
		{
			get
			{
				return this._stepsTabHideToLeftAnimation;
			}
		}

		public IAnimation StepsTabHideToRightAnimation
		{
			get
			{
				return this._stepsTabHideToRightAnimation;
			}
		}

		public IAnimation RankingTabShowAnimation
		{
			get
			{
				return this._rankingTabShowAnimation;
			}
		}

		public IAnimation RankingTabHideAnimation
		{
			get
			{
				return this._rankingTabHideAnimation;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IUiNavigationSubGroupHolder LeftUiNavigationSubGroupHolder
		{
			get
			{
				return this._leftUiNavigationSubGroupHolder;
			}
		}

		public IUiNavigationAxisSelector LeftUiNavigationAxisSelector
		{
			get
			{
				return this._leftUiNavigationAxisSelector;
			}
		}

		public IUiNavigationSubGroupHolder HomeUiNavigationSubGroupHolder
		{
			get
			{
				return this._homeUiNavigationSubGroupHolder;
			}
		}

		public IUiNavigationAxisSelector HomeUiNavigationAxisSelector
		{
			get
			{
				return this._homeUiNavigationAxisSelector;
			}
		}

		public IUiNavigationSubGroupHolder RankingUiNavigationSubGroupHolder
		{
			get
			{
				return this._rankingUiNavigationSubGroupHolder;
			}
		}

		public IUiNavigationAxisSelector RankingUiNavigationAxisSelector
		{
			get
			{
				return this._rankingUiNavigationAxisSelector;
			}
		}

		public IUiNavigationSubGroupHolder TeamUiNavigationSubGroupHolder
		{
			get
			{
				return this._teamUiNavigationSubGroupHolder;
			}
		}

		public IUiNavigationAxisSelector TeamUiNavigationAxisSelector
		{
			get
			{
				return this._teamUiNavigationAxisSelector;
			}
		}

		public IActivatable ContextualTimeInfoActivatable
		{
			get
			{
				return this._contextualTimeInfoGameObject;
			}
		}

		public IActivatable JoinFeedbacksContainer
		{
			get
			{
				return new GameObjectActivatable(this._joinFeedbacksContainer.gameObject);
			}
		}

		public ILabel CrossplayActivationIndicatorLabel
		{
			get
			{
				return this._crossplayActivationIndicatorLabel;
			}
		}

		public void SetTitleInfo(string title, string subTitle)
		{
			this._titleInfo.Setup(title, HmmUiText.TextStyles.TitleCase, subTitle, HmmUiText.TextStyles.TitleCase, string.Empty, HmmUiText.TextStyles.Default, true);
		}

		public void SetSubTitle(string subTitle)
		{
			this._titleInfo.Subtitle = subTitle;
		}

		public ITournamentJoinFeedbackView CreateJoinFeedbackView()
		{
			UnityTournamentJoinFeedbackView result = Object.Instantiate<UnityTournamentJoinFeedbackView>(this._joinFeedbackViewPrefab, this._joinFeedbacksContainer);
			this._joinFeedbacksContainerContentSizeFitter.ForceRefresh();
			return result;
		}

		private void Start()
		{
			this._viewProvider.Bind<ITournamentPresenterView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ITournamentPresenterView>(null);
		}

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UnityButton _joinButton;

		[SerializeField]
		private UnityButton _unsubscribeFromTournamentButton;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityButton _teamRulesButton;

		[SerializeField]
		private UnityToggle _teamToggle;

		[SerializeField]
		private UnityToggle _homeToggle;

		[SerializeField]
		private UnityToggle _rankingToggle;

		[SerializeField]
		private UnityLabel _teamToggleLabel;

		[SerializeField]
		private UnityLabel _scheduleToggleLabel;

		[SerializeField]
		private UnityLabel _rankingToggleLabel;

		[SerializeField]
		private UnityLabel _timeLabel;

		[SerializeField]
		private UnityLabel _timeDescriptionLabel;

		[SerializeField]
		private UnityLabel _tournamentTierLabel;

		[SerializeField]
		private UnityLabel _regionLabel;

		[SerializeField]
		private UnityLabel _victoriesCriteriaValueLabel;

		[SerializeField]
		private UnityLabel _competitiveCriteriaValueLabel;

		[SerializeField]
		private UnityLabel _criteriaOperatorLabel;

		[SerializeField]
		private UnityDynamicImage _logoImage;

		[SerializeField]
		private UnityLabel _logoNameLabel;

		[SerializeField]
		private UnityLabel _crossplayActivationIndicatorLabel;

		[SerializeField]
		private UnityUiTitleInfo _titleInfo;

		[SerializeField]
		private RawImage _backgroundRawImage;

		[SerializeField]
		private Animation _windowAnimation;

		[SerializeField]
		private Animation _tabsAnimation;

		[SerializeField]
		private GameObjectActivatable _contextualTimeInfoGameObject;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityAnimation _teamTabShowAnimation;

		[SerializeField]
		private UnityAnimation _teamTabHideAnimation;

		[SerializeField]
		private UnityAnimation _stepsTabShowFromLeftAnimation;

		[SerializeField]
		private UnityAnimation _stepsTabShowFromRightAnimation;

		[SerializeField]
		private UnityAnimation _stepsTabHideToLeftAnimation;

		[SerializeField]
		private UnityAnimation _stepsTabHideToRightAnimation;

		[SerializeField]
		private UnityAnimation _rankingTabShowAnimation;

		[SerializeField]
		private UnityAnimation _rankingTabHideAnimation;

		[SerializeField]
		private Transform _joinFeedbacksContainer;

		[SerializeField]
		private HmmUiContentSizeFitter _joinFeedbacksContainerContentSizeFitter;

		[SerializeField]
		private UnityTournamentJoinFeedbackView _joinFeedbackViewPrefab;

		[SerializeField]
		private Texture _backgroundFinalsTexture;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationSubGroupHolder _leftUiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _leftUiNavigationAxisSelector;

		[SerializeField]
		private UiNavigationSubGroupHolder _homeUiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _homeUiNavigationAxisSelector;

		[SerializeField]
		private UiNavigationSubGroupHolder _rankingUiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _rankingUiNavigationAxisSelector;

		[SerializeField]
		private UiNavigationSubGroupHolder _teamUiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _teamUiNavigationAxisSelector;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
