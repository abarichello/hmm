using System;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct OptionWindowControllerTab
	{
		public OptionWindowControllerTabInputCategory[] TabInputCategories;

		public OptionWindowControllerTabCameraPanCategory TabCameraPanCategory;

		public OptionWindowControllerTabCursorCategory TabCursorCategory;

		public OptionWindowControllerTabInputLockedCategory[] TabInputLockedCategories;
	}
}
