using System;
using Standard_Assets.Scripts.HMM.Util;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct OptionWindowControllerTabCursorCategory
	{
		[Header("[Title Name - Options Sheet]")]
		public MultiPlatformLocalizationDraft TitleDraft;

		public OptionWindowControllerTabCursorSensitivityItem CursorSensitivityItem;

		public OptionWindowControllerTabCursorModeItem CursorModeItem;
	}
}
