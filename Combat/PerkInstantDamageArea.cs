using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Obsolete("Use PerkInstantDamageCollider instead")]
	public class PerkInstantDamageArea : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this.UserPerkRadius && this.Effect.Gadget.Radius != 0f)
			{
				this.Radius = this.Effect.Gadget.Radius;
				this.SquareSize.x = this.Effect.Gadget.Radius;
				this.SquareSize.y = this.Effect.Gadget.Radius;
			}
			PerkInstantDamageArea.ShapeMode shape = this.Shape;
			if (shape != PerkInstantDamageArea.ShapeMode.Sphere)
			{
				if (shape == PerkInstantDamageArea.ShapeMode.Box)
				{
					this.DamageBox();
				}
			}
			else
			{
				this.DamageSphere();
			}
		}

		private void DamageSphere()
		{
			Vector3 origin = this.Effect.Data.Origin;
			Collider[] array = Physics.OverlapSphere(origin, this.Radius, 1077054464);
			List<BarrierUtils.CombatHit> list = new List<BarrierUtils.CombatHit>();
			foreach (Collider collider in array)
			{
				CombatObject combat = CombatRef.GetCombat(collider);
				bool flag = this.Effect.CheckHit(combat);
				bool barrier = BarrierUtils.IsBarrier(collider);
				if (flag && combat && combat.Controller)
				{
					PerkInstantDamageArea.IgnoreMode ignore = this.Ignore;
					if (ignore != PerkInstantDamageArea.IgnoreMode.None)
					{
						if (ignore != PerkInstantDamageArea.IgnoreMode.Target)
						{
							if (ignore == PerkInstantDamageArea.IgnoreMode.Owner)
							{
								if (this.Effect.Owner && combat.Id.ObjId == this.Effect.Owner.Id.ObjId)
								{
									goto IL_1A8;
								}
							}
						}
						else if (combat.Id.ObjId == this.Effect.Data.TargetId)
						{
							goto IL_1A8;
						}
					}
					if (this.TargetToTakeDamage == BasePerk.PerkTarget.Target)
					{
						list.Add(new BarrierUtils.CombatHit
						{
							Combat = combat,
							Col = collider,
							Barrier = barrier
						});
					}
					else if (this.TargetToTakeDamage == BasePerk.PerkTarget.Owner)
					{
						CombatObject combat2 = CombatRef.GetCombat(this.Effect.Owner);
						if (combat2 != null)
						{
							list.Add(new BarrierUtils.CombatHit
							{
								Combat = combat2,
								Col = combat2.GetComponentInChildren<Collider>(),
								Barrier = false
							});
						}
					}
				}
				IL_1A8:;
			}
			if (this.Effect.Data.EffectInfo.PrioritizeBarrier)
			{
				BarrierUtils.FilterByBarrierPriority(list);
			}
			else
			{
				BarrierUtils.FilterByRaycastFromPoint(base._trans.position, list);
			}
			List<CombatObject> list2 = new List<CombatObject>();
			ModifierData[] datas = (!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers;
			for (int j = 0; j < list.Count; j++)
			{
				CombatObject combat3 = list[j].Combat;
				Vector3 normalized = (combat3.Transform.position - origin).normalized;
				combat3.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, this.Effect.EventId, normalized, origin, list[j].Barrier);
				list2.Add(combat3);
			}
			if (this.IsDamageCallbackEnabled)
			{
				Mural.Post(new DamageAreaCallback(list2, origin, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
			}
		}

		private void DamageBox()
		{
			float y = this.SquareSize.y;
			float x = this.SquareSize.x;
			Vector3 origin = this.Effect.Data.Origin;
			Vector3 direction = this.Effect.Data.Direction;
			Vector3 vector = Vector3.Cross(direction, Vector3.up);
			Vector3 vector2 = vector * -1f;
			Vector3 vector3 = origin + vector * (x / 2f);
			Vector3 vector4 = origin + vector2 * (x / 2f);
			Plane plane;
			plane..ctor(vector2, vector4);
			Plane plane2;
			plane2..ctor(vector, vector3);
			float num = Mathf.Sqrt(Mathf.Pow(y / 2f, 2f) + Mathf.Pow(x / 2f, 2f));
			Vector3 vector5 = origin;
			Collider[] array = Physics.OverlapSphere(vector5, num, 1077054464);
			List<BarrierUtils.CombatHit> list = new List<BarrierUtils.CombatHit>();
			foreach (Collider collider in array)
			{
				CombatObject combat = CombatRef.GetCombat(collider);
				bool barrier = BarrierUtils.IsBarrier(collider);
				if (!combat || !collider || (collider.PlaneCast(plane) != 1 && collider.PlaneCast(plane2) != 1))
				{
					if (this.Effect.CheckHit(combat))
					{
						list.Add(new BarrierUtils.CombatHit
						{
							Combat = combat,
							Col = collider,
							Barrier = barrier
						});
					}
				}
			}
			BarrierUtils.FilterByRaycastFromPoint(base._trans.position, list);
			ModifierData[] datas = (!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers;
			List<CombatObject> list2 = new List<CombatObject>();
			for (int j = 0; j < list.Count; j++)
			{
				CombatObject combat2 = list[j].Combat;
				Vector3 normalized = (combat2.Transform.position - this.Effect.Data.Origin).normalized;
				combat2.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, this.Effect.EventId, normalized, this.Effect.Data.Origin, list[j].Barrier);
				list2.Add(combat2);
			}
			if (this.IsDamageCallbackEnabled)
			{
				Mural.Post(new DamageAreaCallback(list2, origin, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkInstantDamageArea));

		public BasePerk.PerkTarget TargetToTakeDamage = BasePerk.PerkTarget.Target;

		public GadgetSlot TargetGadgetCallback;

		public PerkInstantDamageArea.ShapeMode Shape;

		public PerkInstantDamageArea.IgnoreMode Ignore;

		public bool UseExtraModifiers;

		public float Radius;

		public bool UserPerkRadius = true;

		public Vector2 SquareSize = Vector2.zero;

		public bool IsDamageCallbackEnabled;

		public enum ShapeMode
		{
			Sphere,
			Box
		}

		public enum IgnoreMode
		{
			None,
			Target,
			Owner
		}
	}
}
