using System;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class StringNGuiDropdown : NGuiDropdown<string>
	{
		public StringNGuiDropdown(UIPopupList popupList) : base(popupList)
		{
		}
	}
}
