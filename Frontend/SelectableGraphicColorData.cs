using System;
using ModestTree;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class SelectableGraphicColorData
	{
		public void SetColorHighlighted(bool instant)
		{
			this.CrossFadeColor(this.ColorBlock.highlightedColor, instant);
		}

		public void SetColorNormal(bool instant)
		{
			this.CrossFadeColor(this.ColorBlock.normalColor, instant);
		}

		public void SetColorPressed(bool instant)
		{
			this.CrossFadeColor(this.ColorBlock.pressedColor, instant);
		}

		public void SetColorDisabled(bool instant)
		{
			this.CrossFadeColor(this.ColorBlock.disabledColor, instant);
		}

		private void CrossFadeColor(Color color, bool instant)
		{
			Assert.IsNotNull(this.TargetGraphic, "TargetGraphic not set! " + StackTraceUtility.ExtractStackTrace());
			float num = (!instant) ? this.ColorBlock.fadeDuration : 0f;
			this.TargetGraphic.CrossFadeColor(color, num, true, this.UseAlpha);
		}

		[SerializeField]
		private Graphic TargetGraphic;

		[SerializeField]
		private ColorBlock ColorBlock;

		[SerializeField]
		private bool UseAlpha = true;
	}
}
