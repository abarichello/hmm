using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkInstantDamageAngle : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._angle = this.Angle;
			this._range = ((this.Range != 0f) ? this.Range : this.Effect.Data.Range);
			BarrierUtils.OverlapSphereRaycastFromCenter(this.Effect.Data.Origin, this._range, 1077058560, PerkInstantDamageAngle._objects);
			for (int i = 0; i < PerkInstantDamageAngle._objects.Count; i++)
			{
				BarrierUtils.CombatHit combatHit = PerkInstantDamageAngle._objects[i];
				bool flag = this.Effect.CheckHit(combatHit.Combat);
				if (flag && combatHit.Combat && combatHit.Combat.Controller)
				{
					if (PhysicsUtils.IsInsideAngle(this.Effect.Data.Origin, this.Effect.Data.Direction, combatHit.Col, this._angle))
					{
						combatHit.Combat.Controller.AddModifiers(this.Effect.Data.Modifiers, this.Effect.Gadget.Combat, this.Effect.EventId, combatHit.Barrier);
					}
				}
			}
			PerkInstantDamageAngle._objects.Clear();
		}

		public float Angle = 180f;

		public float Range;

		private float _angle;

		private float _range;

		private static readonly List<BarrierUtils.CombatHit> _objects = new List<BarrierUtils.CombatHit>(10);
	}
}
