using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/Link")]
	public class CombatLinkParameter : Parameter<ICombatLink>
	{
		public override int CompareTo(object context, BaseParameter other)
		{
			CombatLinkParameter combatLinkParameter = (CombatLinkParameter)other;
			if (combatLinkParameter.GetValue(context) == base.GetValue(context))
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
