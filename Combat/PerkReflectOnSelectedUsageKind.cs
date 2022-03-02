using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkReflectOnSelectedUsageKind : BasePerk, IPerkWithCollision, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this._latestRadius = this.Effect.GetRadius();
		}

		public int Priority()
		{
			return 0;
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
			if (this.Effect.IsDead || GameHubBehaviour.Hub.Net.IsClient() || !this.UsageKind.HasFlag(PerkUsageKind.OnTriggerHit))
			{
				return;
			}
			PhysicsUtils2.SimpleHit hit = PhysicsUtils2.GetHit(other.transform.position, base._trans.GetComponent<Collider>());
			this.Reflect(other, hit.point, hit.normal, isBarrier);
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

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || !this.UsageKind.HasFlag(PerkUsageKind.OnDestroy))
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(evt.RemoveData.Origin, this._latestRadius, 1085467648);
			for (int i = 0; i < array.Length; i++)
			{
				Collider collider = array[i];
				PhysicsUtils2.SimpleHit hit = PhysicsUtils2.GetHit(collider.transform.position, base._trans.GetComponent<Collider>());
				this.Reflect(array[i], hit.point, hit.normal, false);
			}
		}

		private void Reflect(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isBarrier)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (combat == null)
			{
				return;
			}
			ModifierData[] datas = (!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers;
			combat.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, -1, hitNormal, hitPoint, isBarrier);
		}

		public bool UseExtraModifiers;

		[BitMask(typeof(PerkUsageKind))]
		public PerkUsageKind UsageKind;

		private float _latestRadius;
	}
}
