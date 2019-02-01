using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class GuiGadgetCounterComponents
	{
		public void Setup(GadgetBehaviour gadget, Func<float> getCurrent, Func<float> getMax, Func<bool> hasUpgrade)
		{
			this.GetCurrent = getCurrent;
			this.GetMax = getMax;
			this.HasUpgrade = hasUpgrade;
			this._offset = 0f;
			this.UpgradeAnimation.Stop();
			for (int i = 0; i < this.GlowSprites.Length; i++)
			{
				UI2DSprite ui2DSprite = this.GlowSprites[i];
				ui2DSprite.gameObject.SetActive(false);
				ui2DSprite.transform.parent.gameObject.SetActive(false);
			}
			if (gadget is SpikeTrap)
			{
				this.onlyShowWhenUpgraded = true;
				for (int j = 0; j < gadget.Info.Damage.Length; j++)
				{
					ModifierInfo modifierInfo = gadget.Info.Damage[j];
					if (modifierInfo.Effect == EffectKind.HPLightDamage || modifierInfo.Effect == EffectKind.HPHeavyDamage)
					{
						this._offset += modifierInfo.Amount;
					}
				}
				this.GadgetCounterIcon.sprite2D = this.TrapSprite;
			}
			else if (gadget is GranTorinoRocket)
			{
				this.onlyShowWhenUpgraded = false;
				this.GadgetCounterIcon.sprite2D = this.ScrapSprite;
			}
			this.SetCounter(this.GetCurrent());
			this.SetMaxCounter(this.GetMax());
			this.isActive = true;
		}

		private void SetCounter(float counter)
		{
			if (Math.Abs(this._counterCache - counter) < 0.01f)
			{
				return;
			}
			this._counterCache = counter;
			this.BaseCounterLabel.text = (counter + this._offset).ToString("0");
		}

		private void SetMaxCounter(float maxCounter)
		{
			this.UpgradeMaxCounterLabel.text = string.Format("/{0}", (maxCounter + this._offset).ToString("0"));
		}

		public void Update()
		{
			bool flag = this.HasUpgrade();
			if (this._hasUpgradeCache != flag)
			{
				this._hasUpgradeCache = flag;
				this.CounterUpgrade(this._hasUpgradeCache);
			}
			if (this._hasUpgradeCache)
			{
				this.SetCounter(this.GetCurrent());
			}
		}

		private void CounterUpgrade(bool isActive)
		{
			if (this.onlyShowWhenUpgraded)
			{
				this.GroupGameObject.SetActive(isActive);
			}
			GUIUtils.PlayAnimation(this.UpgradeAnimation, !isActive, 1f, string.Empty);
			if (isActive)
			{
				this.SetMaxCounter(this.GetMax());
			}
			this.SetCounter(this.GetCurrent());
		}

		[Header("[Main Group]")]
		public GameObject GroupGameObject;

		[Header("[Base Info]")]
		public UILabel BaseCounterLabel;

		[Header("[Upgrade Group]")]
		public GameObject UpgradeGroupGameObject;

		[Header("[Upgrade Animation]")]
		public Animation UpgradeAnimation;

		[Header("[Max Info]")]
		public UILabel UpgradeMaxCounterLabel;

		[Header("[Gadget Counter Icon]")]
		public UI2DSprite GadgetCounterIcon;

		[Header("[Specific Icons]")]
		public Sprite ScrapSprite;

		public Sprite TrapSprite;

		[Header("[Glow List]")]
		public UI2DSprite[] GlowSprites;

		public bool isActive;

		private bool onlyShowWhenUpgraded;

		private Func<float> GetCurrent;

		private Func<float> GetMax;

		private Func<bool> HasUpgrade;

		private float _counterCache = -1f;

		private bool _hasUpgradeCache;

		private float _offset;
	}
}
