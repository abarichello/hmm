using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassSeasonCellView : EnhancedScrollerCellView
	{
		public void Setup(IBattlepassSeasonScroller battlepassSeasonScroller, UnityUiBattlepassSeasonCellView.SeasonCellViewData cellViewData, UnityUiBattlepassSeasonCellView.OnSlotSelected onSlotSelected, UnityUiBattlepassSeasonCellView.OnSlotUpdateSelector onSlotUpdateSelector, ref UnityUiBattlepassSeasonScroller.SlotAnimationTriggerDelegate onSlotAnimationTrigger, ref UnityUiBattlepassSeasonScroller.SlotTransformRequestDelegate onSlotTransformRequest)
		{
			onSlotAnimationTrigger = (UnityUiBattlepassSeasonScroller.SlotAnimationTriggerDelegate)Delegate.Remove(onSlotAnimationTrigger, new UnityUiBattlepassSeasonScroller.SlotAnimationTriggerDelegate(this.OnSlotAnimationTrigger));
			onSlotAnimationTrigger = (UnityUiBattlepassSeasonScroller.SlotAnimationTriggerDelegate)Delegate.Combine(onSlotAnimationTrigger, new UnityUiBattlepassSeasonScroller.SlotAnimationTriggerDelegate(this.OnSlotAnimationTrigger));
			onSlotTransformRequest = (UnityUiBattlepassSeasonScroller.SlotTransformRequestDelegate)Delegate.Remove(onSlotTransformRequest, new UnityUiBattlepassSeasonScroller.SlotTransformRequestDelegate(this.OnSlotTransformRequest));
			onSlotTransformRequest = (UnityUiBattlepassSeasonScroller.SlotTransformRequestDelegate)Delegate.Combine(onSlotTransformRequest, new UnityUiBattlepassSeasonScroller.SlotTransformRequestDelegate(this.OnSlotTransformRequest));
			this._battlepassSeasonScroller = battlepassSeasonScroller;
			this._slotLevel = cellViewData.Level;
			this._freeSlotHasReward = cellViewData.FreeSlotData.SlotHasReward;
			this._premiumSlotHasReward = cellViewData.PremiumSlotData.SlotHasReward;
			this._onSlotSelected = onSlotSelected;
			this._onSlotUpdateSelector = onSlotUpdateSelector;
			this.InternalSetup(cellViewData);
		}

		protected void OnEnable()
		{
			GUIUtils.AnimationSetLastFrame(this._freeSlotGui.UnlockAnimation);
			GUIUtils.AnimationSetLastFrame(this._premiumSlotGui.UnlockAnimation);
			GUIUtils.AnimationSetLastFrame(this._level.UnlockAnimation);
		}

		private void OnSlotAnimationTrigger(int slotLevel, UnityUiBattlepassSeasonCellView.SeasonCellViewData cellViewData)
		{
			if (slotLevel == this._slotLevel)
			{
				this.PlayUnlockAnimation(this._freeSlotHasReward, this._premiumSlotHasReward);
				this.InternalSetup(cellViewData);
			}
		}

		private void OnSlotTransformRequest(int slotLevel)
		{
			if (slotLevel == this._slotLevel)
			{
				this._onSlotUpdateSelector(slotLevel, base.transform);
			}
		}

		private void InternalSetup(UnityUiBattlepassSeasonCellView.SeasonCellViewData cellViewData)
		{
			bool flag = this._battlepassSeasonScroller.IsSlotSelectionOutCorner(this._slotLevel);
			this._level.LevelText.text = (this._slotLevel + 1).ToString("0");
			this._selectorCanvasGroup.alpha = ((!cellViewData.IsCurrentLevel || flag) ? 0f : 1f);
			UnityUiBattlepassSeasonCellView.SeasonCellViewLevelInfo seasonCellViewLevelInfo = this._level.LevelInfos[(int)cellViewData.LevelType];
			this._level.LevelText.color = seasonCellViewLevelInfo.TextColor;
			this._level.LevelRawImage.texture = seasonCellViewLevelInfo.BgTexture;
			this._level.LevelOutline.enabled = seasonCellViewLevelInfo.hasOutline;
			this._level.LevelText.fontSize = seasonCellViewLevelInfo.FontSize;
			this.FillBorder(this._borderFree, this._freeSlotGui, cellViewData.FreeSlotData);
			this.FillBorder(this._borderPremium, this._premiumSlotGui, cellViewData.PremiumSlotData);
			this.FillLockInfo(false, this._freeSlotGui, cellViewData.FreeSlotData);
			this.FillLockInfo(true, this._premiumSlotGui, cellViewData.FreeSlotData);
			if (!cellViewData.FreeSlotData.SlotHasReward)
			{
				this.MakeSlotEmpty(false, this._freeSlotGui);
			}
			else
			{
				this.FillDataSlot(this._freeSlotGui, cellViewData.FreeSlotData, flag);
			}
			if (!cellViewData.PremiumSlotData.SlotHasReward)
			{
				this.MakeSlotEmpty(true, this._premiumSlotGui);
			}
			else
			{
				this.FillDataSlot(this._premiumSlotGui, cellViewData.PremiumSlotData, flag);
			}
		}

		private void FillBorder(UnityUiBattlepassSeasonCellView.SeasonCellViewBorderInfo borderInfo, UnityUiBattlepassSeasonCellView.SeasonCellViewSlotGui slotGui, UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData slotData)
		{
			slotGui.BorderImage.sprite = ((!slotData.IsLocked) ? borderInfo.UnlockedSprite : borderInfo.LockedSprite);
			slotGui.BorderImage.color = ((!slotData.IsLocked) ? borderInfo.UnlockedColor : borderInfo.LockedColor);
			slotGui.SlotButton.interactable = (slotData.SlotHasReward && !slotData.IsSelected);
		}

		private void FillLockInfo(bool isPremium, UnityUiBattlepassSeasonCellView.SeasonCellViewSlotGui slotGui, UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData freeSlotData)
		{
			if (isPremium)
			{
				slotGui.LockRawImage.texture = ((!freeSlotData.IsLocked) ? this._locks.PremiumUnlockableTexture : this._locks.PremiumLockedTexture);
			}
			else
			{
				slotGui.LockRawImage.texture = this._locks.FreeTexture;
			}
		}

		private void FillDataSlot(UnityUiBattlepassSeasonCellView.SeasonCellViewSlotGui slotGui, UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData slotData, bool isSlotSelectionOutCorner)
		{
			slotGui.IconRawImage.TryToLoadAsset(slotData.IconAssetName);
			bool flag = slotData.IsLocked && slotData.IsRepeated;
			slotGui.RepeatedIconCanvasGroup.alpha = ((!flag) ? 0f : 1f);
			CanvasGroup repeatedIconCanvasGroup = slotGui.RepeatedIconCanvasGroup;
			bool flag2 = flag;
			slotGui.RepeatedIconCanvasGroup.blocksRaycasts = flag2;
			repeatedIconCanvasGroup.interactable = flag2;
			slotGui.FameCanvasGroup.alpha = ((slotData.CurrencyAmount <= 0) ? 0f : 1f);
			if (slotData.CurrencyAmount > 0)
			{
				slotGui.FameText.text = slotData.CurrencyAmount.ToString("0");
				slotGui.FameText.color = ((!slotData.IsHardCurrency) ? this._softCurrencyColor : this._hardCurrencyColor);
				slotGui.FameTextOutline.effectColor = ((!slotData.IsHardCurrency) ? this._softCurrencyOutlineColor : this._hardCurrencyOutlineColor);
				slotGui.FameText.SetAlpha((!slotData.IsLocked) ? slotGui.EnabledFameAlpha : slotGui.DisabledFameAlpha);
			}
			slotGui.LockCanvasGroup.alpha = ((!slotData.IsLocked) ? 0f : 1f);
			slotGui.IconCanvasGroup.alpha = 1f;
			slotGui.SelectionBorderCanvasGroup.alpha = ((!slotData.IsSelected || isSlotSelectionOutCorner) ? 0f : 1f);
			if (slotData.IsSelected)
			{
				slotGui.SlotButton.colors = this._selectedButtonColorBlock;
			}
			else
			{
				slotGui.SlotButton.colors = ((!slotData.IsLocked) ? this._unlockedButtonColorBlock : this._lockedButtonColorBlock);
			}
		}

		private void MakeSlotEmpty(bool isPremium, UnityUiBattlepassSeasonCellView.SeasonCellViewSlotGui slotGui)
		{
			slotGui.IconRawImage.ClearAsset();
			slotGui.IconCanvasGroup.alpha = 0f;
			slotGui.SelectionBorderCanvasGroup.alpha = 0f;
			slotGui.LockCanvasGroup.alpha = 0f;
			slotGui.FameCanvasGroup.alpha = 0f;
			slotGui.BorderImage.color = this._emptySlotBorderColor;
			slotGui.BorderImage.sprite = ((!isPremium) ? this._borderFree.UnlockedSprite : this._borderPremium.UnlockedSprite);
			slotGui.RepeatedIconCanvasGroup.alpha = 0f;
			CanvasGroup repeatedIconCanvasGroup = slotGui.RepeatedIconCanvasGroup;
			bool flag = false;
			slotGui.RepeatedIconCanvasGroup.blocksRaycasts = flag;
			repeatedIconCanvasGroup.interactable = flag;
		}

		[UnityUiComponentCall]
		public void OnFreeSlotClick()
		{
			this._onSlotSelected(false, this._slotLevel);
		}

		[UnityUiComponentCall]
		public void OnPremiumSlotClick()
		{
			this._onSlotSelected(true, this._slotLevel);
		}

		public override void RefreshCellView()
		{
			base.RefreshCellView();
			this.InternalSetup(this._battlepassSeasonScroller.GetSeasonCellViewData(this._slotLevel));
		}

		private void PlayUnlockAnimation(bool showFreeSlotAnimation, bool showPremiumSlotAnimation)
		{
			if (showFreeSlotAnimation)
			{
				GUIUtils.PlayAnimation(this._freeSlotGui.UnlockAnimation, false, 1f, string.Empty);
			}
			if (showPremiumSlotAnimation)
			{
				GUIUtils.PlayAnimation(this._premiumSlotGui.UnlockAnimation, false, 1f, string.Empty);
			}
			GUIUtils.PlayAnimation(this._level.UnlockAnimation, false, 1f, string.Empty);
		}

		[Header("[Components]")]
		[SerializeField]
		private CanvasGroup _selectorCanvasGroup;

		[Header("[Views]")]
		[SerializeField]
		private UnityUiBattlepassSeasonCellView.SeasonCellViewSlotGui _freeSlotGui;

		[SerializeField]
		private UnityUiBattlepassSeasonCellView.SeasonCellViewSlotGui _premiumSlotGui;

		[SerializeField]
		private UnityUiBattlepassSeasonCellView.SeasonCellViewBorderInfo _borderFree;

		[SerializeField]
		private UnityUiBattlepassSeasonCellView.SeasonCellViewBorderInfo _borderPremium;

		[SerializeField]
		private UnityUiBattlepassSeasonCellView.SeasonCellViewLevel _level;

		[SerializeField]
		private UnityUiBattlepassSeasonCellView.SeasonCellViewLocks _locks;

		[Header("[Configs]")]
		[SerializeField]
		private Color _softCurrencyColor;

		[SerializeField]
		private Color _softCurrencyOutlineColor;

		[SerializeField]
		private Color _hardCurrencyColor;

		[SerializeField]
		private Color _hardCurrencyOutlineColor;

		[SerializeField]
		private Color _emptySlotBorderColor;

		[Header("[Locked Button Colors]")]
		[SerializeField]
		private ColorBlock _lockedButtonColorBlock;

		[Header("[Unlocked Button Colors]")]
		[SerializeField]
		private ColorBlock _unlockedButtonColorBlock;

		[Header("[Selected Button Colors]")]
		[SerializeField]
		private ColorBlock _selectedButtonColorBlock;

		private UnityUiBattlepassSeasonCellView.OnSlotSelected _onSlotSelected;

		private UnityUiBattlepassSeasonCellView.OnSlotUpdateSelector _onSlotUpdateSelector;

		private int _slotLevel;

		private bool _freeSlotHasReward;

		private bool _premiumSlotHasReward;

		private IBattlepassSeasonScroller _battlepassSeasonScroller;

		public struct SeasonCellViewSlotData
		{
			public bool SlotHasReward;

			public string IconAssetName;

			public int CurrencyAmount;

			public bool IsHardCurrency;

			public bool IsLocked;

			public bool IsSelected;

			public bool IsRepeated;
		}

		public struct SeasonCellViewData
		{
			public int Level;

			public bool IsCurrentLevel;

			public UnityUiBattlepassSeasonCellView.LevelType LevelType;

			public UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData FreeSlotData;

			public UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData PremiumSlotData;
		}

		public delegate bool OnSlotSelected(bool isPremium, int slotLevel);

		public delegate void OnSlotUpdateSelector(int slotLevel, Transform slotTransform);

		[Serializable]
		private struct SeasonCellViewSlotGui
		{
			[Header("[Configs]")]
			[Range(0f, 1f)]
			public float EnabledFameAlpha;

			[Range(0f, 1f)]
			public float DisabledFameAlpha;

			[Header("[UI Components]")]
			public Image BorderImage;

			public Button SlotButton;

			public HmmUiRawImage IconRawImage;

			public CanvasGroup IconCanvasGroup;

			public CanvasGroup LockCanvasGroup;

			public RawImage LockRawImage;

			public CanvasGroup FameCanvasGroup;

			public Text FameText;

			public Outline FameTextOutline;

			public CanvasGroup SelectionBorderCanvasGroup;

			public Animation UnlockAnimation;

			public CanvasGroup RepeatedIconCanvasGroup;
		}

		[Serializable]
		private struct SeasonCellViewBorderInfo
		{
			public Color LockedColor;

			public Sprite LockedSprite;

			public Color UnlockedColor;

			public Sprite UnlockedSprite;
		}

		[Serializable]
		private struct SeasonCellViewLevelInfo
		{
			public string LevelType;

			public Color TextColor;

			public int FontSize;

			public bool hasOutline;

			public Texture BgTexture;
		}

		[Serializable]
		private struct SeasonCellViewLevel
		{
			[Header("[Configs]")]
			public UnityUiBattlepassSeasonCellView.SeasonCellViewLevelInfo[] LevelInfos;

			[Header("[UI Components]")]
			public RawImage LevelRawImage;

			public Text LevelText;

			public Outline LevelOutline;

			public Animation UnlockAnimation;
		}

		public enum LevelType
		{
			Unlocked,
			Current,
			Locked
		}

		[Serializable]
		private struct SeasonCellViewLocks
		{
			public Texture FreeTexture;

			public Texture PremiumLockedTexture;

			public Texture PremiumUnlockableTexture;
		}
	}
}
