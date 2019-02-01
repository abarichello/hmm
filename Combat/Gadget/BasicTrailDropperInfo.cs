using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicTrailDropperInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicTrailDropper);
		}

		[Header("[BasicTrailDropper]")]
		public bool UseTrail;

		public string UseTrailUpgrade;

		public FXInfo TrailEffect;

		public ModifierInfo[] TrailModifier;

		public float TrailPiecesLifeTime;

		public float TrailPiecesDropIntervalMillis;

		public int TrailColliderRadius;

		public bool TrailMustFollowCar;
	}
}
