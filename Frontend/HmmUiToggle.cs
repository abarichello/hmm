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
			ColorBlock colors = base.colors;
			ColorBlock colorBlock = default(ColorBlock);
			colorBlock.colorMultiplier = colors.colorMultiplier;
			colorBlock.disabledColor = ((!isOnValue) ? this._disabledColor : colors.normalColor);
			colorBlock.fadeDuration = colors.fadeDuration;
			colorBlock.highlightedColor = colors.highlightedColor;
			colorBlock.normalColor = colors.normalColor;
			colorBlock.pressedColor = colors.pressedColor;
			colors = colorBlock;
			base.colors = colors;
		}

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			switch (state)
			{
			case 0:
				this.ColorNormal(instant);
				break;
			case 1:
				this.ColorHover(instant);
				break;
			case 2:
				this.ColorPressed(instant);
				break;
			case 3:
				this.ColorDisabled(instant);
				break;
			}
		}

		private void ColorHover(bool instant)
		{
			for (int i = 0; i < this._graphicColorData.Length; i++)
			{
				this._graphicColorData[i].SetColorHighlighted(instant);
			}
			for (int j = 0; j < this._spriteSwappers.Length; j++)
			{
				this._spriteSwappers[j].SetColorHighlighted();
			}
		}

		private void ColorNormal(bool instant)
		{
			for (int i = 0; i < this._graphicColorData.Length; i++)
			{
				this._graphicColorData[i].SetColorNormal(instant);
			}
			for (int j = 0; j < this._spriteSwappers.Length; j++)
			{
				this._spriteSwappers[j].SetColorNormal();
			}
		}

		private void ColorPressed(bool instant)
		{
			for (int i = 0; i < this._graphicColorData.Length; i++)
			{
				this._graphicColorData[i].SetColorPressed(instant);
			}
			for (int j = 0; j < this._spriteSwappers.Length; j++)
			{
				this._spriteSwappers[j].SetColorPressed();
			}
		}

		private void ColorDisabled(bool instant)
		{
			for (int i = 0; i < this._graphicColorData.Length; i++)
			{
				this._graphicColorData[i].SetColorDisabled(instant);
			}
			for (int j = 0; j < this._spriteSwappers.Length; j++)
			{
				this._spriteSwappers[j].SetColorDisabled();
			}
		}

		private bool _allowSwitchOff;

		private Color _disabledColor;

		[SerializeField]
		private UiButtonSpriteSwapper[] _spriteSwappers = new UiButtonSpriteSwapper[0];

		[SerializeField]
		private SelectableGraphicColorData[] _graphicColorData = new SelectableGraphicColorData[0];
	}
}
