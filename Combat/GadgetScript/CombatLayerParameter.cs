using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/CombatLayer")]
	public class CombatLayerParameter : Parameter<CombatLayer>
	{
		public override int CompareTo(object context, BaseParameter other)
		{
			CombatLayerParameter combatLayerParameter = (CombatLayerParameter)other;
			if (combatLayerParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
		}
	}
}
