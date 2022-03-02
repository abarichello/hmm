using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/String")]
	public class StringParameter : Parameter<string>
	{
		protected override void WriteToBitStream(object context, BitStream bs)
		{
			bs.WriteString(base.GetValue(context));
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, bs.ReadString());
		}
	}
}
