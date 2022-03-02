using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Vector2")]
	public class Vector2Parameter : Parameter<Vector2>
	{
		protected override void WriteToBitStream(object context, BitStream bs)
		{
			bs.WriteVector2(base.GetValue(context));
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, bs.ReadVector2());
		}
	}
}
