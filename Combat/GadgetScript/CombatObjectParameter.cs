using System;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/CombatObject")]
	public class CombatObjectParameter : Parameter<ICombatObject>
	{
		public override int CompareTo(object context, BaseParameter other)
		{
			IParameterTomate<ICombatObject> parameterTomate = (IParameterTomate<ICombatObject>)other.ParameterTomate;
			if (parameterTomate.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(object context, BitStream bs)
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

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, ((IHMMGadgetContext)context).GetCombatObject(bs.ReadInt()));
		}
	}
}
