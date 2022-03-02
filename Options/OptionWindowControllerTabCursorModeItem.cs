using System;
using Hoplon.Input.Business;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct OptionWindowControllerTabCursorModeItem
	{
		[Header("[Action Name - Options Sheet]")]
		public string CursorActionDraft;

		public JoystickCursorMode defaultJoystickCursorMode;
	}
}
