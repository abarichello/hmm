using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCallbackAreaOnTick : BasePerk
	{
		private Vector3 _origin
		{
			get
			{
				if (this.IsDynamic)
				{
					if (!this._myCombatTransform)
					{
						this._myCombatTransform = this.Effect.Gadget.Combat.transform;
					}
					return this._myCombatTransform.position;
				}
				return this.Effect.Data.Origin;
			}
		}

		private Vector3 _direction
		{
			get
			{
				if (this.IsDynamic)
				{
					return this.Effect.Gadget.CalcDirection(this._origin, this.Effect.Gadget.Target);
				}
				return this.Effect.Data.Direction;
			}
		}

		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._timedUpdater = new TimedUpdater(this.TickMillis, false, false);
			if (this.Effect.Data.Range != 0f)
			{
				this.Radius = this.Effect.Data.Range;
				this.SquareSize.x = this.Effect.Data.Range;
				this.SquareSize.y = this.Effect.Data.Range;
			}
		}

		private void FixedUpdate()
		{
			if (!this._timedUpdater.ShouldHalt())
			{
				PerkCallbackAreaOnTick.ShapeMode shape = this.Shape;
				if (shape != PerkCallbackAreaOnTick.ShapeMode.Sphere)
				{
					if (shape == PerkCallbackAreaOnTick.ShapeMode.Box)
					{
						this.CheckBox();
					}
				}
				else
				{
					this.CheckSphere();
				}
			}
		}

		private void CheckSphere()
		{
			Collider[] array = Physics.OverlapSphere(this._origin, this.Radius, 1077054464);
			List<CombatObject> list = new List<CombatObject>();
			foreach (Collider comp in array)
			{
				CombatObject combat = CombatRef.GetCombat(comp);
				if (!list.Contains(combat))
				{
					bool flag = this.Effect.CheckHit(combat);
					if (flag && combat && combat.Controller)
					{
						list.Add(combat);
					}
				}
			}
			Mural.Post(new DamageAreaCallback(list, this._origin, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
		}

		private void CheckBox()
		{
			float y = this.SquareSize.y;
			float x = this.SquareSize.x;
			Vector3 origin = this._origin;
			Vector3 direction = this._direction;
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
			List<CombatObject> list = new List<CombatObject>();
			foreach (Collider collider in array)
			{
				CombatObject combat = CombatRef.GetCombat(collider);
				if (!list.Contains(combat))
				{
					if (!combat || !collider || (collider.PlaneCast(plane) != 1 && collider.PlaneCast(plane2) != 1))
					{
						if (this.Effect.CheckHit(combat))
						{
							list.Add(combat);
						}
					}
				}
			}
			Mural.Post(new DamageAreaCallback(list, origin, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkCallbackAreaOnTick));

		private TimedUpdater _timedUpdater;

		public PerkCallbackAreaOnTick.ShapeMode Shape;

		public float Radius;

		public Vector2 SquareSize = Vector2.zero;

		public int TickMillis;

		public GadgetSlot TargetGadgetCallback;

		public bool IsDynamic;

		private Transform _myCombatTransform;

		public enum ShapeMode
		{
			Sphere,
			Box
		}
	}
}
