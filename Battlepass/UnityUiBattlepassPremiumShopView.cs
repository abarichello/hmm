using System;
using System.Collections;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassPremiumShopView : MonoBehaviour, IBattlepassPremiumShopView
	{
		protected void Awake()
		{
			this._battlepassPremiumShopComponent = this._battlepassPremiumShopComponentAsset;
		}

		protected void Start()
		{
			this._battlepassPremiumShopComponent.RegisterPremiumShopWindow(this);
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
				this._shopItems[i].Setup(packages[i]);
				this._shopItems[i].gameObject.SetActive(true);
			}
			for (int j = num; j < this._shopItems.Length; j++)
			{
				this._shopItems[j].gameObject.SetActive(false);
			}
			string formatedText = string.Format("{0}{1} - {2}{3}", new object[]
			{
				"<color=#{0}>",
				Language.Get(this._battlepassSeasonTitleDraft, TranslationSheets.Battlepass),
				Language.Get("BATTLEPASS_ENDS_IN", TranslationSheets.Battlepass),
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
			this._isVisible = isVisible;
			this._mainCanvasGroup.interactable = this._isVisible;
			base.StartCoroutine(this.SetVisibilityCoroutine());
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
	}
}
