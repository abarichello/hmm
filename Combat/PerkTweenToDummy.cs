using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkTweenToDummy : BasePerk
	{
		private Identifiable TargetIdentifiable
		{
			get
			{
				return (this.Target != BasePerk.PerkTarget.Owner) ? this.Effect.Target : this.Effect.Owner;
			}
		}

		public override void PerkInitialized()
		{
			Transform targetDummy = BasePerk.GetTargetDummy(this.TargetIdentifiable, this.FromDummy, this.FromCustomDummy);
			Transform targetDummy2 = BasePerk.GetTargetDummy(this.TargetIdentifiable, this.ToDummy, this.ToCustomDummy);
			this.Effect.transform.parent = targetDummy;
			this.Effect.transform.localPosition = Vector3.zero;
			this.Effect.transform.localRotation = Quaternion.identity;
			TweenPosition.Begin(base.gameObject, this.Duration, targetDummy2.localPosition);
		}

		public BasePerk.PerkTarget Target;

		public CDummy.DummyKind FromDummy;

		public string FromCustomDummy;

		public CDummy.DummyKind ToDummy;

		public string ToCustomDummy;

		public float Duration = 1f;
	}
}
