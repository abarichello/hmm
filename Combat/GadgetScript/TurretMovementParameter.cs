using System;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/TurretMovement")]
	public class TurretMovementParameter : Parameter<ITurretMovement>
	{
		public override int CompareTo(object context, BaseParameter other)
		{
			TurretMovementParameter turretMovementParameter = (TurretMovementParameter)other;
			if (turretMovementParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
			ITurretMovement value = base.GetValue(context);
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
			base.SetValue(context, CombatRef.GetCombat(bs.ReadInt()).TurretMovement);
		}
	}
}
