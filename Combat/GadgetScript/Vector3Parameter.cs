using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Vector3")]
	public class Vector3Parameter : Parameter<Vector3>
	{
		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			bs.WriteVector3(base.GetValue(context));
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			base.SetValue(context, bs.ReadVector3());
		}
	}
}
