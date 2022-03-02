using System;
using System.Collections.Generic;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.UnityUI;
using Hoplon.Input.UiNavigation;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Divisions
{
	public class UnityCompetitiveDivisionsView : MonoBehaviour, ICompetitiveDivisionsView
	{
		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public ILabel CalibrationMatchesPlayedIncompleteLabel
		{
			get
			{
				return this._calibrationMatchesPlayedIncompleteLabel;
			}
		}

		public ILabel CalibrationMatchesPlayedCompletedLabel
		{
			get
			{
				return this._calibrationMatchesPlayedCompletedLabel;
			}
		}

		public ILabel CalibrationTotalMatchesLabel
		{
			get
			{
				return this._calibrationTotalMatchesLabel;
			}
		}

		public ILabel CalibrationDescriptionLabel
		{
			get
			{
				return this._calibrationDescriptionLabel;
			}
		}

		public ILabel SubdivisionsDescriptionLabel
		{
			get
			{
				return this._subdivisionsDescriptionLabel;
			}
		}

		public ILabel DivisionTopRankingDescriptionLabel
		{
			get
			{
				return this._divisionsTopRankingDescriptionLabel;
			}
		}

		public IEnumerable<ICompetitiveDivisionView> DivisionViews
		{
			get
			{
				return this._divisionsViews;
			}
		}

		public IEnumerable<ICompetitiveSubdivisionView> SubdivisionViews
		{
			get
			{
				return this._subdivisionsViews;
			}
		}

		public ICompetitiveDivisionView TopPlacementDivisionView
		{
			get
			{
				return this._topPlacementDivisionView;
			}
		}

		public IActivatable SubdivisionsDescriptionActivatable
		{
			get
			{
				return this._subdivisionsDescriptionActivatable;
			}
		}

		public IActivatable TopPlacementDescriptionActivatable
		{
			get
			{
				return this._topPlacementDescriptionActivatable;
			}
		}

		public IActivatable TopPlacementPositionActivatable
		{
			get
			{
				return this._topPlacementPositionActivatable;
			}
		}

		public ILabel TopPlacementDescriptionLabel
		{
			get
			{
				return this._topPlacementDescriptionLabel;
			}
		}

		public ILabel TopPlacementPositionLabel
		{
			get
			{
				return this._topPlacementPositionLabel;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
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

		private void Awake()
		{
			this._viewProvider.Bind<ICompetitiveDivisionsView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ICompetitiveDivisionsView>(null);
		}

		public void SetTitleInfo(string title, string subTitle, string description)
		{
			this._titleInfo.Setup(title, HmmUiText.TextStyles.UpperCase, subTitle, HmmUiText.TextStyles.UpperCase, description, HmmUiText.TextStyles.UpperCase, false);
		}

		[InjectOnClient]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityLabel _calibrationMatchesPlayedIncompleteLabel;

		[SerializeField]
		private UnityLabel _calibrationMatchesPlayedCompletedLabel;

		[SerializeField]
		private UnityLabel _calibrationTotalMatchesLabel;

		[SerializeField]
		private UnityLabel _calibrationDescriptionLabel;

		[SerializeField]
		private UnityUiTitleInfo _titleInfo;

		[SerializeField]
		private UnityLabel _divisionsTopRankingDescriptionLabel;

		[SerializeField]
		private UnityLabel _subdivisionsDescriptionLabel;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityCompetitiveDivisionView[] _divisionsViews;

		[SerializeField]
		private UnityCompetitiveDivisionView _topPlacementDivisionView;

		[SerializeField]
		private UnityCompetitiveSubdivisionView[] _subdivisionsViews;

		[SerializeField]
		private GameObjectActivatable _subdivisionsDescriptionActivatable;

		[SerializeField]
		private GameObjectActivatable _topPlacementDescriptionActivatable;

		[SerializeField]
		private GameObjectActivatable _topPlacementPositionActivatable;

		[SerializeField]
		private UnityLabel _topPlacementDescriptionLabel;

		[SerializeField]
		private UnityLabel _topPlacementPositionLabel;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
