﻿using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Vector3")]
	public class Vector3Parameter : Parameter<Vector3>
	{
		protected override void WriteToBitStream(object context, BitStream bs)
		{
			bs.WriteVector3(base.GetValue(context));
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, bs.ReadVector3());
		}
	}
}
