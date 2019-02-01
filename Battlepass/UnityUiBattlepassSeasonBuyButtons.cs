using System;
using System.Diagnostics;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.UnityUI;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassSeasonBuyButtons : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate UnlockPremiumButtonClickCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate BuyLevelsButtonClickCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate BuyAllLevelsButtonClickCallback;

		public void AddBuyCallbacks(UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate unlockPremiumButtonClickCallback, UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate buyLevelsButtonClickCallback, UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate buyAllLevelsButtonClickCallback)
		{
			this.UnlockPremiumButtonClickCallback += unlockPremiumButtonClickCallback;
			this.BuyLevelsButtonClickCallback += buyLevelsButtonClickCallback;
			this.BuyAllLevelsButtonClickCallback += buyAllLevelsButtonClickCallback;
		}

		public void RemoveBuyCallbacks(UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate unlockPremiumButtonClickCallback, UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate buyLevelsButtonClickCallback, UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate buyAllLevelsButtonClickCallback)
		{
			this.UnlockPremiumButtonClickCallback -= unlockPremiumButtonClickCallback;
			this.BuyLevelsButtonClickCallback -= buyLevelsButtonClickCallback;
			this.BuyAllLevelsButtonClickCallback -= buyAllLevelsButtonClickCallback;
		}

		public void SetPremiumButtonInteractable(bool isInteractable)
		{
			this._unlockPremiumButton.interactable = isInteractable;
		}

		public void SetupBuyLevels(string levelsText)
		{
			this._levelsButtonHoverValue.Setup(levelsText);
			this._premiumBuyLevelButtonCanvasGroup.alpha = 0f;
			this._premiumBuyLevelButtonCanvasGroup.interactable = false;
		}

		public float PlayUnlockAnimation()
		{
			this._unlockButtonsAnimation.Play();
			return this._unlockButtonsAnimation.clip.length;
		}

		public void SetAllLevelsButtonHoverValue(string priceValueText)
		{
			this._allLevelsButtonHoverValue.Setup(priceValueText);
		}

		public void SetupByPremiumState(bool userHasPremium, int currentLevel, int maxSlots)
		{
			this._premiumUnlockButtonCanvasGroup.alpha = ((!userHasPremium) ? 1f : 0f);
			this._premiumUnlockButtonCanvasGroup.interactable = !userHasPremium;
			this._premiumUnlockButtonCanvasGroup.blocksRaycasts = !userHasPremium;
			bool flag = currentLevel != maxSlots - 1 && userHasPremium;
			this._premiumLevelButtonsCanvasGroup.alpha = ((!flag) ? 0f : 1f);
			this._premiumLevelButtonsCanvasGroup.interactable = flag;
			this._premiumLevelButtonsCanvasGroup.blocksRaycasts = flag;
			this._buyAllLevelsButton.interactable = true;
			this._buyLevelsButton.interactable = true;
		}

		public void UpdateBuyLevelButton(int selectedLevel, int currentLevel, int buyLevelPriceValue)
		{
			int num = (selectedLevel > currentLevel) ? (selectedLevel - currentLevel) : 1;
			int num2 = buyLevelPriceValue * num;
			this._levelsButtonHoverValue.Setup(num2.ToString("0"));
			this._premiumBuyLevelButtonText.text = string.Format(Language.Get("BATTLEPASS_PURCHASE_LEVELS", TranslationSheets.Battlepass), num);
			this._premiumBuyLevelButtonCanvasGroup.alpha = 1f;
			this._premiumBuyLevelButtonCanvasGroup.interactable = true;
			this._buyLevelsButton.interactable = true;
		}

		public void DisableButtonsCanvas()
		{
			this._premiumBuyAllLevelsButtonCanvasGroup.alpha = 0f;
			this._premiumBuyLevelButtonCanvasGroup.alpha = 0f;
		}

		public void EnableLevelButtons()
		{
			this._premiumLevelButtonsCanvasGroup.alpha = 1f;
			this._premiumLevelButtonsCanvasGroup.interactable = true;
			this._buyAllLevelsButton.interactable = true;
		}

		public void DisableLevelButtons()
		{
			this._premiumLevelButtonsCanvasGroup.interactable = false;
			this._premiumBuyLevelButtonCanvasGroup.alpha = 0f;
			this._premiumBuyLevelButtonCanvasGroup.interactable = false;
			this._buyAllLevelsButton.interactable = false;
		}

		[UnityUiComponentCall]
		public void OnUnlockPremiumButtonClick()
		{
			if (this.UnlockPremiumButtonClickCallback != null)
			{
				this.UnlockPremiumButtonClickCallback();
			}
		}

		[UnityUiComponentCall]
		public void OnBuyLevelsButtonClick()
		{
			if (this.BuyLevelsButtonClickCallback != null)
			{
				this.BuyLevelsButtonClickCallback();
			}
		}

		[UnityUiComponentCall]
		public void OnBuyAllLevelsButtonClick()
		{
			if (this.BuyAllLevelsButtonClickCallback != null)
			{
				this.BuyAllLevelsButtonClickCallback();
			}
		}

		[Header("[Components]")]
		[SerializeField]
		private CanvasGroup _premiumUnlockButtonCanvasGroup;

		[SerializeField]
		private CanvasGroup _premiumLevelButtonsCanvasGroup;

		[SerializeField]
		private CanvasGroup _premiumBuyLevelButtonCanvasGroup;

		[SerializeField]
		private CanvasGroup _premiumBuyAllLevelsButtonCanvasGroup;

		[SerializeField]
		private Text _premiumBuyLevelButtonText;

		[SerializeField]
		private Animation _unlockButtonsAnimation;

		[SerializeField]
		private Button _unlockPremiumButton;

		[SerializeField]
		private Button _buyLevelsButton;

		[SerializeField]
		private Button _buyAllLevelsButton;

		[Header("[Views]")]
		[SerializeField]
		private UnityUiButtonHoverValue _levelsButtonHoverValue;

		[SerializeField]
		private UnityUiButtonHoverValue _allLevelsButtonHoverValue;

		public delegate void SeasonBuyButtonClickDelegate();
	}
}
