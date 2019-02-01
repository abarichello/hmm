using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public abstract class AbstractFX : GameHubBehaviour
	{
		public abstract int EventId { get; set; }

		protected MonoBehaviour[] DestroyEffectListenerScripts
		{
			get
			{
				MonoBehaviour[] result;
				if ((result = this._destroyEffectListenerScripts) == null)
				{
					result = (this._destroyEffectListenerScripts = base.GetComponentsInChildren<MonoBehaviour>(true));
				}
				return result;
			}
		}

		public abstract Identifiable Target { get; }

		public abstract Identifiable Owner { get; }

		public abstract Vector3 TargetPosition { get; }

		public abstract byte CustomVar { get; }

		public abstract CDummy.DummyKind GetDummyKind();

		public abstract Transform GetDummy(CDummy.DummyKind kind);

		public abstract GadgetBehaviour GetGadget();

		public abstract bool WasCreatedInFog();

		public override string ToString()
		{
			return string.Format("{0} Visible={1} EventId={2} Target={3} TargetPos={4} Owner={5} OwnerPos={6} TargetPosition={7} CustomVar={8}", new object[]
			{
				base.name,
				this.Visible,
				this.EventId,
				(!(this.Target == null)) ? this.Target.name : "null",
				(!(this.Target == null)) ? this.Target.transform.position.ToString() : string.Empty,
				(!(this.Owner == null)) ? this.Owner.name : "null",
				(!(this.Owner == null)) ? this.Owner.transform.position.ToString() : string.Empty,
				this.TargetPosition,
				this.CustomVar
			});
		}

		public bool Visible = true;

		protected MonoBehaviour[] _destroyEffectListenerScripts;
	}
}
