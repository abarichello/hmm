﻿using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class TakeoffInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(TakeoffGadget);
		}

		[Header("Take Off Variables")]
		[Tooltip("If grid lands on yellow, no effect, if lands on green, fire effect, if lands on red, fire extra effect")]
		public float YellowToGreenValue = 0.1f;

		public float GreenToRedValue = 0.2f;
	}
}
