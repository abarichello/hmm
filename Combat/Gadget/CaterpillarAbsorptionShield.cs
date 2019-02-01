using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CaterpillarAbsorptionShield : GadgetBehaviour
	{
		public CaterpillarAbsorptionShieldInfo MyInfo
		{
			get
			{
				return base.Info as CaterpillarAbsorptionShieldInfo;
			}
		}

		private CombatObject CurrentWardCombatObject
		{
			get
			{
				if (this._currentWardEffect == -1)
				{
					return null;
				}
				if (this._currentWardCombatObject)
				{
					return this._currentWardCombatObject;
				}
				this._currentWardCombatObject = CombatRef.GetCombat(GameHubBehaviour.Hub.ObjectCollection.GetObjectByKind(ContentKind.Wards.Byte(), this._currentWardEffect));
				return this._currentWardCombatObject;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this._upgWardModifiers = ModifierData.CreateData(this.MyInfo.WardModifiers, this.MyInfo);
			this._upgWardExtraModifiers = ModifierData.CreateData(this.MyInfo.WardExtraModifiers, this.MyInfo);
			this._upgExplosionModifiers = ModifierData.CreateData(this.MyInfo.ExplosionModifiers, this.MyInfo);
			this._upgExplosionRange = new Upgradeable(this.MyInfo.ExplosionRangeUpgrade, this.MyInfo.ExplosionRange, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgWardModifiers.SetLevel(upgradeName, level);
			this._upgWardExtraModifiers.SetLevel(upgradeName, level);
			this._upgExplosionModifiers.SetLevel(upgradeName, level);
			this._upgExplosionRange.SetLevel(upgradeName, level);
		}

		protected override void Awake()
		{
			base.Awake();
			this.GadgetBaseUpdater = new GadgetBaseUpdater(GameHubBehaviour.Hub, this, new Action(this.UpdateJokerBar), new Action(base.GadgetUpdate), new Func<int>(this.FireGadget), null, null, new Action<int>(base.OnGadgetUsed));
			this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private void Start()
		{
			this._myTransform = this.Combat.transform;
		}

		private void UpdateJokerBar()
		{
			if (this.CurrentWardCombatObject != null)
			{
				this.Combat.GadgetStates.SetJokerBarState(this.CurrentWardCombatObject.Data.HP, (float)this.CurrentWardCombatObject.Data.HPMax);
			}
			else
			{
				this.Combat.GadgetStates.SetJokerBarState(0f, 0f);
			}
		}

		protected override void GadgetUpdate()
		{
			this.GadgetBaseUpdater.RunGadgetUpdate();
		}

		protected override int FireGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.WardEffect);
			effectEvent.LifeTime = this.MyInfo.WardLifeTime;
			effectEvent.Origin = this._myTransform.position;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Origin + this._myTransform.forward);
			effectEvent.Modifiers = this._upgWardModifiers;
			effectEvent.ExtraModifiers = this._upgWardExtraModifiers;
			this._currentWardEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.OnGadgetUsed(this._currentWardEffect);
			if (this.MyInfo.WardFeedback)
			{
				this.Combat.Feedback.Add(this.MyInfo.WardFeedback, this._currentWardEffect, this.Combat.Id.ObjId, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (int)(this.MyInfo.WardLifeTime * 1000f), 0, base.Slot);
			}
			return this._currentWardEffect;
		}

		private void FireExplosion()
		{
			this._currentWardEffect = -1;
			if (this._upgExplosionRange <= 0f)
			{
				return;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ExplosionEffect);
			effectEvent.Origin = this._myTransform.position;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Origin + this._myTransform.forward);
			effectEvent.Modifiers = this._upgExplosionModifiers;
			effectEvent.Range = this._upgExplosionRange;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId == this._currentWardEffect)
			{
				this.FireExplosion();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CaterpillarAbsorptionShield));

		protected GadgetBaseUpdater GadgetBaseUpdater;

		private Transform _myTransform;

		private int _currentWardEffect = -1;

		private CombatObject _currentWardCombatObject;

		private ModifierData[] _upgWardModifiers;

		private ModifierData[] _upgWardExtraModifiers;

		private ModifierData[] _upgExplosionModifiers;

		private Upgradeable _upgExplosionRange;
	}
}
