using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/CombatObject")]
	public class CombatObjectParameter : Parameter<ICombatObject>
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
			ICombatObject value = base.GetValue(context);
			if (value != null)
			{
				bs.WriteInt(value.Identifiable.ObjId);
			}
			else
			{
				bs.WriteInt(-1);
			}
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			base.SetValue(context, ((IHMMGadgetContext)context).GetCombatObject(bs.ReadInt()));
		}
	}
}
