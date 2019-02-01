using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class TrollAllInOneInfo : BasicTrailDropperInfo
	{
		public override Type GadgetType()
		{
			return typeof(TrollAllInOne);
		}

		[Header("[TrollAllInOne]")]
		public bool UseEffect3;

		public string UseEffect3Upgrade;

		public float Effect3LifeTime;

		public FXInfo Damage3Effect;

		public ModifierInfo[] Damage3Modifier;
	}
}
