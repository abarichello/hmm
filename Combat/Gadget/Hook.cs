using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class Hook : BasicLink
	{
		public new HookInfo MyInfo
		{
			get
			{
				return base.Info as HookInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._hookBackDamage = ModifierData.CreateData(this.MyInfo.HookBackDamage, this.MyInfo);
			this._hookBackExtraDamage = ModifierData.CreateData(this.MyInfo.HookBackExtraDamage, this.MyInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._hookBackDamage.SetLevel(upgradeName, level);
			this._hookBackExtraDamage.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			int num = base.FireGadget();
			this._currentHook = num;
			this._hookBack = false;
			return num;
		}

		protected override void RunBeforeUpdate()
		{
			this.Combat.GadgetStates.SetJokerBarState(this.Combat.Data.HPTemp, this.Combat.Data.HPTemp);
		}

		private void LateUpdate()
		{
			if (this._hitWall && !this._hitCombat)
			{
				this.HookFail(this._wallOrigin);
			}
			this._hitWall = false;
			this._hitCombat = false;
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.DestroyReason == BaseFX.EDestroyReason.Cleanup)
			{
				return;
			}
			if (evt.RemoveData.TargetEventId != this._currentHook)
			{
				return;
			}
			if (evt.RemoveData.SrvWasScenery && !this._hitCombat)
			{
				if (!this._hookBack)
				{
					this._wallOrigin = evt.RemoveData.Origin;
					this._hitWall = true;
				}
				return;
			}
			this._hitCombat = true;
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(evt.RemoveData.TargetId);
			if (!@object || @object == this.Combat)
			{
				return;
			}
			List<Identifiable> list = new List<Identifiable>();
			if (this._hookMoreThanOne)
			{
				Collider[] array = Physics.OverlapSphere(@object.transform.position, base.Radius, 1077058560);
				for (int i = 0; i < array.Length; i++)
				{
					CombatObject combat = CombatRef.GetCombat(array[i]);
					bool flag = BaseFX.CheckHit(this.Combat, combat, this.MyInfo.Effect);
					if (flag && combat.Controller && @object.ObjId != combat.Id.ObjId)
					{
						list.Add(combat.Id);
					}
				}
				EffectEvent effectEvent = base.GetEffectEvent(base.CannonInfo.ExtraEffect);
				effectEvent.MoveSpeed = this._moveSpeed.Get();
				effectEvent.Origin = @object.transform.position;
				effectEvent.Target = @object.transform.position;
				effectEvent.TargetId = this.TargetId;
				effectEvent.LifeTime = this.ExtraLifeTime;
				effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
				effectEvent.Modifiers = ModifierData.CopyData(this.ExtraModifier);
				int effectID = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
				base.ExistingFiredEffectsAdd(effectID);
			}
			this.CreateHook(this.MyInfo.HookBackEffect, @object, this.Combat.transform.position);
			for (int j = 0; j < list.Count; j++)
			{
				Identifiable target = list[j];
				this.CreateHook(this.MyInfo.HookBackEffect, target, this.Combat.transform.position);
			}
			if (this.OnHookedTargets != null)
			{
				this.OnHookedTargets(@object, list);
			}
		}

		private void CreateHook(FXInfo effect, Identifiable target, Vector3 origin)
		{
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.MoveSpeed = this.MyInfo.HookBackMoveSpeed;
			effectEvent.TargetId = target.ObjId;
			effectEvent.Modifiers = this._hookBackDamage;
			effectEvent.ExtraModifiers = this._hookBackExtraDamage;
			effectEvent.LifeTime = base.LifeTime;
			if (this.MyInfo.GoToTarget)
			{
				effectEvent.Origin = origin;
				effectEvent.Target = target.transform.position;
			}
			else
			{
				effectEvent.Origin = target.transform.position;
				effectEvent.Target = origin;
			}
			int effectID = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(effectID);
		}

		private void HookFail(Vector3 initialPosition)
		{
			HookInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.HookFailEffect);
			effectEvent.MoveSpeed = myInfo.HookFailMoveSpeed;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			effectEvent.Modifiers = ModifierData.CopyData(this._damage);
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			effectEvent.Origin = initialPosition;
			base.SetTargetAndDirection(effectEvent);
			effectEvent.Direction = -effectEvent.Direction;
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(num);
			this._currentHook = num;
			this._hookBack = true;
		}

		public override void ExecuteSecondClick()
		{
			if (this._hookBack)
			{
				return;
			}
			base.ExecuteSecondClick();
		}

		public Action<Identifiable, IEnumerable<Identifiable>> OnHookedTargets;

		private int _currentHook;

		private bool _hookBack;

		protected ModifierData[] _hookBackDamage;

		protected ModifierData[] _hookBackExtraDamage;

		private bool _hookMoreThanOne;

		private bool _hitWall;

		private Vector3 _wallOrigin;

		private bool _hitCombat;
	}
}
