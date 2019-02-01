using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkInstantCallbackArea : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._origin = this.Effect.Data.Origin;
			this._direction = this.Effect.Data.Direction;
			if (this.Effect.Data.Range != 0f)
			{
				this.Radius = this.Effect.Data.Range;
				this.SquareSize.x = this.Effect.Data.Range;
				this.SquareSize.y = this.Effect.Data.Range;
			}
			PerkInstantCallbackArea.ShapeMode shape = this.Shape;
			if (shape != PerkInstantCallbackArea.ShapeMode.Sphere)
			{
				if (shape == PerkInstantCallbackArea.ShapeMode.Box)
				{
					this.CheckBox();
				}
			}
			else
			{
				this.CheckSphere();
			}
		}

		private void CheckSphere()
		{
			Collider[] array = Physics.OverlapSphere(this._origin, this.Radius, 1077058560);
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
			Vector3 inPoint = origin + vector * (x / 2f);
			Vector3 inPoint2 = origin + vector2 * (x / 2f);
			Plane plane = new Plane(vector2, inPoint2);
			Plane plane2 = new Plane(vector, inPoint);
			float radius = Mathf.Sqrt(Mathf.Pow(y / 2f, 2f) + Mathf.Pow(x / 2f, 2f));
			Vector3 position = origin;
			Collider[] array = Physics.OverlapSphere(position, radius, 1077058560);
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

		public static readonly BitLogger Log = new BitLogger(typeof(PerkInstantCallbackArea));

		public PerkInstantCallbackArea.ShapeMode Shape;

		public float Radius;

		public Vector2 SquareSize = Vector2.zero;

		public GadgetSlot TargetGadgetCallback;

		private Vector3 _origin;

		private Vector3 _direction;

		public enum ShapeMode
		{
			Sphere,
			Box
		}
	}
}
