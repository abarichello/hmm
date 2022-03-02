using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnTick : BasePerk
	{
		protected Vector3 OriginPosition
		{
			get
			{
				return (!this.UseOriginPosition) ? this._origin.position : this.Effect.Data.Origin;
			}
		}

		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			base.PerkInitialized();
			this._timedUpdater = new TimedUpdater(this.TickMillis, false, false);
			this._modifiers = base.GetModifiers(this.Source);
			this._target = base.GetTargetCombat(this.Effect, this.Target);
			this._origin = base.GetTargetTransform(this.Effect, this.Origin);
			PerkDamageOnTick.Log.DebugFormat("_origin:{0} pos:{1} _target:{2} pos:{3}", new object[]
			{
				this._origin.name,
				this.OriginPosition,
				this._target.name,
				this._target.transform.position
			});
		}

		private void FixedUpdate()
		{
			if (!this._timedUpdater.ShouldHalt())
			{
				this.DealDamage();
			}
		}

		protected virtual void DealDamage()
		{
			if (this._target == null)
			{
				return;
			}
			Vector3 normalized = (this._target.Transform.position - this.OriginPosition).normalized;
			ModifierData[] array = this._modifiers;
			if (this.DamageByRange)
			{
				float baseAmount = this.DamageToRange.Evaluate(Vector3.Distance(this._target.Transform.position, this.OriginPosition));
				array = ModifierData.CreateConvoluted(array, baseAmount);
			}
			this._target.Controller.AddModifiers(array, this.Effect.Gadget.Combat, this.Effect.EventId, normalized, this.OriginPosition, false);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkDamageOnTick));

		public bool UseOriginPosition;

		public BasePerk.PerkTarget Origin;

		public BasePerk.PerkTarget Target;

		public int TickMillis;

		public bool DamageByRange = true;

		public AnimationCurve DamageToRange;

		public BasePerk.DamageSource Source;

		protected ModifierData[] _modifiers;

		protected TimedUpdater _timedUpdater;

		protected CombatObject _target;

		protected Transform _origin;
	}
}
