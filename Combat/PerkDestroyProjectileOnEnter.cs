using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyProjectileOnEnter : BasePerk, IPerkWithCollision
	{
		public override void PerkInitialized()
		{
			this.SanityCheck();
		}

		private bool SanityCheck()
		{
			if (this._isSanityChecked)
			{
				return true;
			}
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return false;
			}
			this._isSanityChecked = true;
			return true;
		}

		public int Priority()
		{
			return -1;
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
			if (!this.SanityCheck())
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this.Effect.IsDead)
			{
				return;
			}
			BaseFX fx = BaseFX.GetFX(other.GetComponent<Collider>());
			CombatObject other2 = (!fx) ? null : fx.Gadget.Combat;
			bool flag = this.Effect.CheckHit(other2);
			if (flag && fx && fx.Gadget && (this.SlotFilter == GadgetSlot.Any || fx.Gadget.Slot == this.SlotFilter))
			{
				fx.TriggerDestroy(-1, base._trans.position, false, other, Vector3.zero, BaseFX.EDestroyReason.Default, isBarrier);
			}
		}

		public void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyProjectileOnEnter));

		public GadgetSlot SlotFilter = GadgetSlot.Any;

		private bool _isSanityChecked;
	}
}
