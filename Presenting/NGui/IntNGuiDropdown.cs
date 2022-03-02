using System;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class IntNGuiDropdown : NGuiDropdown<int>
	{
		public IntNGuiDropdown(UIPopupList popupList) : base(popupList)
		{
		}
	}
}
