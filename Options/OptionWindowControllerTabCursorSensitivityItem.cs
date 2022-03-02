using System;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct OptionWindowControllerTabCursorSensitivityItem
	{
		[Header("[Action Name - Options Sheet]")]
		public string CursorActionDraft;

		[Range(0f, 1f)]
		public float DefaultValuePercent;

		public int VisualMinValue;

		public int VisualMaxValue;

		public float InfraMinValue;

		public float InfraMaxValue;
	}
}
