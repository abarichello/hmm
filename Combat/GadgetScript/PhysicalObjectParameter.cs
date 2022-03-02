using System;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/PhysicalObject")]
	public class PhysicalObjectParameter : Parameter<IPhysicalObject>
	{
		public override int CompareTo(object context, BaseParameter other)
		{
			PhysicalObjectParameter physicalObjectParameter = (PhysicalObjectParameter)other;
			if (physicalObjectParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
			bs.WriteInt(((CombatMovement)base.GetValue(context)).Combat.Id.ObjId);
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, ((IHMMGadgetContext)context).GetCombatObject(bs.ReadInt()).PhysicalObject);
		}
	}
}
