using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.UnityUI;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public class UnityCompetitiveRankingView : MonoBehaviour, ICompetitiveRankingView
	{
		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public ITitle Title
		{
			get
			{
				return this._title;
			}
		}

		public IToggle ShowGlobalToggle
		{
			get
			{
				return this._showGlobalToggle;
			}
		}

		public IToggle ShowFriendsToggle
		{
			get
			{
				return this._showFriendsToggle;
			}
		}

		public ILabel ShowGlobalToggleLabel
		{
			get
			{
				return this._showGlobalToggleLabel;
			}
		}

		public ILabel ShowFriendsToggleLabel
		{
			get
			{
				return this._showFriendsToggleLabel;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IAnimation FadeInAnimation
		{
			get
			{
				return this._fadeInAnimation;
			}
		}

		public IAnimation FadeOutAnimation
		{
			get
			{
				return this._fadeOutAnimation;
			}
		}

		public IActivatable CalibratingStateGroup
		{
			get
			{
				return this._calibratingStateGroup;
			}
		}

		public ILabel CalibrationTotalMatchesLabel
		{
			get
			{
				return this._calibrationTotalMatchesLabel;
			}
		}

		public ILabel CalibrationMatchesPlayedLabel
		{
			get
			{
				return this._calibrationMatchesPlayedIncompleteLabel;
			}
		}

		public IActivatable DivisionGroup
		{
			get
			{
				return this._divisionGroup;
			}
		}

		public ILabel DivisionNameLabel
		{
			get
			{
				return this._divisionNameLabel;
			}
		}

		public ILabel RankScoreLabel
		{
			get
			{
				return this._rankScoreLabel;
			}
		}

		public ILabel TopPositionLabel
		{
			get
			{
				return this._topPositionLabel;
			}
		}

		public IEnumerable<IRankBadgeComponents> RanksBadgesComponents
		{
			get
			{
				return this._rankBadgesComponents;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<ICompetitiveRankingView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ICompetitiveRankingView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityUiTitleInfo _title;

		[SerializeField]
		private UnityToggle _showGlobalToggle;

		[SerializeField]
		private UnityToggle _showFriendsToggle;

		[SerializeField]
		private UnityLabel _showGlobalToggleLabel;

		[SerializeField]
		private UnityLabel _showFriendsToggleLabel;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityAnimation _fadeInAnimation;

		[SerializeField]
		private UnityAnimation _fadeOutAnimation;

		[SerializeField]
		private GameObjectActivatable _calibratingStateGroup;

		[SerializeField]
		private UnityLabel _calibrationTotalMatchesLabel;

		[SerializeField]
		private UnityLabel _calibrationMatchesPlayedIncompleteLabel;

		[SerializeField]
		private GameObjectActivatable _divisionGroup;

		[SerializeField]
		private UnityLabel _divisionNameLabel;

		[SerializeField]
		private UnityLabel _rankScoreLabel;

		[SerializeField]
		private UnityLabel _topPositionLabel;

		[SerializeField]
		private UnityRankBadgeComponents[] _rankBadgesComponents;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
