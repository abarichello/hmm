using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class DopplerLaserLinkInfo : DamageAngleFirstEnemyInfo
	{
		public override Type GadgetType()
		{
			return typeof(DopplerLaserLink);
		}

		[Header("[Laser Link]")]
		public FXInfo InfiniteRay;

		public bool UseInfiniteRay;

		public string InfiniteRayUpgrade;
	}
}
