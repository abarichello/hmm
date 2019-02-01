using System;
using Assets.ClientApiObjects;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Obsolete]
	public class StoreItem : CharactersItem
	{
		public void ToggleScroll(bool isEnabled)
		{
			UIDragScrollView uidragScrollView;
			if ((uidragScrollView = this._scrollView) == null)
			{
				uidragScrollView = (this._scrollView = this.button.GetComponent<UIDragScrollView>());
			}
			UIDragScrollView uidragScrollView2 = uidragScrollView;
			if (uidragScrollView2 != null)
			{
				uidragScrollView2.enabled = isEnabled;
			}
		}

		public void SetLocked(bool canBuySkin, bool isUnlockSeen, int level)
		{
			this._isReadyToOpenLock = false;
			this._canBuySkin = canBuySkin;
			this._isUnlockSeen = isUnlockSeen;
			if (this.UnlockGroupComponents.LevelLabel == null)
			{
				return;
			}
			this.UpdateUnlockGroupVisibility(canBuySkin, isUnlockSeen);
			this.UnlockGroupComponents.LevelLabel.text = (level + 1).ToString("0");
			this._isReadyToOpenLock = (canBuySkin && !isUnlockSeen);
			if (this._isReadyToOpenLock)
			{
				this.UnlockGroupComponents.UnlockSkinAnimation.Play();
			}
		}

		private void UpdateUnlockGroupVisibility(bool canBuySkin, bool isUnlockSeen)
		{
			bool active = !canBuySkin || !isUnlockSeen;
			this.UnlockGroupComponents.IconSprite.gameObject.SetActive(active);
			this.UnlockGroupComponents.LevelLabel.gameObject.SetActive(active);
			this.UnlockGroupComponents.ExplosionIconSprite.gameObject.SetActive(false);
		}

		public bool IsReadyToOpenLock()
		{
			return this._isReadyToOpenLock;
		}

		public void UnlockAnimation()
		{
			this.UnlockGroupComponents.IconSprite.gameObject.SetActive(false);
			this.UnlockGroupComponents.IconSpriteAura.gameObject.SetActive(false);
			this.UnlockGroupComponents.LevelLabel.gameObject.SetActive(false);
			this.UnlockGroupComponents.ExplosionIconSprite.gameObject.SetActive(true);
			this.UnlockGroupComponents.UnlockSkinAnimation.Stop();
			this.UnlockGroupComponents.ExplosionAnimation.Play();
			this._isReadyToOpenLock = false;
			this._canBuySkin = true;
			this._isUnlockSeen = true;
		}

		protected void OnEnable()
		{
			if (this.UnlockGroupComponents.IconSprite == null)
			{
				return;
			}
			this.UpdateUnlockGroupVisibility(this._canBuySkin, this._isUnlockSeen);
			if (this._isReadyToOpenLock)
			{
				this.UnlockGroupComponents.UnlockSkinAnimation.Play();
			}
		}

		protected void OnDisable()
		{
			if (this.UnlockGroupComponents.IconSprite == null)
			{
				return;
			}
			this.UnlockGroupComponents.IconSprite.gameObject.SetActive(false);
			this.UnlockGroupComponents.IconSpriteAura.gameObject.SetActive(false);
			this.UnlockGroupComponents.LevelLabel.gameObject.SetActive(false);
			this.UnlockGroupComponents.ExplosionIconSprite.gameObject.SetActive(false);
			this.UnlockGroupComponents.ExplosionAnimation.Stop();
			this.UnlockGroupComponents.UnlockSkinAnimation.Stop();
		}

		public ItemTypeScriptableObject StoreItemType;

		public GameObject HardPriceGroup;

		public GameObject SoftPriceGroup;

		public UILabel itemName;

		public UIButton UnlockButton;

		public int Index;

		[Header("[Unlock Group]")]
		[SerializeField]
		protected StoreItem.UnlockGroup UnlockGroupComponents;

		private UIDragScrollView _scrollView;

		private bool _isReadyToOpenLock;

		private bool _canBuySkin;

		private bool _isUnlockSeen;

		[Serializable]
		public struct UnlockGroup
		{
			public Animation UnlockSkinAnimation;

			public Animation ExplosionAnimation;

			public UILabel LevelLabel;

			public UI2DSprite IconSprite;

			public UI2DSprite IconSpriteAura;

			public UI2DSprite ExplosionIconSprite;
		}
	}
}
