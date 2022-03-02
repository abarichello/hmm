using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Float")]
	public class FloatParameter : Parameter<float>
	{
		protected override void WriteToBitStream(object context, BitStream bs)
		{
			bs.WriteFloat(base.GetValue(context));
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, bs.ReadFloat());
		}
	}
}
