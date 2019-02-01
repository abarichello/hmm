using System;

namespace HeavyMetalMachines.Utils
{
	public static class NGUIExtensions
	{
		public static void SetEnabledAndSelected(this UIButton me)
		{
			me.isEnabled = true;
			me.SetState(UIButtonColor.State.Hover, true);
			UICamera.selectedObject = me.gameObject;
			UICamera.controllerNavigationObject = me.gameObject;
		}
	}
}
