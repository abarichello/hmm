using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.UnityUI;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public class UnityCompetitiveRewardsView : MonoBehaviour, ICompetitiveRewardsView
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

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public ILabel DivisionNameLabel
		{
			get
			{
				return this._divisionNameLabel;
			}
		}

		public ILabel DivisionScoreIntervalLabel
		{
			get
			{
				return this._divisionScoreIntervalLabel;
			}
		}

		public IButton PreviousDivisionButton
		{
			get
			{
				return this._previousDivisionButton;
			}
		}

		public IButton NextDivisionButton
		{
			get
			{
				return this._nextDivisionButton;
			}
		}

		public IAnimation DivisionShowAnimation
		{
			get
			{
				return this._divisionShowAnimation;
			}
		}

		public IAnimation DivisionHideAnimation
		{
			get
			{
				return this._divisionHideAnimation;
			}
		}

		public IItemPreviewer ItemPreviewer
		{
			get
			{
				return this._unityItemPreview;
			}
		}

		public IActivatable ItemPreviewerActivatable
		{
			get
			{
				return new GameObjectActivatable(this._unityItemPreview.gameObject);
			}
		}

		public IActivatable[] DivisionsPreviews
		{
			get
			{
				return this._divisionsPreviews;
			}
		}

		public IActivatable TopPlacementRewardsObservation
		{
			get
			{
				return this._topPlacementRewardsObservation;
			}
		}

		public IDynamicImage TemporaryPreviewImage
		{
			get
			{
				return this._temporaryPreviewImage;
			}
		}

		public string[] TemporaryDivisionsPreviewsImageNames
		{
			get
			{
				return this._temporaryDivisionsPreviewsImageNames;
			}
		}

		public ILabel TemporaryPrizeListLabel
		{
			get
			{
				return this._temporaryPrizeListLabel;
			}
		}

		public IAnimation TemporaryPreviewShowAnimation
		{
			get
			{
				return this._temporaryPreviewShowAnimation;
			}
		}

		public IAnimation TemporaryPreviewHideAnimation
		{
			get
			{
				return this._temporaryPreviewHideAnimation;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public ICompetitiveRewardItemView CreateAndAddItem()
		{
			UnityCompetitiveRewardItemView unityCompetitiveRewardItemView = Object.Instantiate<UnityCompetitiveRewardItemView>(this._itemViewPrefab, this._itemsParent, false);
			((UnityToggle)unityCompetitiveRewardItemView.Toggle).Toggle.group = this._itemsToggleGroup;
			return unityCompetitiveRewardItemView;
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICompetitiveRewardsView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveRewardsView>(null);
		}

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityUiTitleInfo _title;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityLabel _divisionNameLabel;

		[SerializeField]
		private UnityLabel _divisionScoreIntervalLabel;

		[SerializeField]
		private UnityButton _previousDivisionButton;

		[SerializeField]
		private UnityButton _nextDivisionButton;

		[SerializeField]
		private UnityAnimation _divisionShowAnimation;

		[SerializeField]
		private UnityAnimation _divisionHideAnimation;

		[SerializeField]
		private UnityCompetitiveRewardItemView _itemViewPrefab;

		[SerializeField]
		private Transform _itemsParent;

		[SerializeField]
		private ToggleGroup _itemsToggleGroup;

		[SerializeField]
		private UnityUiBattlepassArtPreview _unityItemPreview;

		[SerializeField]
		private GameObjectActivatable[] _divisionsPreviews;

		[SerializeField]
		private GameObjectActivatable _topPlacementRewardsObservation;

		[SerializeField]
		private UnityDynamicImage _temporaryPreviewImage;

		[SerializeField]
		private string[] _temporaryDivisionsPreviewsImageNames;

		[SerializeField]
		private UnityLabel _temporaryPrizeListLabel;

		[SerializeField]
		private UnityAnimation _temporaryPreviewShowAnimation;

		[SerializeField]
		private UnityAnimation _temporaryPreviewHideAnimation;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
