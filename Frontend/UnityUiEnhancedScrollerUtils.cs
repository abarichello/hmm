using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public static class UnityUiEnhancedScrollerUtils
	{
		public static void SetSpacing(this EnhancedScroller enhancedScroller, int spacing)
		{
			HorizontalOrVerticalLayoutGroup component = enhancedScroller.ScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
			component.spacing = (float)spacing;
			enhancedScroller.spacing = (float)spacing;
		}
	}
}
