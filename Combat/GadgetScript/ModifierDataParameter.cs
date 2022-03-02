using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/ModifierData")]
	public class ModifierDataParameter : Parameter<ModifierData>
	{
		protected override void WriteToBitStream(object context, BitStream bs)
		{
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
		}
	}
}
