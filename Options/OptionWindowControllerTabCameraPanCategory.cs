using System;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct OptionWindowControllerTabCameraPanCategory
	{
		[Header("[Title Name - Options Sheet]")]
		public string TitleDraft;

		public OptionWindowControllerTabCameraPanItem[] CameraPanItems;
	}
}
