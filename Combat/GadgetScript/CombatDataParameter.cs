using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/CombatData")]
	public class CombatDataParameter : Parameter<CombatData>
	{
		public override int CompareTo(IParameterContext context, BaseParameter other)
		{
			CombatObjectParameter combatObjectParameter = (CombatObjectParameter)other;
			if (combatObjectParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			CombatData value = base.GetValue(context);
			if (value != null)
			{
				bs.WriteInt(value.Id.ObjId);
			}
			else
			{
				bs.WriteInt(-1);
			}
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			base.SetValue(context, CombatRef.GetCombat(bs.ReadInt()).Data);
		}
	}
}
