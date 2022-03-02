using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public class UnityCompetitiveSeasonRewardsCollectionView : MonoBehaviour, ICompetitiveSeasonRewardsCollectionView
	{
		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public ILabel SeasonNameLabel
		{
			get
			{
				return this._seasonNameLabel;
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

		public IActivatable[] DivisionGroups
		{
			get
			{
				return this._divisionGroups;
			}
		}

		public ILabel DivisionNameLabel
		{
			get
			{
				return this._divisionNameLabel;
			}
		}

		public ILabel ScoreLabel
		{
			get
			{
				return this._scoreLabel;
			}
		}

		public IButton ConfirmButton
		{
			get
			{
				return this._confirmButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IDynamicImage[] DivisionImages
		{
			get
			{
				return this._divisionImages;
			}
		}

		public ICompetitiveSeasonRewardCollectionItemView CreateAndAddItem()
		{
			return Object.Instantiate<UnityCompetitiveSeasonRewardCollectionItemView>(this._rewardItemViewPrefab, this._itemsParent);
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICompetitiveSeasonRewardsCollectionView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveSeasonRewardsCollectionView>(null);
		}

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityLabel _seasonNameLabel;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private GameObjectActivatable[] _divisionGroups;

		[SerializeField]
		private UnityLabel _divisionNameLabel;

		[SerializeField]
		private UnityLabel _scoreLabel;

		[SerializeField]
		private UnityButton _confirmButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UnityCompetitiveSeasonRewardCollectionItemView _rewardItemViewPrefab;

		[SerializeField]
		private Transform _itemsParent;

		[SerializeField]
		private UnityDynamicImage[] _divisionImages;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
