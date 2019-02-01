using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Integer")]
	public class IntParameter : Parameter<int>, INumericParameter
	{
		public float GetFloatValue(IParameterContext context)
		{
			return (float)base.GetValue(context);
		}

		public void SetFloatValue(IParameterContext context, float value)
		{
			base.SetValue(context, (int)value);
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			bs.WriteInt(base.GetValue(context));
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			base.SetValue(context, bs.ReadInt());
		}
	}
}
