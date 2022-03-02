using System;
using Hoplon.Input.Business;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct OptionWindowControllerTabCameraPanItem
	{
		[Header("[Action Name - Options Sheet]")]
		public string PanActionDraft;

		[Range(0f, 1f)]
		public float DefaultValuePercent;

		public int VisualMinValue;

		public int VisualMaxValue;

		public float InfraMinValue;

		public float InfraMaxValue;

		public CameraPanCode CameraPanCode;
	}
}
