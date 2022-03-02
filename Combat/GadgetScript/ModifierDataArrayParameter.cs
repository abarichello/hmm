using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/ModifierDataArray")]
	public class ModifierDataArrayParameter : Parameter<ModifierData[]>
	{
		protected override void WriteToBitStream(object context, BitStream bs)
		{
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
		}
	}
}
