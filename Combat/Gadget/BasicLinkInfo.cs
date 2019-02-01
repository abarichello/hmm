using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BasicLinkInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicLink);
		}

		[Header("[Link]")]
		public float OwnerMass = 1f;

		public string OwnerMassUpgrade;

		public float OwnerMassWhenStruggling = 1f;

		public string OwnerMassWhenStrugglingUpgrade;

		public float TargetMass = 1f;

		public string TargetMassUpgrade;

		public float TargetMassWhenStruggling = 1f;

		public string TargetMassWhenStrugglingUpgrade;

		public CDummy.DummyKind OwnerDummy;

		public string OwnerCustomDummyName;

		public CDummy.DummyKind TargetDummy;

		public string TargetCustomDummyName;

		public string TagLink;

		public bool StealLink;

		public string StealLinkUpgrade;

		public float Compression;

		public float Tension;

		public bool ClampIn;

		public bool ClampOut;
	}
}
