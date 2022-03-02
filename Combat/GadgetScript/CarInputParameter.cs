using System;
using HeavyMetalMachines.Car;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/CarInput")]
	public class CarInputParameter : Parameter<CarInput>
	{
		public override int CompareTo(object context, BaseParameter other)
		{
			CarInputParameter carInputParameter = (CarInputParameter)other;
			if (carInputParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(object context, BitStream bs)
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

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, CombatRef.GetCombat(bs.ReadInt()).CarInput);
		}
	}
}
