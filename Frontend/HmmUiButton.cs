using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiButton")]
	public class HmmUiButton : Button
	{
		protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			switch (state)
			{
			case Selectable.SelectionState.Normal:
				this.ColorNormal();
				break;
			case Selectable.SelectionState.Highlighted:
				this.ColorHover();
				break;
			case Selectable.SelectionState.Pressed:
				this.ColorPressed();
				break;
			case Selectable.SelectionState.Disabled:
				this.OnDeselect();
				break;
			}
		}

		private void OnDeselect()
		{
			this.ColorDisabled();
		}

		private void ColorHover()
		{
			for (int i = 0; i < this._graphicColorData.Length; i++)
			{
				HmmUiButton.ButtonGraphicColorData buttonGraphicColorData = this._graphicColorData[i];
				buttonGraphicColorData.TargetGraphic.CrossFadeColor(buttonGraphicColorData.ColorBlock.highlightedColor, buttonGraphicColorData.ColorBlock.fadeDuration, true, buttonGraphicColorData.UseAlpha);
			}
		}

		private void ColorNormal()
		{
			for (int i = 0; i < this._graphicColorData.Length; i++)
			{
				HmmUiButton.ButtonGraphicColorData buttonGraphicColorData = this._graphicColorData[i];
				buttonGraphicColorData.TargetGraphic.CrossFadeColor(buttonGraphicColorData.ColorBlock.normalColor, buttonGraphicColorData.ColorBlock.fadeDuration, true, buttonGraphicColorData.UseAlpha);
			}
		}

		private void ColorPressed()
		{
			for (int i = 0; i < this._graphicColorData.Length; i++)
			{
				HmmUiButton.ButtonGraphicColorData buttonGraphicColorData = this._graphicColorData[i];
				buttonGraphicColorData.TargetGraphic.CrossFadeColor(buttonGraphicColorData.ColorBlock.pressedColor, buttonGraphicColorData.ColorBlock.fadeDuration, true, buttonGraphicColorData.UseAlpha);
			}
		}

		private void ColorDisabled()
		{
			for (int i = 0; i < this._graphicColorData.Length; i++)
			{
				HmmUiButton.ButtonGraphicColorData buttonGraphicColorData = this._graphicColorData[i];
				buttonGraphicColorData.TargetGraphic.CrossFadeColor(buttonGraphicColorData.ColorBlock.disabledColor, buttonGraphicColorData.ColorBlock.fadeDuration, true, buttonGraphicColorData.UseAlpha);
			}
		}

		[SerializeField]
		private HmmUiButton.ButtonGraphicColorData[] _graphicColorData = new HmmUiButton.ButtonGraphicColorData[0];

		[Serializable]
		public class ButtonGraphicColorData
		{
			public Graphic TargetGraphic;

			public ColorBlock ColorBlock;

			public bool UseAlpha = true;
		}
	}
}
