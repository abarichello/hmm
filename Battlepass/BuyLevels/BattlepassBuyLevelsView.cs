using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Battlepass.BuyLevels
{
	public class BattlepassBuyLevelsView : MonoBehaviour, IBattlepassBuyLevelsView
	{
		public IActivatable WindowActivatable
		{
			get
			{
				return this._windowGameObjectActivatable;
			}
		}

		public IAnimation ShowWindowAnimation
		{
			get
			{
				return this._showWindowanimation;
			}
		}

		public IAnimation HideWindowAnimation
		{
			get
			{
				return this._hideWindowanimation;
			}
		}

		public ILabel TargetLevelLabel
		{
			get
			{
				return this._targetLevelLabel;
			}
		}

		public ILabel SelectedLevelsPriceLabel
		{
			get
			{
				return this._selectedLevelsPriceLabel;
			}
		}

		public IButton PurchaseSelectedButton
		{
			get
			{
				return this._purchaseSelectedButton;
			}
		}

		public ILabel AllLevelsPriceLabel
		{
			get
			{
				return this._allLevelsPriceLabel;
			}
		}

		public IButton PurchaseAllButton
		{
			get
			{
				return this._purchaseAllButton;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
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
			this._viewProvider.Bind<IBattlepassBuyLevelsView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IBattlepassBuyLevelsView>(null);
		}

		[Inject]
		[UsedImplicitly]
		private IViewProvider _viewProvider;

		[SerializeField]
		private GameObjectActivatable _windowGameObjectActivatable;

		[SerializeField]
		private UnityAnimation _showWindowanimation;

		[SerializeField]
		private UnityAnimation _hideWindowanimation;

		[SerializeField]
		private UnityLabel _targetLevelLabel;

		[SerializeField]
		private UnityLabel _selectedLevelsPriceLabel;

		[SerializeField]
		private UnityButton _purchaseSelectedButton;

		[SerializeField]
		private UnityLabel _allLevelsPriceLabel;

		[SerializeField]
		private UnityButton _purchaseAllButton;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
