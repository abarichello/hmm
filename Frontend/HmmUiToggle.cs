using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiToggle")]
	public class HmmUiToggle : Toggle
	{
		protected override void Awake()
		{
			base.Awake();
			this._allowSwitchOff = (base.group == null || base.group.allowSwitchOff);
			this._disabledColor = base.colors.disabledColor;
			if (!this._allowSwitchOff)
			{
				this.onValueChanged.RemoveListener(new UnityAction<bool>(this.OnValueChanged));
				this.onValueChanged.AddListener(new UnityAction<bool>(this.OnValueChanged));
			}
		}

		private void OnValueChanged(bool isOnValue)
		{
			base.interactable = !isOnValue;
			base.colors = new ColorBlock
			{
				colorMultiplier = base.colors.colorMultiplier,
				disabledColor = ((!isOnValue) ? this._disabledColor : base.colors.normalColor),
				fadeDuration = base.colors.fadeDuration,
				highlightedColor = base.colors.highlightedColor,
				normalColor = base.colors.normalColor,
				pressedColor = base.colors.pressedColor
			};
		}

		private bool _allowSwitchOff;

		private Color _disabledColor;
	}
}
