using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class PerkAttachToDummy : PerkAttachToObject
	{
		protected override Vector3 TargetInterpolatedPosition
		{
			get
			{
				return this._targetTransform.position;
			}
		}

		protected override Transform GetTarget()
		{
			Identifiable targetIdentifiable = base.TargetIdentifiable;
			if (targetIdentifiable == null)
			{
				return null;
			}
			CDummy bitComponentInChildren = targetIdentifiable.GetBitComponentInChildren<CDummy>();
			if (bitComponentInChildren == null)
			{
				return targetIdentifiable.transform;
			}
			Transform dummy = bitComponentInChildren.GetDummy(this.dummyKind, this.customDummyName);
			if (dummy == null)
			{
				return targetIdentifiable.transform;
			}
			return dummy;
		}

		public CDummy.DummyKind dummyKind;

		public string customDummyName;
	}
}
