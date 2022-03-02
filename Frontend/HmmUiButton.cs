using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiButton")]
	public class HmmUiButton : Button
	{
		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			if (!this.IsActive())
			{
				return;
			}
			if (!this.IsInteractable() && base.currentSelectionState == 1)
			{
				base.UpdateSelectionState(null);
			}
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

		[SerializeField]
		private UiButtonSpriteSwapper[] _spriteSwappers = new UiButtonSpriteSwapper[0];

		[SerializeField]
		private SelectableGraphicColorData[] _graphicColorData = new SelectableGraphicColorData[0];
	}
}
