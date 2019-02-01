using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class FireEffectsInAreaInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(FireEffectsInArea);
		}

		[Header("Effect that will be created in the given FireEffectRadius")]
		public FXInfo AreaEffect;

		public ModifierInfo[] AreaModifier;

		public float FireEffectRadius;

		public string FireEffectRadiusUpgrade;

		public bool OnlyMyTeam;

		[Header("Will respect this Direction, if the future should respect Effect (ShotPosAndDir) :P")]
		public BasicCannonInfo.DirectionEnum Direction;

		[Header("Hit mask to find combatants in the area of effect and apply the basic fire effect")]
		public HitMask AreaHitMask;
	}
}
