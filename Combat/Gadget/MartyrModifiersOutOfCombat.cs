using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MartyrModifiersOutOfCombat : GadgetBehaviour, DamageTakenCallback.IDamageTakenCallbackListener, ActionCallback.IActionCallbackListener, ICachedObject
	{
		private MartyrModifiersOutOfCombatInfo MyInfo
		{
			get
			{
				return base.Info as MartyrModifiersOutOfCombatInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgModifiers = ModifierData.CreateData(this.MyInfo.Modifiers, this.MyInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgModifiers.SetLevel(upgradeName, level);
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!base.Activated)
			{
				return;
			}
			if (this._currentModifiersEffect == -1)
			{
				return;
			}
			this.DestroyEffect();
			this.FireGadget();
		}

		protected override void Awake()
		{
			base.Awake();
			this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		public override void Activate()
		{
			base.Activate();
			this._myTransform = this.Combat.transform;
			this.StartListening();
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!base.Activated)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreboardState.BombDelivery)
			{
				return;
			}
			if (this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Dead) || this.Combat.SpawnController.State != SpawnStateKind.Spawned)
			{
				if (this._currentModifiersEffect != -1)
				{
					this.DestroyEffectAndResetCooldown();
				}
				else
				{
					this.ResetCooldown();
				}
				return;
			}
			if (this._currentModifiersEffect == -1)
			{
				this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
			base.GadgetUpdate();
			if (this._currentModifiersEffect != -1)
			{
				CombatData data = this.Combat.Data;
				if (data.HP >= (float)data.HPMax)
				{
					this.DestroyEffectAndResetCooldown();
				}
				return;
			}
			if (this.CurrentTime < this.CurrentCooldownTime)
			{
				return;
			}
			CombatData data2 = this.Combat.Data;
			if (data2.HP >= (float)data2.HPMax)
			{
				this.ResetCooldown();
				return;
			}
			this.FireGadget();
		}

		public override void Clear()
		{
			base.Clear();
			this.DestroyEffectAndResetCooldown();
		}

		private void StartListening()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._currentListenerEffect != -1)
			{
				return;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ListenerEffect);
			effectEvent.Origin = this._myTransform.position;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Origin + this._myTransform.forward);
			this._currentListenerEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			if (this.FireNormalAndExtraEffectsTogether.BoolGet())
			{
				EffectEvent effectEvent2 = base.GetEffectEvent(this.MyInfo.ExtraEffect);
				effectEvent2.Range = this.GetRange();
				effectEvent2.Origin = GadgetBehaviour.DummyPosition(this.Combat, this.MyInfo.ExtraEffect);
				effectEvent2.Target = base.Target;
				effectEvent2.TargetId = this.TargetId;
				effectEvent2.LifeTime = ((this.ExtraLifeTime <= 0f) ? (effectEvent2.Range / effectEvent2.MoveSpeed) : this.ExtraLifeTime);
				effectEvent2.ExtraModifiers = this.ExtraModifier;
				base.SetTargetAndDirection(effectEvent2);
				GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent2);
			}
		}

		private void StopListening()
		{
			if (this._currentListenerEffect == -1)
			{
				return;
			}
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentListenerEffect,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._currentListenerEffect = -1;
		}

		protected override int FireGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ModifiersEffect);
			effectEvent.Modifiers = this._upgModifiers;
			effectEvent.Origin = this._myTransform.position;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Origin + this._myTransform.forward);
			this._currentModifiersEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			return this._currentModifiersEffect;
		}

		public void OnDamageTakenCallback(DamageTakenCallback evt)
		{
			if (evt.ListenerEffectId == this._currentListenerEffect && evt.Amount > 0f && !this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Invulnerable))
			{
				this.DestroyEffectAndResetCooldown();
			}
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			this.DestroyEffectAndResetCooldown();
		}

		public void OnActionCallback(ActionCallback evt)
		{
			if (evt.EffectId == this._currentListenerEffect)
			{
				this.DestroyEffectAndResetCooldown();
			}
		}

		public void DestroyEffectAndResetCooldown()
		{
			if (this._currentModifiersEffect != -1)
			{
				this.DestroyEffect();
			}
			this.ResetCooldown();
		}

		private void ResetCooldown()
		{
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime;
		}

		private void DestroyEffect()
		{
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentModifiersEffect,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._currentModifiersEffect = -1;
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.TargetEventId == this._currentModifiersEffect)
			{
				this._currentModifiersEffect = -1;
				this.ResetCooldown();
			}
			if (evt.RemoveData.TargetEventId == this._currentListenerEffect)
			{
				this._currentListenerEffect = -1;
				this.StartListening();
			}
		}

		public void OnSendToCache()
		{
			this.StopListening();
		}

		public void OnGetFromCache()
		{
		}

		private ModifierData[] _upgModifiers;

		private Transform _myTransform;

		private int _currentModifiersEffect = -1;

		private int _currentListenerEffect = -1;
	}
}
