using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnStayInArea : BasePerk, IPerkWithCollision
	{
		public int Priority()
		{
			return -2;
		}

		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._hitSet.Clear();
			this._modifiers = this.Effect.Data.Modifiers;
			this._combatModifiers.Clear();
			this.TestCollisionInstantly();
		}

		public void TestCollisionInstantly()
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			component.position += PerkDamageOnStayInArea.Translation;
			RaycastHit[] array = component.SweepTestAll(Vector3.down, 100f, QueryTriggerInteraction.Collide);
			PerkDamageOnStayInArea.CombatHits.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				Collider collider = array[i].collider;
				CombatObject combatObject = this.CombatHitEnter(collider);
				if (combatObject)
				{
					PerkDamageOnStayInArea.CombatHits.Add(new BarrierUtils.CombatHit
					{
						Col = collider,
						Combat = combatObject,
						Barrier = BarrierUtils.IsBarrier(collider)
					});
				}
			}
			BarrierUtils.FilterByRaycastFromPoint(this.Effect.Data.Origin, PerkDamageOnStayInArea.CombatHits);
			for (int j = 0; j < PerkDamageOnStayInArea.CombatHits.Count; j++)
			{
				CombatObject combat = PerkDamageOnStayInArea.CombatHits[j].Combat;
				Vector3 normalizedDir = this.GetNormalizedDir(combat);
				this.ApplyModifiers(combat, false, normalizedDir);
			}
			component.position = this.Effect.Data.Origin;
		}

		public void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
			if (this.Effect.IsDead || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			CombatObject combatObject = this.CombatHitEnter(other);
			if (combatObject == null)
			{
				return;
			}
			if (this._hitSet.Contains(combatObject))
			{
				return;
			}
			Vector3 normalizedDir = this.GetNormalizedDir(combatObject);
			this.ApplyModifiers(combatObject, barrier, normalizedDir);
		}

		private Vector3 GetNormalizedDir(CombatObject combat)
		{
			return (this.Effect.Gadget.Combat.transform.position - combat.transform.position).normalized;
		}

		public void ApplyModifiers(CombatObject combat, bool barrier, Vector3 dir)
		{
			ModifierData[] array = ModifierData.CopyData(this._modifiers);
			this._combatModifiers.Add(combat, array);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ForceLifeTime("lifetime", array[i].LifeTime - (float)(GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this.Effect.Event.Time) * 0.001f);
			}
			array.SetDirection(dir);
			combat.Controller.AddModifiers(array, this.Effect.Gadget.Combat, this.Effect.EventId, barrier);
			this._hitSet.Add(combat);
		}

		public void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
			if (this.Effect.IsDead || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			CombatObject combatObject = this.CombatHitEnter(other);
			if (combatObject == null)
			{
				return;
			}
			if (!this._combatModifiers.ContainsKey(combatObject))
			{
				return;
			}
			combatObject.Controller.RemoveModifiers(this._combatModifiers[combatObject], this.Effect.Gadget.Combat, this.Effect.EventId);
			this._combatModifiers.Remove(combatObject);
			this._hitSet.Remove(combatObject);
		}

		private CombatObject CombatHitEnter(Collider other)
		{
			CombatObject combatObject = null;
			CombatObject combatObject2 = null;
			bool flag = false;
			switch (this.Mode)
			{
			case PerkDamageOnEnter.ModeEnum.DamageOtherAndOwner:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTargetOnEnterScenery:
				combatObject = (combatObject2 = CombatRef.GetCombat(this.Effect.Target));
				flag = (9 == other.gameObject.layer);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOtherIgnoreTarget:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				flag = (combatObject2 && combatObject2.Id != this.Effect.Target && this.Effect.CheckHit(combatObject2));
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTarget:
				combatObject = CombatRef.GetCombat(this.Effect.Target);
				combatObject2 = CombatRef.GetCombat(other);
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTargetIgnoreOther:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				flag = (combatObject2 && combatObject2.Id == this.Effect.Target && this.Effect.CheckHit(combatObject2));
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOwner:
				combatObject = CombatRef.GetCombat(this.Effect.Owner);
				combatObject2 = CombatRef.GetCombat(other);
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOther:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				if (combatObject && combatObject.Id.ObjId != this.Effect.Owner.ObjId)
				{
					flag = this.Effect.CheckHit(combatObject2);
				}
				break;
			}
			if (flag && combatObject && combatObject2)
			{
				return combatObject2;
			}
			return null;
		}

		[Header("This Perk will apply damage only when the target is in the area")]
		public PerkDamageOnEnter.ModeEnum Mode = PerkDamageOnEnter.ModeEnum.DamageOtherAndOwner;

		private Dictionary<CombatObject, ModifierData[]> _combatModifiers = new Dictionary<CombatObject, ModifierData[]>();

		private readonly HashSet<CombatObject> _hitSet = new HashSet<CombatObject>();

		private ModifierData[] _modifiers;

		private const float TranslationDistance = 100f;

		private static readonly Vector3 Translation = new Vector3(0f, 100f, 0f);

		private static readonly List<BarrierUtils.CombatHit> CombatHits = new List<BarrierUtils.CombatHit>();
	}
}
