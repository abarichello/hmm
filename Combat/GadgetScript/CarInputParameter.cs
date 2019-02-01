using System;
using HeavyMetalMachines.Car;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/CarInput")]
	public class CarInputParameter : Parameter<CarInput>
	{
		public override int CompareTo(IParameterContext context, BaseParameter other)
		{
			CarInputParameter carInputParameter = (CarInputParameter)other;
			if (carInputParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			CarInput value = base.GetValue(context);
			if (value != null)
			{
				bs.WriteInt(value.GetComponent<Identifiable>().ObjId);
			}
			else
			{
				bs.WriteInt(-1);
			}
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			base.SetValue(context, CombatRef.GetCombat(bs.ReadInt()).CarInput);
		}
	}
}
