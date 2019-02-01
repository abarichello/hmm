using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class ReflectiveProjectileInfo : MouseDelayedProjectileInfo
	{
		public override Type GadgetType()
		{
			return typeof(ReflectiveProjectile);
		}

		[Header("[Reflective]")]
		public FXInfo ReflectEffect;

		public ModifierInfo[] ReflectModifiers;

		public float ReflectLifetime;

		public int MaxReflections;

		public bool IsReflectable = true;
	}
}
