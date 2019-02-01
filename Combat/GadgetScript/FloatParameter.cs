using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Float")]
	public class FloatParameter : Parameter<float>, INumericParameter
	{
		public float GetFloatValue(IParameterContext context)
		{
			return base.GetValue(context);
		}

		public void SetFloatValue(IParameterContext context, float value)
		{
			base.SetValue(context, value);
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			bs.WriteFloat(base.GetValue(context));
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			base.SetValue(context, bs.ReadFloat());
		}
	}
}
