﻿using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/ModifierDataArray")]
	public class ModifierDataArrayParameter : Parameter<ModifierData[]>
	{
		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
		}
	}
}
