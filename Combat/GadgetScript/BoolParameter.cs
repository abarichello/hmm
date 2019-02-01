using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Bool")]
	public class BoolParameter : Parameter<bool>
	{
		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			bs.WriteBool(base.GetValue(context));
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			base.SetValue(context, bs.ReadBool());
		}
	}
}
