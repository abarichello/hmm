using System;
using System.Diagnostics;
using HeavyMetalMachines.Battlepass.BuyLevels;
using HeavyMetalMachines.Frontend;
using UniRx;
using UnityEngine;
using Zenject;

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

		public void SetPremiumButtonVisibility(bool isVisible)
		{
			this._premiumUnlockButtonCanvasGroup.interactable = isVisible;
			this._premiumUnlockButtonCanvasGroup.blocksRaycasts = isVisible;
			this._premiumUnlockButtonCanvasGroup.alpha = ((!isVisible) ? 0f : 1f);
		}

		public void SetupBuyLevels(int price, int targetLevel)
		{
			this._selectedLevelsPrice = price;
			this._targetLevel = targetLevel;
		}

		public void SetAllLevelsButtonHoverValue(int priceValue)
		{
			this._allLevelsPrice = priceValue;
		}

		public float PlayUnlockAnimation()
		{
			this._unlockButtonsAnimation.Play();
			return this._unlockButtonsAnimation.clip.length;
		}

		public void ShowLevelButtons()
		{
			this._buyLevelsCanvasGroup.alpha = 1f;
			this._buyLevelsCanvasGroup.interactable = true;
			this._buyLevelsCanvasGroup.blocksRaycasts = true;
		}

		public void HideLevelButtons()
		{
			this._buyLevelsCanvasGroup.alpha = 0f;
			this._buyLevelsCanvasGroup.interactable = false;
			this._buyLevelsCanvasGroup.blocksRaycasts = false;
		}

		[UnityUiComponentCall]
		public void OnUnlockPremiumButtonClick()
		{
			if (this.UnlockPremiumButtonClickCallback != null)
			{
				this.UnlockPremiumButtonClickCallback();
			}
		}

		private void OnBuyCurrentLevel()
		{
			if (this.BuyLevelsButtonClickCallback != null)
			{
				this.BuyLevelsButtonClickCallback();
			}
		}

		private void OnBuyAllLevels()
		{
			if (this.BuyAllLevelsButtonClickCallback != null)
			{
				this.BuyAllLevelsButtonClickCallback();
			}
		}

		[UnityUiComponentCall]
		public void OnBuyLevelsClick()
		{
			IDisposable buyLevelsPresenterDisposable = ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this._buyLevelsPresenter.Initialize(), delegate(Unit _)
			{
				this._buyLevelsPresenter.SetupLevelValues(this._selectedLevelsPrice, this._targetLevel, this._allLevelsPrice);
				return this._buyLevelsPresenter.Show();
			}));
			IDisposable buyCurrentLevelDisposable = ObservableExtensions.Subscribe<Unit>(this._buyLevelsPresenter.ObserveBuyCurrentLevel(), delegate(Unit _)
			{
				this.OnBuyCurrentLevel();
			});
			IDisposable buyAllLevelsDisposable = ObservableExtensions.Subscribe<Unit>(this._buyLevelsPresenter.ObserveBuyAllLevels(), delegate(Unit _)
			{
				this.OnBuyAllLevels();
			});
			ObservableExtensions.Subscribe<Unit>(this._buyLevelsPresenter.ObserveHide(), delegate(Unit _)
			{
				buyLevelsPresenterDisposable.Dispose();
				buyCurrentLevelDisposable.Dispose();
				buyAllLevelsDisposable.Dispose();
				this._buyLevelsPresenter.Dispose();
			});
		}

		[Header("[Components]")]
		[SerializeField]
		private CanvasGroup _premiumUnlockButtonCanvasGroup;

		[SerializeField]
		private CanvasGroup _buyLevelsCanvasGroup;

		[SerializeField]
		private Animation _unlockButtonsAnimation;

		[Inject]
		private IBattlepassBuyLevelsPresenter _buyLevelsPresenter;

		private int _selectedLevelsPrice;

		private int _targetLevel;

		private int _allLevelsPrice;

		public delegate void SeasonBuyButtonClickDelegate();
	}
}
