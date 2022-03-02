using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class ItemGadgetSelfEffect : GadgetBehaviour
	{
		private ItemGadgetSelfEffectInfo MyInfo
		{
			get
			{
				return base.Info as ItemGadgetSelfEffectInfo;
			}
		}

		protected virtual void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._transform = this.Combat.transform;
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._modifiers = ModifierData.CreateData(this.MyInfo.Modifiers, this.MyInfo);
			this.UpdateModifiersPlusAttach();
			this.ReduceDamage = new Upgradeable(this.MyInfo.ReduceDamageOnHitUpgrade, this.MyInfo.ReduceDamageOnHit, base.Info.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._modifiers.SetLevel(upgradeName, level);
			this.ReduceDamage.SetLevel(upgradeName, level);
		}

		protected override void Awake()
		{
			base.Awake();
			this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		protected override int FireGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.LifeTime = base.LifeTime;
			effectEvent.Range = this.GetRange();
			effectEvent.CustomVar = (byte)this.ReduceDamage.Get();
			effectEvent.Origin = this._transform.position;
			effectEvent.Target = this._transform.position;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target + this._transform.forward);
			effectEvent.Modifiers = ModifierData.CopyData(this._modifiersPlusAttach);
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			this._currentEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this._currentFeedbackId = ((!(this.MyInfo.Feedback != null)) ? -1 : this.Combat.Feedback.Add(this.MyInfo.Feedback, -1, this.Combat.Id.ObjId, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (int)(base.LifeTime * 1000f), 0, base.Slot));
			return this._currentEffectId;
		}

		protected virtual void RemoveFeedback()
		{
			if (this._currentFeedbackId != -1)
			{
				this.Combat.Feedback.Remove(this._currentFeedbackId);
				this._currentFeedbackId = -1;
			}
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.TargetEventId != this._currentEffectId)
			{
				return;
			}
			this._currentEffectId = -1;
			this.RemoveFeedback();
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			this.RemoveFeedback();
		}

		protected virtual void UpdateModifiersPlusAttach()
		{
			ModifierData[] attachedDamage = base.AttachedDamage;
			this._modifiersPlusAttach = new ModifierData[attachedDamage.Length + this._modifiers.Length];
			for (int i = 0; i < this._modifiersPlusAttach.Length; i++)
			{
				this._modifiersPlusAttach[i] = ((i < attachedDamage.Length) ? attachedDamage[i] : this._modifiers[i - attachedDamage.Length]);
			}
		}

		public override void AttachDamage(ModifierData[] damage)
		{
			base.AttachDamage(damage);
			this.UpdateModifiersPlusAttach();
		}

		protected ModifierData[] _modifiers;

		protected ModifierData[] _modifiersPlusAttach;

		protected Transform _transform;

		protected int _currentFeedbackId = -1;

		protected int _currentEffectId = -1;

		protected Upgradeable ReduceDamage;
	}
}
