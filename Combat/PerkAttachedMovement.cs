using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkAttachedMovement : BasePerk
	{
		private Identifiable TargetIdentifiable
		{
			get
			{
				return (this.RefTarget != BasePerk.PerkTarget.Owner) ? this.Effect.Target : this.Effect.Owner;
			}
		}

		public override void PerkInitialized()
		{
			if (this.TargetIdentifiable && this.ColliderCombatRef != null)
			{
				this.ColliderCombatRef.Combat = this.TargetIdentifiable.GetBitComponent<CombatObject>();
			}
			Transform targetDummy = BasePerk.GetTargetDummy(this.TargetIdentifiable, this.dummyKind, this.customDummyName);
			Transform transform = this.Effect.transform;
			transform.parent = targetDummy;
			transform.localPosition = this.LocalPosition;
		}

		public override void PerkDestroyed(DestroyEffectMessage destroyEffectMessage)
		{
			base.PerkDestroyed(destroyEffectMessage);
			if (this.ColliderCombatRef != null)
			{
				this.ColliderCombatRef.Combat = null;
			}
		}

		public BasePerk.PerkTarget RefTarget;

		public CDummy.DummyKind dummyKind;

		public Vector3 LocalPosition;

		public string customDummyName;

		public CombatRef ColliderCombatRef;
	}
}
