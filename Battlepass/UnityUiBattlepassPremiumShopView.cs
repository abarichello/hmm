using System;
using System.Collections;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassPremiumShopView : MonoBehaviour, IBattlepassPremiumShopView
	{
		public string BattlepassSeasonTitleDraft
		{
			get
			{
				return this._battlepassSeasonTitleDraft;
			}
			set
			{
				this._battlepassSeasonTitleDraft = value;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		protected void Awake()
		{
			this._battlepassPremiumShopComponent = this._battlepassPremiumShopComponentAsset;
		}

		protected void Start()
		{
			IGetBattlepassSeason getBattlepassSeason = this._diContainer.Resolve<IGetBattlepassSeason>();
			BattlepassSeason battlepassSeason = getBattlepassSeason.Get();
			this._battlepassPremiumShopComponent.RegisterPremiumShopWindow(this, battlepassSeason);
		}

		protected void OnDisable()
		{
			this.SetVisibility(false);
			base.StopAllCoroutines();
			this._isAnimating = false;
			if (!this._isVisible && this._battlepassPremiumShopComponent.IsPremiumShopSceneLoad())
			{
				this._battlepassPremiumShopComponent.OnHidePremiumShopWindowAnimationEnded();
			}
		}

		public void Setup(ItemTypeScriptableObject[] packages, TimeSpan remainingTime)
		{
			int num = this._shopItems.Length;
			if (packages.Length != num)
			{
				UnityUiBattlepassPremiumShopView.Log.WarnFormat("Package count differs from components configured. {0} packages; {1} components", new object[]
				{
					packages.Length,
					num
				});
				num = Math.Min(packages.Length, num);
			}
			for (int i = 0; i < num; i++)
			{
				UnityUiBattlepassPremiumShopItem shopItem = this._shopItems[i];
				shopItem.Setup(packages[i]);
				shopItem.IsPurchasableChanged += delegate(bool isActiveInStore)
				{
					shopItem.gameObject.SetActive(isActiveInStore);
				};
				shopItem.gameObject.SetActive(shopItem.IsPurchasable);
			}
			for (int j = num; j < this._shopItems.Length; j++)
			{
				this._shopItems[j].gameObject.SetActive(false);
			}
			string formatedText = string.Format("{0}{1} - {2}{3}", new object[]
			{
				"<color=#{0}>",
				Language.Get(this._battlepassSeasonTitleDraft, TranslationContext.Battlepass),
				Language.Get("BATTLEPASS_ENDS_IN", TranslationContext.Battlepass),
				"</color> {1}"
			});
			this._timerInfo.Setup(remainingTime, new TimeSpan(1, 0, 0, 0), formatedText);
		}

		public void SetVisibility(bool isVisible)
		{
			if (this._isAnimating || this._isVisible == isVisible)
			{
				return;
			}
			this.SetUiNavigationFocus(isVisible);
			this._isVisible = isVisible;
			this._mainCanvasGroup.interactable = this._isVisible;
			if (base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(this.SetVisibilityCoroutine());
			}
			else
			{
				UnityUiBattlepassPremiumShopView.Log.WarnFormat("SetVisibility({0}) called when gameObject.activeInHierarchy false", new object[]
				{
					isVisible
				});
			}
		}

		private void SetUiNavigationFocus(bool focused)
		{
			if (focused)
			{
				this.UiNavigationGroupHolder.AddGroup();
			}
			else
			{
				this.UiNavigationGroupHolder.RemoveGroup();
			}
		}

		private IEnumerator SetVisibilityCoroutine()
		{
			this._isAnimating = true;
			GUIUtils.PlayAnimation(this._mainWindowAnimation, !this._isVisible, 1f, string.Empty);
			yield return new WaitForSeconds(this._mainWindowAnimation.clip.length);
			if (!this._isVisible)
			{
				this._battlepassPremiumShopComponent.OnHidePremiumShopWindowAnimationEnded();
			}
			this._isAnimating = false;
			yield break;
		}

		public bool IsVisible()
		{
			return this._isVisible;
		}

		public void SetInteractability(bool isInteractable)
		{
			this._mainCanvasGroup.interactable = isInteractable;
		}

		[UnityUiComponentCall]
		public void OnBuyPremiumClick(int packageIndex)
		{
			this._battlepassPremiumShopComponent.OnBuyPremiumRequested(packageIndex);
		}

		[UnityUiComponentCall]
		public void OnBackButtonClick()
		{
			this._battlepassPremiumShopComponent.HidePremiumShopWindow();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(UnityUiBattlepassPremiumShopView));

		[SerializeField]
		private BattlepassPremiumShopComponent _battlepassPremiumShopComponentAsset;

		private IBattlepassPremiumShopComponent _battlepassPremiumShopComponent;

		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[SerializeField]
		private Animation _mainWindowAnimation;

		[SerializeField]
		private GameObject _bundlePackageGameObject;

		[SerializeField]
		private UnityUiBattlepassPremiumShopItem[] _shopItems;

		[SerializeField]
		private UnityUiTimerInfo _timerInfo;

		private bool _isVisible;

		private bool _isAnimating;

		[SerializeField]
		private string _battlepassSeasonTitleDraft;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[InjectOnClient]
		private DiContainer _diContainer;
	}
}
