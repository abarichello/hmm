using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Combat/PhysicalObject")]
	public class PhysicalObjectParameter : Parameter<IPhysicalObject>
	{
		public override int CompareTo(IParameterContext context, BaseParameter other)
		{
			PhysicalObjectParameter physicalObjectParameter = (PhysicalObjectParameter)other;
			if (physicalObjectParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			bs.WriteInt(((CombatMovement)base.GetValue(context)).Combat.Id.ObjId);
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			base.SetValue(context, ((IHMMGadgetContext)context).GetCombatObject(bs.ReadInt()).PhysicalObject);
		}
	}
}
