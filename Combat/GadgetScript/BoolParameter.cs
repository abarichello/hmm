using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Bool")]
	public class BoolParameter : Parameter<bool>
	{
		protected override void WriteToBitStream(object context, BitStream bs)
		{
			bs.WriteBool(base.GetValue(context));
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, bs.ReadBool());
		}
	}
}
