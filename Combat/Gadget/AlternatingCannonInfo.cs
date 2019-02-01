using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class AlternatingCannonInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(AlternatingCannon);
		}

		public float SwitchDelay;

		[Header("Check this to make it possible to switch back to effect when extra effect is on")]
		public bool CanSwitchExtraToNormal;
	}
}
