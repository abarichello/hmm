using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class GadgetBehaviour : GameHubBehaviour, IObjectSpawnListener, DestroyEffectMessage.IDestroyEffectListener, IGadgetInput
	{
		public GadgetInfo Info
		{
			get
			{
				return this._info;
			}
			set
			{
				this._info = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GadgetBehaviour.OnDrainLifeDelegate OnSelfRepair;

		public GadgetSlot Slot
		{
			get
			{
				return this._slot;
			}
			set
			{
				this._slot = value;
			}
		}

		public GadgetNatureKind Nature
		{
			get
			{
				if (this.Combat.HasGadgetContext((int)this.Slot))
				{
					return ((CombatGadget)this.Combat.GetGadgetContext((int)this.Slot)).Nature;
				}
				return this._info.Nature;
			}
		}

		public bool Activated
		{
			get
			{
				return this._activated;
			}
			set
			{
				if (this._activated != value && this.Combat != null && this.Combat.IsPlayer)
				{
					GameHubBehaviour.Hub.Stream.StatsStream.Changed(this.Combat.GetComponent<PlayerStats>());
				}
				this._activated = value;
			}
		}

		public bool Pressed
		{
			get
			{
				bool flag = this.Info.SpawnStateTypeToRun == this.Combat.SpawnController.State;
				if (this.Slot == GadgetSlot.PassiveGadget)
				{
					return !GameHubBehaviour.Hub.Global.LockAllPlayers && this._pressed && flag && !this.Combat.Attributes.IsGadgetDisarmed(this.Slot, this.Nature);
				}
				return !GameHubBehaviour.Hub.Global.LockAllPlayers && (this._pressed || this._isForcePressed) && flag;
			}
			set
			{
				if (!value && this._blockPressedFalse)
				{
					return;
				}
				this.PressedThisFrame = (this.Activated && value);
				this._pressed = (this.Activated && (this.Info.AlwaysPressed || value));
				if (this._pressed)
				{
					this._blockPressedFalse = true;
				}
			}
		}

		public bool IsPassiveGadget
		{
			get
			{
				return this.Slot == GadgetSlot.PassiveGadget;
			}
		}

		public Vector3 Target
		{
			get
			{
				return (!this.Info.UseInputTarget && !this.Combat.IsBot) ? (this.Dir + this.Combat.Transform.position) : this._target;
			}
			set
			{
				this._target = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ServerListenToGadgetUse;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ServerListenToWarmupFired;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ServerListenToGadgetReady;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<BaseFX> ClientListenToEffectStarted;

		public GadgetBehaviour.UpgradeInstance[] GetAllBoughtUpgrades()
		{
			return Array.FindAll<GadgetBehaviour.UpgradeInstance>(this.Upgrades, (GadgetBehaviour.UpgradeInstance x) => x.WasBought());
		}

		protected virtual Vector3 DummyPosition()
		{
			return GadgetBehaviour.DummyPosition(this.Combat, this.Info.Effect);
		}

		protected virtual Vector3 DummyPosition(EffectEvent data)
		{
			return GadgetBehaviour.DummyPosition(this.Combat, data.EffectInfo);
		}

		public static Vector3 DummyPosition(CombatObject combat, FXInfo effect)
		{
			Transform dummy = combat.Dummy.GetDummy(effect.ShotPosAndDir.Dummy, effect.ShotPosAndDir.DummyName, null);
			Vector3 result = dummy.position + dummy.rotation * effect.ShotPosAndDir.OffsetPos;
			if (result.y < -0.01f)
			{
				result.y = 0f;
			}
			return result;
		}

		protected Vector3 DummyForward(FXInfo effect)
		{
			Transform dummy = this.Combat.Dummy.GetDummy(effect.ShotPosAndDir.Dummy, effect.ShotPosAndDir.DummyName, null);
			return dummy.forward;
		}

		public virtual int ForceFire()
		{
			return this.InternalFireGadget();
		}

		protected virtual int FireWarmup()
		{
			return this.FireWarmup(null);
		}

		protected virtual int FireWarmup(Action<EffectEvent> customizeData)
		{
			if (this.Info.WarmupSeconds == 0f || string.IsNullOrEmpty(this.Info.WarmupEffect.Effect))
			{
				return -1;
			}
			EffectEvent effectEvent = this.GetEffectEvent(this.Info.WarmupEffect);
			effectEvent.Origin = GadgetBehaviour.DummyPosition(this.Combat, this.Info.WarmupEffect);
			effectEvent.Target = this.Target;
			effectEvent.Range = this.GetRange();
			effectEvent.LifeTime = this.Info.WarmupSeconds;
			effectEvent.Modifiers = this._warmupModifiers;
			this.SetTargetAndDirection(effectEvent);
			if (effectEvent.EffectInfo.ShotPosAndDir.UseTargetAsOrigin)
			{
				effectEvent.Origin = GameHubBehaviour.Hub.MatchMan.GetClosestValidPoint(effectEvent.Origin, new float[]
				{
					(float)this.Combat.Team
				}, this.Radius);
			}
			if (customizeData != null)
			{
				customizeData(effectEvent);
			}
			this.LastWarmupId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this.ExistingFiredEffectsAdd(this.LastWarmupId);
			if (!string.IsNullOrEmpty(this.Info.WarmupEffectExtra.Effect))
			{
				EffectEvent effectEvent2 = this.GetEffectEvent(this.Info.WarmupEffectExtra);
				effectEvent2.Origin = effectEvent.Origin;
				effectEvent2.Target = this.Target;
				effectEvent2.Direction = effectEvent.Direction;
				effectEvent2.Range = effectEvent.Range;
				effectEvent2.LifeTime = effectEvent.LifeTime;
				effectEvent2.Modifiers = this._warmupModifiersExtra;
				int effectID = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent2);
				this.ExistingFiredEffectsAdd(effectID);
			}
			if (this.ServerListenToWarmupFired != null)
			{
				this.ServerListenToWarmupFired();
			}
			if (this._gadgetState != null && !(this is FireOtherGadgetOverTime))
			{
				this.Combat.GadgetStates.SetEffectState(this._gadgetState, (this.LastWarmupId == -1) ? EffectState.Idle : EffectState.Warmup);
			}
			return this.LastWarmupId;
		}

		private int InternalFireGadget()
		{
			if (this.FireEffectOnSpecificTarget.BoolGet())
			{
				this.LastEffectId = -1;
				this.GetTargetId(ref this._targetIdsForSpecificTargetEffect);
				while (this._targetIdsForSpecificTargetEffect != null && this._targetIdsForSpecificTargetEffect.Count > 0)
				{
					int targetId = this._targetIdsForSpecificTargetEffect.Pop();
					int num = (!this.ReplaceFireNormalToFireExtra.BoolGet()) ? this.FireGadget(targetId) : this.FireExtraGadget(targetId);
					this.ExistingFiredEffectsAdd(num);
					this.LastEffectId = num;
					if (this.FireNormalAndExtraEffectsTogether.BoolGet() && this.LastEffectId != -1)
					{
						this.ExistingFiredEffectsAdd(this.FireExtraGadget(targetId));
					}
				}
			}
			else
			{
				this.LastEffectId = ((!this.ReplaceFireNormalToFireExtra.BoolGet()) ? this.FireGadget() : this.FireExtraGadget());
				this.ExistingFiredEffectsAdd(this.LastEffectId);
				if (this.FireNormalAndExtraEffectsTogether.BoolGet())
				{
					if (this.Info.ExtraEffectDelayMillis > 0)
					{
						base.StartCoroutine(this.FireExtraGadgetWithDelay());
					}
					else
					{
						this.ExistingFiredEffectsAdd(this.FireExtraGadget());
					}
				}
			}
			if (this._gadgetState != null && !(this is FireOtherGadgetOverTime))
			{
				this.Combat.GadgetStates.SetEffectState(this._gadgetState, (this.LastEffectId == -1) ? EffectState.Idle : EffectState.Running);
			}
			return this.LastEffectId;
		}

		private IEnumerator FireExtraGadgetWithDelay()
		{
			yield return this.waitForSeconds;
			this.ExistingFiredEffectsAdd(this.FireExtraGadget());
			yield break;
		}

		private int InternalFireExtraGadget()
		{
			return this.FireExtraGadget();
		}

		public virtual bool IsBombBlocked(BaseFX baseFx)
		{
			return this.Combat.BombBlocked;
		}

		private void GetTargetId(ref Stack<int> targets)
		{
			if (targets == null)
			{
				targets = new Stack<int>(8);
			}
			switch (this.ChosenTarget.IntGet())
			{
			case 1:
				for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
				{
					PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
					targets.Push(playerData.CharacterInstance.ObjId);
				}
				break;
			case 2:
				for (int j = 0; j < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; j++)
				{
					PlayerData playerData2 = GameHubBehaviour.Hub.Players.PlayersAndBots[j];
					if (playerData2.Team == this.Combat.Team)
					{
						targets.Push(playerData2.CharacterInstance.ObjId);
					}
				}
				break;
			case 3:
				for (int k = 0; k < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; k++)
				{
					PlayerData playerData3 = GameHubBehaviour.Hub.Players.PlayersAndBots[k];
					if (playerData3.Team != this.Combat.Team)
					{
						targets.Push(playerData3.CharacterInstance.ObjId);
					}
				}
				break;
			case 4:
			{
				FXInfo effect = this.Info.Effect;
				if (GadgetBehaviour.<>f__mg$cache0 == null)
				{
					GadgetBehaviour.<>f__mg$cache0 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.ShortestMagnitude);
				}
				int targetId = this.GetTargetId(effect, GadgetBehaviour.<>f__mg$cache0, null);
				if (targetId != -1)
				{
					targets.Push(targetId);
				}
				break;
			}
			case 5:
				this.GetGadgetTarget(this.Combat.CustomGadget0, ref targets);
				break;
			case 6:
				this.GetGadgetTarget(this.Combat.CustomGadget1, ref targets);
				break;
			case 7:
				this.GetGadgetTarget(this.Combat.CustomGadget2, ref targets);
				break;
			case 8:
			{
				FXInfo effect2 = this.Info.Effect;
				if (GadgetBehaviour.<>f__mg$cache1 == null)
				{
					GadgetBehaviour.<>f__mg$cache1 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.LowestHp);
				}
				GadgetBehaviour.CriteriaFunction pfnPrimaryCriteriaFunction = GadgetBehaviour.<>f__mg$cache1;
				if (GadgetBehaviour.<>f__mg$cache2 == null)
				{
					GadgetBehaviour.<>f__mg$cache2 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.ShortestMagnitude);
				}
				int targetId2 = this.GetTargetId(effect2, pfnPrimaryCriteriaFunction, GadgetBehaviour.<>f__mg$cache2);
				if (targetId2 != -1)
				{
					targets.Push(targetId2);
				}
				break;
			}
			case 9:
			{
				FXInfo effect3 = this.Info.Effect;
				if (GadgetBehaviour.<>f__mg$cache3 == null)
				{
					GadgetBehaviour.<>f__mg$cache3 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.LowestRelativeHp);
				}
				GadgetBehaviour.CriteriaFunction pfnPrimaryCriteriaFunction2 = GadgetBehaviour.<>f__mg$cache3;
				if (GadgetBehaviour.<>f__mg$cache4 == null)
				{
					GadgetBehaviour.<>f__mg$cache4 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.ShortestMagnitude);
				}
				int targetId3 = this.GetTargetId(effect3, pfnPrimaryCriteriaFunction2, GadgetBehaviour.<>f__mg$cache4);
				if (targetId3 != -1)
				{
					targets.Push(targetId3);
				}
				break;
			}
			}
		}

		private void GetGadgetTarget(GadgetBehaviour targetGadget, ref Stack<int> targets)
		{
			if (targetGadget.ExistingFiredEffects.Count <= 0)
			{
				return;
			}
			int num = this.Combat.CustomGadget0.ExistingFiredEffects[0];
			if (num == -1)
			{
				return;
			}
			BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(num);
			if (baseFx != null && baseFx.Target != null && baseFx.Target.ObjId != -1)
			{
				targets.Push(baseFx.Target.ObjId);
			}
		}

		protected int GetTargetId(FXInfo effect, GadgetBehaviour.CriteriaFunction pfnPrimaryCriteriaFunction, GadgetBehaviour.CriteriaFunction pfnSecondaryCriteriaFunction)
		{
			Identifiable target = this.GetTarget(effect, this.Combat.transform.position, new List<int>
			{
				this.Combat.Id.ObjId
			}, pfnPrimaryCriteriaFunction, pfnSecondaryCriteriaFunction);
			if (target)
			{
				return target.ObjId;
			}
			return -1;
		}

		public Identifiable GetTarget(FXInfo effect, Vector3 position, List<int> ignoreIds, GadgetBehaviour.CriteriaFunction funcToCalc, GadgetBehaviour.CriteriaFunction pfnSecondaryCriteriaFunction)
		{
			float num = float.MaxValue;
			float maxValue = float.MaxValue;
			float num2 = float.MaxValue;
			float maxValue2 = float.MaxValue;
			bool flag = true;
			Identifiable result = null;
			this.GetCombatsInArea(position, this.Radius, 1077054464, ref this.m_cpoCombatObjects);
			int i = 0;
			int count = this.m_cpoCombatObjects.Count;
			while (i < count)
			{
				CombatObject combatObject = this.m_cpoCombatObjects[i];
				if (combatObject && (ignoreIds == null || !ignoreIds.Contains(combatObject.Id.ObjId)))
				{
					if (combatObject.IsAlive() && BaseFX.CheckHit(this.Combat, combatObject, effect))
					{
						if (this.Info.FocusOnBombCarrier && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(combatObject))
						{
							return combatObject.Player.CharacterInstance;
						}
						if (!funcToCalc(position, combatObject, ref maxValue) && pfnSecondaryCriteriaFunction != null)
						{
							pfnSecondaryCriteriaFunction(position, combatObject, ref maxValue2);
						}
						if (flag)
						{
							flag = false;
							if (pfnSecondaryCriteriaFunction != null)
							{
								pfnSecondaryCriteriaFunction(position, combatObject, ref maxValue2);
							}
						}
						if (num != maxValue || maxValue2 != num2)
						{
							result = combatObject.Player.CharacterInstance;
							num = maxValue;
							num2 = maxValue2;
						}
					}
				}
				i++;
			}
			return result;
		}

		public static bool ShortestMagnitude(Vector3 position, CombatObject combatObject, ref float shortestMagnitude)
		{
			float sqrMagnitude = (combatObject.Player.CharacterInstance.transform.position - position).sqrMagnitude;
			if (shortestMagnitude > sqrMagnitude)
			{
				shortestMagnitude = sqrMagnitude;
			}
			else if (shortestMagnitude == sqrMagnitude)
			{
				return false;
			}
			return true;
		}

		public static bool LowestHp(Vector3 position, CombatObject combatObject, ref float lowestHp)
		{
			if (lowestHp > combatObject.Data.HP)
			{
				lowestHp = combatObject.Data.HP;
			}
			else if (lowestHp == combatObject.Data.HP)
			{
				return false;
			}
			return true;
		}

		public static bool LowestRelativeHp(Vector3 position, CombatObject combatObject, ref float lowestRelativeHp)
		{
			if (lowestRelativeHp > combatObject.Data.CurrentHPPercent)
			{
				lowestRelativeHp = combatObject.Data.CurrentHPPercent;
			}
			else if (lowestRelativeHp == combatObject.Data.CurrentHPPercent)
			{
				return false;
			}
			return true;
		}

		protected void ExistingFiredEffectsAdd(int effectID)
		{
			if (effectID == -1)
			{
				return;
			}
			if (this.ExistingFiredEffects.Contains(effectID))
			{
				return;
			}
			this.ExistingFiredEffects.Add(effectID);
		}

		protected void ExistingFiredEffectsRemove(int effectID)
		{
			this.ExistingFiredEffects.Remove(effectID);
		}

		protected void ExistingFiredEffectsRemoveAt(int index)
		{
			this.ExistingFiredEffects.RemoveAt(index);
		}

		protected bool ExistingFiredEffectsContains(int effectID)
		{
			return this.ExistingFiredEffects.Contains(effectID);
		}

		protected virtual int FireGadget()
		{
			GadgetBehaviour.Log.Warn("Fire Gadget called but not implemented! Maybe you should override method FireGadget!!!");
			return -1;
		}

		protected int FireGadget(int targetId)
		{
			return this.FireWithSpecifiedTarget(targetId, new Func<int>(this.FireGadget));
		}

		protected virtual int FireExtraGadget()
		{
			GadgetBehaviour.Log.Warn("Fire Extra Gadget called but not implemented! Maybe you should override method FireGadget!!!");
			return -1;
		}

		protected virtual int FireExtraGadget(Action<EffectEvent> customizeData)
		{
			return -1;
		}

		protected virtual int FireExtraGadget(Vector3 position)
		{
			return -1;
		}

		protected int FireExtraGadget(int targetId)
		{
			return this.FireWithSpecifiedTarget(targetId, new Func<int>(this.FireExtraGadget));
		}

		protected int FireWithSpecifiedTarget(int targetId, Func<int> fireAction)
		{
			int targetId2 = this.TargetId;
			this.TargetId = targetId;
			int result = fireAction();
			this.TargetId = targetId2;
			return result;
		}

		protected virtual int FireExtraGadgetOnDeath(DestroyEffectMessage destroyEvt)
		{
			if (this.Info.FireExtraOnEffectDeathOnlyIfTargetIdIsValid && destroyEvt.RemoveData.TargetId == -1)
			{
				return -1;
			}
			if (this.Info.FireExtraOnEffectDeathOnlyOnReason != BaseFX.EDestroyReason.None && this.Info.FireExtraOnEffectDeathOnlyOnReason != destroyEvt.RemoveData.DestroyReason)
			{
				return -1;
			}
			int targetId = this.TargetId;
			if (this.Info.UseTargetIdFromRemoveData)
			{
				this.TargetId = destroyEvt.RemoveData.TargetId;
			}
			int result;
			if (this.Info.UseEffectDeathPosition)
			{
				result = this.FireExtraGadget(destroyEvt.RemoveData.Origin);
			}
			else
			{
				result = this.FireExtraGadget();
			}
			this.TargetId = targetId;
			return result;
		}

		private IGadgetUpdater CreateGadgetUpdater()
		{
			if (!(this is BasicCannon) && !(this is ItemGadgetSelfEffect))
			{
				return null;
			}
			IGadgetUpdater result = null;
			switch (this.Info.Kind)
			{
			case GadgetKind.Instant:
				result = new GadgetBaseUpdater(GameHubBehaviour.Hub, this, new Action(this.RunBeforeUpdate), new Action(this.GadgetUpdate), new Func<int>(this.FireWarmup), new Func<int>(this.InternalFireGadget), new Func<int>(this.InternalFireExtraGadget), new Action<int>(this.OnGadgetUsed));
				break;
			case GadgetKind.InstantWithCharges:
				result = new GadgetWithChargesUpdater(GameHubBehaviour.Hub, this, new Action(this.GadgetUpdate), new Func<int>(this.InternalFireGadget), new Action<int>(this.OnGadgetUsed));
				break;
			case GadgetKind.Charged:
				result = new GadgetChargedUpdater(GameHubBehaviour.Hub, this, new Action(this.RunBeforeUpdate), new Action(this.GadgetUpdate), new Func<int>(this.FireWarmup), new Func<int>(this.InternalFireGadget), new Func<int>(this.InternalFireExtraGadget), new Action<int>(this.OnGadgetUsed));
				break;
			case GadgetKind.Pressed:
				result = new GadgetPressedUpdater(GameHubBehaviour.Hub, this, new Action(this.RunBeforeUpdate), new Action(this.GadgetUpdate), new Func<int>(this.FireWarmup), new Func<int>(this.InternalFireGadget), new Func<int>(this.InternalFireExtraGadget), new Action<int>(this.OnGadgetUsed));
				break;
			case GadgetKind.Toggle:
				result = new GadgetToggledUpdater(GameHubBehaviour.Hub, this, new Action(this.RunBeforeUpdate), new Action(this.GadgetUpdate), new Func<int>(this.InternalFireGadget), new Action<int>(this.OnGadgetUsed));
				break;
			case GadgetKind.Overheat:
				result = new GadgetOverheatUpdater(GameHubBehaviour.Hub, this, new Action(this.RunBeforeUpdate), new Action(this.GadgetUpdate), new Func<int>(this.FireWarmup), new Func<int>(this.InternalFireGadget), new Func<int>(this.InternalFireExtraGadget), new Action<int>(this.OnGadgetUsed));
				break;
			case GadgetKind.Switch:
				result = new GadgetSwitchUpdater(GameHubBehaviour.Hub, this, new Action(this.RunBeforeUpdate), new Action(this.GadgetUpdate), new Func<int>(this.FireWarmup), new Func<int>(this.InternalFireGadget), new Func<Action<EffectEvent>, int>(this.FireExtraGadget), new Action<int>(this.OnGadgetUsed));
				break;
			}
			return result;
		}

		protected virtual void RunBeforeUpdate()
		{
		}

		public void ForceFixedUpdate()
		{
			this.GadgetUpdate();
		}

		protected virtual void Awake()
		{
			if (this.Info != null)
			{
				this.SetInfo(this.Info);
				this.Info.PreCacheAssets();
				this.waitForSeconds = new WaitForSeconds((float)this.Info.ExtraEffectDelayMillis / 1000f);
			}
		}

		private void TmpSpecialHpRepairRecharge(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventId)
		{
			if (this.Combat.Data.IsFullEP() || mod.GadgetInfo == null || mod.GadgetInfo.GadgetId != this.Info.GadgetId)
			{
				return;
			}
			if (this._tmpSpecialRepairCharge != 0f && mod.Info.Effect == EffectKind.HPRepair)
			{
				ModifierData[] datas = this.CreateDrainData(EffectKind.EPRepair, amount, this._tmpSpecialRepairCharge, this.Info.TmpSpecialChargeFeedback);
				this.Combat.Controller.AddModifiers(datas, this.Combat, -1, false);
			}
		}

		private void TmpSpecialHpDamageRecharge(ModifierData mod, float amount)
		{
			if (this.Combat.Data.IsFullEP())
			{
				return;
			}
			if (this._tmpSpecialDamageCharge != 0f && mod.Info.Effect.IsHPDamage())
			{
				ModifierData[] datas = this.CreateDrainData(EffectKind.EPRepair, amount, this._tmpSpecialDamageCharge, this.Info.TmpSpecialChargeFeedback);
				this.Combat.Controller.AddModifiers(datas, this.Combat, -1, false);
			}
		}

		public virtual void DrainCheck(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			if (!this.Activated)
			{
				return;
			}
			if (mod.IsReactive || mod.Amount <= 0f)
			{
				return;
			}
			if (mod.GadgetInfo == null || mod.GadgetInfo.GadgetId != this.Info.GadgetId)
			{
				return;
			}
			this.TmpSpecialHpDamageRecharge(mod, amount);
			if (this.DrainLife != 0f)
			{
				this.DrainLifeCheck(this.Combat, taker, mod.Info.Effect.IsHPDamage(), amount, this.DrainLife, eventid, this.Info.LifeStealFeedback);
			}
			if (this.DrainRepair != 0f)
			{
				this.DrainRepairCheck(this.Combat, taker, mod, amount, this.DrainRepair, eventid);
			}
		}

		protected void DrainLifeCheck(CombatObject combat, CombatObject taker, bool isHpDamage, float amount, float drainPct, int eventid, ModifierFeedbackInfo drainLifeAuraFeedback)
		{
			if (!isHpDamage || combat.Data.HP >= (float)combat.Data.HPMax)
			{
				return;
			}
			ModifierData[] datas = this.CreateDrainData(EffectKind.HPRepair, amount, drainPct, drainLifeAuraFeedback);
			if (this.OnSelfRepair != null && this.Combat == combat)
			{
				float num = (float)combat.Data.HPMax - combat.Data.HP;
				float amount2 = Mathf.Min(num, amount * drainPct);
				this.OnSelfRepair(amount2);
			}
			combat.Controller.AddModifiers(datas, taker, -1, false);
		}

		protected void DrainRepairCheck(CombatObject combat, CombatObject taker, ModifierData mod, float amount, float amountPct, int eventid)
		{
			if (mod.Info.Effect != EffectKind.HPRepair)
			{
				return;
			}
			if (taker.Data.IsFullHP())
			{
				return;
			}
			ModifierData[] datas = this.CreateDrainData(EffectKind.HPRepair, amount, amountPct, this.Info.DrainRepairFeedback);
			combat.Controller.AddModifiers(datas, this.Combat, -1, false);
		}

		private ModifierData[] CreateDrainData(EffectKind effect, float amount, float amountPct, ModifierFeedbackInfo feedbackInfo)
		{
			ModifierInfo drainData = this._drainData;
			drainData.Effect = effect;
			drainData.Amount = amount * amountPct;
			drainData.Feedback = feedbackInfo;
			ModifierInfo[] infos = new ModifierInfo[]
			{
				drainData
			};
			return ModifierData.CreateData(infos, this.Info);
		}

		protected virtual void OnDestroy()
		{
			this.OnSelfRepair = null;
			this.ServerListenToGadgetUse = null;
			this.ServerListenToGadgetReady = null;
			this.ClientListenToEffectStarted = null;
			this.ListenToGadgetSetLevel = null;
			this.ListenToGadgetUpgradeChanged = null;
			this.RemoveListeners();
		}

		protected void OnGadgetUsed(int eventId)
		{
			this.ServerInformListenToGadgetUse();
			if (!this.Info.TurnOffOutOfCombat)
			{
				return;
			}
			if (this.Combat.PlayerController != null)
			{
				this.Combat.PlayerController.ActionExecuted(this);
			}
			if (this._onGadgetUsedModifiers == null)
			{
				return;
			}
			if (!this.Combat.IsPlayer)
			{
				return;
			}
			bool flag = false;
			GadgetSlot slot = this.Slot;
			if (slot != GadgetSlot.CustomGadget0)
			{
				if (slot != GadgetSlot.CustomGadget1)
				{
					if (slot == GadgetSlot.CustomGadget2)
					{
						flag = this.Combat.Player.Character.ApplyOnGadget2UsedModifiers;
					}
				}
				else
				{
					flag = this.Combat.Player.Character.ApplyOnGadget1UsedModifiers;
				}
			}
			else
			{
				flag = this.Combat.Player.Character.ApplyOnGadget0UsedModifiers;
			}
			if (!flag)
			{
				return;
			}
			for (int i = 0; i < this._onGadgetUsedModifiers.Length; i++)
			{
				if (this.Combat.Player.Character.OnGadgetUsedModifiers[i].LifeTime > 0f)
				{
					if (this.Cooldown < 0f)
					{
						string lifeTimeUpgrade = this.Combat.Player.Character.OnGadgetUsedModifiers[i].LifeTimeUpgrade;
						float lifeTime = this.Combat.Player.Character.OnGadgetUsedModifiers[i].LifeTime;
						this._onGadgetUsedModifiers[0].ForceLifeTime(lifeTimeUpgrade, Mathf.Min(this.Cooldown, lifeTime));
					}
				}
			}
			this.Combat.Controller.AddModifiers(this._onGadgetUsedModifiers, this.Combat, eventId, false);
		}

		protected void ServerInformListenToGadgetUse()
		{
			if (this.ServerListenToGadgetUse != null)
			{
				this.ServerListenToGadgetUse();
			}
		}

		protected void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._gadgetUpdater != null)
			{
				this._gadgetUpdater.RunGadgetUpdate();
			}
			else
			{
				this.GadgetUpdate();
			}
		}

		protected virtual void GadgetUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._blockPressedFalse = false;
			if (this._gadgetState == null)
			{
				return;
			}
			if (!this.Activated)
			{
				if (this._gadgetState.GadgetState != GadgetState.NotActive)
				{
					this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.NotActive, 0L, 0, this.CurrentHeat, 0, null);
				}
				return;
			}
			if (this.Toggled)
			{
				this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.Toggled, 0L, 0, this.CurrentHeat, 0, null);
				return;
			}
			int value = 0;
			if (this.Kind == GadgetKind.Switch)
			{
				value = this.CurrentGadgetValue;
				if (this._lastGadgetValue != this.CurrentGadgetValue)
				{
					this._lastGadgetValue = this.CurrentGadgetValue;
					this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.Cooldown, this.CurrentCooldownTime, value, this.CurrentHeat, 0, null);
					return;
				}
			}
			if (this.Kind == GadgetKind.InstantWithCharges)
			{
				if (this.ChargeCount < 1)
				{
					this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.Cooldown, this.ChargeTime, 0, this.CurrentHeat, this._gadgetState.Counter, null);
					return;
				}
				this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.Ready, this.ChargeTime, this.ChargeCount, this.CurrentHeat, this._gadgetState.Counter, null);
				if (this.ServerListenToGadgetReady != null)
				{
					this.ServerListenToGadgetReady();
				}
				return;
			}
			else
			{
				if (this.Kind == GadgetKind.Overheat)
				{
					GadgetState gadgetState = (!((GadgetOverheatUpdater)this._gadgetUpdater).IsOverheated) ? GadgetState.Ready : GadgetState.CoolingAfterOverheat;
					this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, gadgetState, this.CurrentCooldownTime, 0, this.CurrentHeat, 0, null);
					return;
				}
				if (this.CurrentCooldownTime > this.CurrentTime)
				{
					this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.Cooldown, this.CurrentCooldownTime, value, this.CurrentHeat, 0, null);
					return;
				}
				if (this.Kind == GadgetKind.Charged)
				{
					this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.Ready, 0L, 0, this.CurrentHeat, 0, null);
					return;
				}
				if (this.Pressed)
				{
					return;
				}
				if (this.Slot == GadgetSlot.CustomGadget2 && this.Kind == GadgetKind.Instant && !this.Combat._data.CanSpendEP((float)this.ActivationCost))
				{
					this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.Waiting, 0L, 0, 0f, 0, null);
					return;
				}
				this.Combat.GadgetStates.SetGadgetState(this._gadgetState, this.Slot, GadgetState.Ready, 0L, value, this.CurrentHeat, 0, null);
				if (this.ServerListenToGadgetReady != null)
				{
					this.ServerListenToGadgetReady();
				}
				return;
			}
		}

		public virtual float Cooldown
		{
			get
			{
				CombatAttributes attributes = this.Combat._data.Combat.Attributes;
				switch (this.Slot)
				{
				case GadgetSlot.CustomGadget0:
					return this._cooldown.Get() * (1f - attributes.CooldownReductionGadget0Pct) - attributes.CooldownReductionGadget0;
				case GadgetSlot.CustomGadget1:
					return this._cooldown.Get() * (1f - attributes.CooldownReductionGadget1Pct) - attributes.CooldownReductionGadget1;
				case GadgetSlot.CustomGadget2:
					return this._cooldown.Get() * (1f - attributes.CooldownReductionGadget2Pct) - attributes.CooldownReductionGadget2;
				case GadgetSlot.BoostGadget:
					return this._cooldown.Get() * (1f - attributes.CooldownReductionGadgetBPct) - attributes.CooldownReductionGadgetB;
				default:
					return this._cooldown.Get();
				}
			}
		}

		protected virtual float GetCooldownWithoutReduction()
		{
			return this._cooldown.Get();
		}

		public int ActivationCost
		{
			get
			{
				return this._activationCost.IntGet();
			}
		}

		public int ActivatedCost
		{
			get
			{
				return this._activatedCost.IntGet();
			}
		}

		public float WarmupTime
		{
			get
			{
				return this.Info.WarmupSeconds;
			}
		}

		public float LifeTime
		{
			get
			{
				return this._lifeTime.Get();
			}
		}

		public float DrainLife
		{
			get
			{
				return this._drainLife.Get();
			}
		}

		public float DrainRepair
		{
			get
			{
				return this._drainRepair.Get();
			}
		}

		public float Radius
		{
			get
			{
				return this._radius.Get();
			}
		}

		public int MaxChargeCount
		{
			get
			{
				return (int)this._maxChargeCount.Get();
			}
		}

		public float MaxChargeTime
		{
			get
			{
				return this._maxChargeTime.Get();
			}
		}

		public virtual void SetInfo(GadgetInfo info)
		{
			this.Info = info;
			this.Info.GadgetSlot = this.Slot;
			this._cooldown = new Upgradeable(this.Info.CooldownUpgrade, this.Info.Cooldown, this.Info.UpgradesValues);
			this.OverheatHeatRate = new Upgradeable(this.Info.OverheatHeatRateUpgrade, this.Info.OverheatHeatRate, this.Info.UpgradesValues);
			this.ActivationOverheatHeatCost = new Upgradeable(this.Info.ActivationOverheatHeatCostUpgrade, this.Info.ActivationOverheatHeatCost, this.Info.UpgradesValues);
			this.OverheatDelayBeforeCooling = new Upgradeable(this.Info.OverheatDelayBeforeCoolingUpgrade, this.Info.OverheatDelayBeforeCooling, this.Info.UpgradesValues);
			this.OverheatCoolingRate = new Upgradeable(this.Info.OverheatCoolingRateUpgrade, this.Info.OverheatCoolingRate, this.Info.UpgradesValues);
			this.OverheatUnblockRate = new Upgradeable(this.Info.OverheatUnblockRateUpgrade, this.Info.OverheatUnblockRate, this.Info.UpgradesValues);
			this._activationCost = new Upgradeable(this.Info.ActivationCostUpgrade, (float)this.Info.ActivationCost, info.UpgradesValues);
			this._activatedCost = new Upgradeable(this.Info.ActivatedCostUpgrade, (float)this.Info.ActivatedCost, info.UpgradesValues);
			this._lifeTime = new Upgradeable(this.Info.LifeTimeUpgrade, this.Info.LifeTime, this.Info.UpgradesValues);
			this._drainLife = new Upgradeable(this.Info.DrainLifeUpgrade, this.Info.DrainLife, this.Info.UpgradesValues);
			this._drainRepair = new Upgradeable(this.Info.DrainRepairUpgrade, this.Info.DrainRepair, this.Info.UpgradesValues);
			this._tmpSpecialDamageCharge = new Upgradeable(this.Info.TmpSpecialDamageChargeUpgrade, this.Info.TmpSpecialDamageCharge, this.Info.UpgradesValues);
			this._tmpSpecialRepairCharge = new Upgradeable(this.Info.TmpSpecialRepairChargeUpgrade, this.Info.TmpSpecialRepairCharge, this.Info.UpgradesValues);
			this._range = new Upgradeable(this.Info.RangeUpgrade, this.Info.Range, this.Info.UpgradesValues);
			this._radius = new Upgradeable(this.Info.RadiusUpgrade, this.Info.Radius, this.Info.UpgradesValues);
			this.ExtraModifier = ModifierData.CreateData(this.Info.ExtraModifier, this.Info);
			this.ExtraLifeTime = new Upgradeable(this.Info.ExtraLifeTimeUpgrade, this.Info.ExtraLifeTime, this.Info.UpgradesValues);
			this.FireNormalAndExtraEffectsTogether = new Upgradeable(this.Info.FireNormalAndExtraEffectsTogetherUpgrade, this.Info.FireNormalAndExtraEffectsTogether, this.Info.UpgradesValues);
			this.FireExtraOnEffectDeath = new Upgradeable(this.Info.FireExtraOnEffectDeathUpgrade, this.Info.FireExtraOnEffectDeath, this.Info.UpgradesValues);
			this.FireExtraOnEnterCallback = new Upgradeable(this.Info.FireExtraOnEnterCallbackUpgrade, this.Info.FireExtraOnEnterCallback, this.Info.UpgradesValues);
			this.FireEffectOnSpecificTarget = new Upgradeable(this.Info.FireEffectOnSpecificTargetUpgrade, this.Info.FireEffectOnSpecificTarget, this.Info.UpgradesValues);
			this.ReplaceFireNormalToFireExtra = new Upgradeable(this.Info.ReplaceFireNormalToFireExtraUpgrade, this.Info.ReplaceFireNormalToFireExtra, this.Info.UpgradesValues);
			this.ChosenTarget = new Upgradeable(this.Info.ChosenTargetUpgrade, (float)this.Info.ChosenTarget, this.Info.UpgradesValues);
			this._maxChargeTime = new Upgradeable(this.Info.ChargeTimeUpgrade, this.Info.ChargeTime, this.Info.UpgradesValues);
			this._maxChargeCount = new Upgradeable(this.Info.ChargeCountUpgrade, (float)this.Info.ChargeCount, this.Info.UpgradesValues);
			this.ChargeTime = (long)(this.MaxChargeTime * 1000f) + (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.ChargeCount = Mathf.Min(this.ChargeCount, this.MaxChargeCount);
			this._warmupModifiers = ModifierData.CreateData(info.WarmupDamage, info);
			this._warmupModifiersExtra = ModifierData.CreateData(info.WarmupDamageExtra, info);
			if ((this.Slot == GadgetSlot.BoostGadget || this.Slot == GadgetSlot.CustomGadget0 || this.Slot == GadgetSlot.CustomGadget1 || this.Slot == GadgetSlot.CustomGadget2) && this.Combat && this.Combat.IsPlayer)
			{
				this._onGadgetUsedModifiers = ModifierData.CreateData(this.Combat.Player.Character.OnGadgetUsedModifiers, info);
			}
			else
			{
				this._onGadgetUsedModifiers = null;
			}
			this.Upgrades = Array.ConvertAll<UpgradeInfo, GadgetBehaviour.UpgradeInstance>(info.Upgrades, (UpgradeInfo up) => new GadgetBehaviour.UpgradeInstance(info, up));
			this.InvisibleUpgrades = Array.ConvertAll<InvisibleUpgradeInfo, GadgetBehaviour.UpgradeInstance>(info.InvisibleUpgrades, (InvisibleUpgradeInfo up) => new GadgetBehaviour.UpgradeInstance(up));
			this.Kind = this.Info.Kind;
			if (this.Combat)
			{
				this.Combat.AddGadgetSlotDictionary(info.GadgetId, this.Slot);
				if (this.Combat.Dummy)
				{
					this._cursorDirectionSign = Math.Sign(Vector3.Dot(this.DummyPosition() - this.Combat.transform.position, this.Combat.transform.forward));
				}
			}
			this._gadgetState = ((!(this.Combat != null) || !(this.Combat.GadgetStates != null)) ? null : this.Combat.GadgetStates.GetGadgetState(this.Slot));
			this._gadgetUpdater = this.CreateGadgetUpdater();
			if (this.Slot == GadgetSlot.BoostGadget || this.Slot == GadgetSlot.EPUpgrade || this.Slot == GadgetSlot.HPUpgrade || this.Slot == GadgetSlot.DmgUpgrade || this.Slot == GadgetSlot.GenericGadget || this.Slot == GadgetSlot.BombGadget || this.Slot == GadgetSlot.LiftSceneryGadget || this.Slot == GadgetSlot.PassiveGadget || this.Slot == GadgetSlot.TrailGadget || this.Slot == GadgetSlot.OutOfCombatGadget || this.Slot == GadgetSlot.RespawnGadget || this.Slot == GadgetSlot.CustomGadget0 || this.Slot == GadgetSlot.CustomGadget1 || this.Slot == GadgetSlot.CustomGadget2 || this.Slot == GadgetSlot.SprayGadget || this.Slot == GadgetSlot.GridHighlightGadget)
			{
				this.Activate();
			}
			this.RegisterListeners();
			this._drainData.LifeStealProvider = this.Combat;
		}

		public virtual void Activate()
		{
			this.Activated = true;
		}

		protected virtual void SetLevel(string upgradeName, int level)
		{
			this._cooldown.SetLevel(upgradeName, level);
			this.OverheatHeatRate.SetLevel(upgradeName, level);
			this.ActivationOverheatHeatCost.SetLevel(upgradeName, level);
			this.OverheatDelayBeforeCooling.SetLevel(upgradeName, level);
			this.OverheatCoolingRate.SetLevel(upgradeName, level);
			this.OverheatUnblockRate.SetLevel(upgradeName, level);
			this._activationCost.SetLevel(upgradeName, level);
			this._activatedCost.SetLevel(upgradeName, level);
			this._lifeTime.SetLevel(upgradeName, level);
			this._drainLife.SetLevel(upgradeName, level);
			this._drainRepair.SetLevel(upgradeName, level);
			this._tmpSpecialDamageCharge.SetLevel(upgradeName, level);
			this._tmpSpecialRepairCharge.SetLevel(upgradeName, level);
			this._range.SetLevel(upgradeName, level);
			this._radius.SetLevel(upgradeName, level);
			this.ExtraModifier.SetLevel(upgradeName, level);
			this.ExtraLifeTime.SetLevel(upgradeName, level);
			this.FireNormalAndExtraEffectsTogether.SetLevel(upgradeName, level);
			this.FireExtraOnEffectDeath.SetLevel(upgradeName, level);
			this.FireEffectOnSpecificTarget.SetLevel(upgradeName, level);
			this.ReplaceFireNormalToFireExtra.SetLevel(upgradeName, level);
			this.ChosenTarget.SetLevel(upgradeName, level);
			this._maxChargeCount.SetLevel(upgradeName, level);
			this._maxChargeTime.SetLevel(upgradeName, level);
			if (this.ListenToGadgetSetLevel != null)
			{
				this.ListenToGadgetSetLevel(this, upgradeName, level);
			}
		}

		public int GetLevel(string upgradeName)
		{
			for (int i = 0; i < this.Upgrades.Length; i++)
			{
				GadgetBehaviour.UpgradeInstance upgradeInstance = this.Upgrades[i];
				if (string.CompareOrdinal(upgradeInstance.Info.Name, upgradeName) == 0)
				{
					return upgradeInstance.Level;
				}
			}
			for (int j = 0; j < this.InvisibleUpgrades.Length; j++)
			{
				GadgetBehaviour.UpgradeInstance upgradeInstance2 = this.InvisibleUpgrades[j];
				if (string.CompareOrdinal(upgradeInstance2.InvisibleInfo.Name, upgradeName) == 0)
				{
					return upgradeInstance2.Level;
				}
			}
			return 0;
		}

		public long GetNumberOfUpgrades()
		{
			return this.Upgrades.LongLength;
		}

		public int GetUpgradeLevel(long nUpgradeIndex)
		{
			return this.Upgrades[(int)(checked((IntPtr)nUpgradeIndex))].Level;
		}

		public float GetUpgradeValue(string upgradeName, int level)
		{
			for (int i = 0; i < this.Info.UpgradesValues.Length; i++)
			{
				UpgradeableValue upgradeableValue = this.Info.UpgradesValues[i];
				if (string.Compare(this.Info.UpgradesValues[i].Name, upgradeName) == 0)
				{
					return upgradeableValue.Values[level];
				}
			}
			return 0f;
		}

		public bool IsUpgradeInShop(string upgradeName)
		{
			for (int i = 0; i < this.Upgrades.Length; i++)
			{
				if (string.Equals(this.Upgrades[i].Info.Name, upgradeName, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GadgetBehaviour.OnGadgetUpraded ListenToGadgetSetLevel;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GadgetBehaviour.OnGadgetUpraded ListenToGadgetUpgradeChanged;

		public void Upgrade(string upgradeName)
		{
			GadgetBehaviour.UpgradeInstance upgradeInstance = this.GetUpgradeInstance(upgradeName);
			if (upgradeInstance == null)
			{
				GadgetBehaviour.Log.WarnFormat("Trying to upgrade unknown name={0} gadget={1}", new object[]
				{
					upgradeName,
					this.Info.Name
				});
				return;
			}
			upgradeInstance.LevelUp();
			this.SetLevel(upgradeName, upgradeInstance.Level);
			if (this.ListenToGadgetUpgradeChanged != null)
			{
				this.ListenToGadgetUpgradeChanged(this, upgradeName, upgradeInstance.Level);
			}
			this._levelDispatcher.Update(this.Combat.Id.ObjId, this.Slot, upgradeName, upgradeInstance.Level);
			this.SetExternalUpgradeLevel(upgradeInstance, true);
		}

		public bool Downgrade(string upgradeName)
		{
			GadgetBehaviour.UpgradeInstance upgradeInstance = this.GetUpgradeInstance(upgradeName);
			if (upgradeInstance == null)
			{
				GadgetBehaviour.Log.WarnFormat("Trying to downgrade unknown name={0} gadget={1}", new object[]
				{
					upgradeName,
					this.Info.Name
				});
				return false;
			}
			if (upgradeInstance.Level == 0)
			{
				return false;
			}
			upgradeInstance.LevelDown();
			this.SetLevel(upgradeName, upgradeInstance.Level);
			if (this.ListenToGadgetUpgradeChanged != null)
			{
				this.ListenToGadgetUpgradeChanged(this, upgradeName, upgradeInstance.Level);
			}
			this._levelDispatcher.Update(this.Combat.Id.ObjId, this.Slot, upgradeName, upgradeInstance.Level);
			this.SetExternalUpgradeLevel(upgradeInstance, false);
			return true;
		}

		public GadgetBehaviour.UpgradeInstance GetUpgradeInstance(string upgradeName)
		{
			GadgetBehaviour.UpgradeInstance upgradeInstance = Array.Find<GadgetBehaviour.UpgradeInstance>(this.Upgrades, (GadgetBehaviour.UpgradeInstance x) => string.CompareOrdinal(x.Info.Name, upgradeName) == 0);
			if (upgradeInstance == null)
			{
				upgradeInstance = Array.Find<GadgetBehaviour.UpgradeInstance>(this.InvisibleUpgrades, (GadgetBehaviour.UpgradeInstance x) => string.CompareOrdinal(x.InvisibleInfo.Name, upgradeName) == 0);
				if (upgradeInstance == null)
				{
					return null;
				}
			}
			return upgradeInstance;
		}

		private void SetExternalUpgradeLevel(GadgetBehaviour.UpgradeInstance upgrade, bool isUpgrade)
		{
			ExternalUpgrade[] externalUpgrades;
			if (upgrade.Info == null && upgrade.InvisibleInfo != null)
			{
				externalUpgrades = upgrade.InvisibleInfo.ExternalUpgrades;
			}
			else
			{
				externalUpgrades = upgrade.Info.ExternalUpgrades;
			}
			if (externalUpgrades != null)
			{
				foreach (ExternalUpgrade externalUpgrade in externalUpgrades)
				{
					GadgetBehaviour gadget = this.Combat.GetGadget(externalUpgrade.GadgetSlot);
					if (externalUpgrade.invisible)
					{
						gadget.SetLevel(externalUpgrade.UpgradeName, upgrade.Level);
					}
					else if (isUpgrade)
					{
						gadget.Upgrade(externalUpgrade.UpgradeName);
					}
					else
					{
						gadget.Downgrade(externalUpgrade.UpgradeName);
					}
				}
			}
		}

		public void ClientSetLevel(string upgradeName, int level)
		{
			GadgetBehaviour.UpgradeInstance upgradeInstance = Array.Find<GadgetBehaviour.UpgradeInstance>(this.Upgrades, (GadgetBehaviour.UpgradeInstance x) => string.CompareOrdinal(x.Info.Name, upgradeName) == 0);
			if (upgradeInstance == null)
			{
				upgradeInstance = Array.Find<GadgetBehaviour.UpgradeInstance>(this.InvisibleUpgrades, (GadgetBehaviour.UpgradeInstance x) => string.CompareOrdinal(x.InvisibleInfo.Name, upgradeName) == 0);
			}
			if (upgradeInstance == null)
			{
				BitLogger log = GadgetBehaviour.Log;
				string format = "Bot={1} upgrade={0} not found on Gadget={2} Upgrades={3}";
				object[] array = new object[4];
				array[0] = upgradeName;
				array[1] = base.Id.ObjId;
				array[2] = this.Info.Name;
				array[3] = Arrays.ToStringWithComma(Array.ConvertAll<GadgetBehaviour.UpgradeInstance, string>(this.Upgrades, (GadgetBehaviour.UpgradeInstance x) => x.Info.Name));
				log.WarnFormat(format, array);
				return;
			}
			if (this.Slot == GadgetSlot.CustomGadget0 && level > 0)
			{
				this.Combat.InstanceSelected = upgradeName;
			}
			bool isUpgrade = upgradeInstance.Level < level;
			upgradeInstance.Level = level;
			this.SetLevel(upgradeName, level);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.SetExternalUpgradeLevel(upgradeInstance, isUpgrade);
			}
		}

		public void ActivateSponsor(int level)
		{
			if (level > 0)
			{
				this.Activate();
				level--;
				for (int i = 0; i < this.Upgrades.Length; i++)
				{
					this.Upgrades[i].Level = level;
				}
				this.SetLevel(string.Empty, level);
			}
		}

		protected ModifierData[] AttachedDamage
		{
			get
			{
				return this._attachedDamage ?? ModifierData.EmptyArray;
			}
		}

		public virtual void AttachDamage(ModifierData[] damage)
		{
			this._attachedDamage = damage;
		}

		protected void SetTargetAndDirection(Vector3 direction, EffectEvent pData)
		{
			pData.Target = direction.normalized;
			pData.Direction = this.CalcDirection(pData.Origin, pData.Origin + pData.Target);
		}

		protected void SetTargetAndDirection(EffectEvent pData)
		{
			if (pData.EffectInfo.ShotPosAndDir.UseTarget)
			{
				pData.Target = this.Target;
				pData.Direction = this.CalcDirection(this.Combat.transform.position, pData.Target);
			}
			else
			{
				pData.Direction = this.DummyForward(pData.EffectInfo);
				pData.Direction.y = 0f;
				pData.Direction.Normalize();
				pData.Target = pData.Origin + pData.Direction * pData.Range;
			}
			if (pData.EffectInfo.ShotPosAndDir.UseTargetAsOrigin)
			{
				pData.Origin = pData.Target;
			}
		}

		public Vector3 CalcDirection(Vector3 from, Vector3 to)
		{
			Vector3 vector = HMMMathUtils.CalcDirectionXZ(from, to);
			if (vector.sqrMagnitude < 0.01f)
			{
				vector = this.Parent.transform.forward;
				vector.y = 0f;
				if (vector.sqrMagnitude < 0.01f)
				{
					vector = Vector3.forward;
				}
			}
			return vector.normalized;
		}

		protected EffectEvent GetEffectEvent(FXInfo effect)
		{
			EffectEvent effectEvent = new EffectEvent();
			effectEvent.SourceGadget = this;
			effectEvent.SourceCombat = this.Combat;
			effectEvent.SourceId = this.Parent.ObjId;
			effectEvent.SourceSlot = this.Slot;
			effectEvent.TagMask = this.GetTagMask();
			effectEvent.CopyInfo(effect);
			return effectEvent;
		}

		public static BaseFX GetBaseFx(int effectId)
		{
			return GameHubBehaviour.Hub.Events.Effects.GetBaseFx(effectId);
		}

		public virtual void EditorReset()
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			for (int i = 0; i < this.Upgrades.Length; i++)
			{
				dictionary[this.Upgrades[i].Info.Name] = this.Upgrades[i].Level;
			}
			this.SetInfo(this.Info);
			foreach (KeyValuePair<string, int> keyValuePair in dictionary)
			{
				this.ClientSetLevel(keyValuePair.Key, keyValuePair.Value);
			}
		}

		protected virtual void RegisterListeners()
		{
			if (!this.Combat)
			{
				return;
			}
			if (this.Info.FireEffectOnGadgetUpgrade)
			{
				this.ListenToGadgetUpgradeChanged += this.GadgetUpgradeChanged;
			}
			if (this.Slot == GadgetSlot.CustomGadget2)
			{
				GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivery;
			}
			this.Combat.ListenToPosDamageCaused += this.DrainCheck;
			this.Combat.ListenToPosHealingCaused += this.DrainCheck;
			this.Combat.ListenToPosHealingCaused += this.TmpSpecialHpRepairRecharge;
			switch (this.Info.ActionKind)
			{
			case CombatObject.ActionKind.OnPreDamageCaused:
				this.Combat.ListenToPreDamageCaused += this.OnPreDamageCaused;
				break;
			case CombatObject.ActionKind.OnPosDamageCaused:
				this.Combat.ListenToPosDamageCaused += this.OnPosDamageCaused;
				break;
			case CombatObject.ActionKind.OnPreDamageTaken:
				this.Combat.ListenToPreDamageTaken += this.OnPreDamageTaken;
				break;
			case CombatObject.ActionKind.OnPosDamageTaken:
				this.Combat.ListenToPosDamageTaken += this.OnPosDamageTaken;
				break;
			case CombatObject.ActionKind.OnPreHealingCaused:
				this.Combat.ListenToPreHealingCaused += this.OnPreDamageCaused;
				break;
			case CombatObject.ActionKind.OnPosHealingCaused:
				this.Combat.ListenToPosHealingCaused += this.OnPosHealingCaused;
				break;
			case CombatObject.ActionKind.OnPreHealingTaken:
				this.Combat.ListenToPreHealingTaken += this.OnPreHealingTaken;
				break;
			case CombatObject.ActionKind.OnPosHealingTaken:
				this.Combat.ListenToPosHealingTaken += this.OnPosHealingTaken;
				break;
			case CombatObject.ActionKind.OnIndestructibleAlmostDied:
				this.Combat.ListenToIndestructibleAlmostDied += this.ListenToIndestructibleAlmostDied;
				break;
			}
		}

		private void GadgetUpgradeChanged(GadgetBehaviour gadget, string upgradename, int level)
		{
			this.InternalFireGadget();
		}

		private void OnBombDelivery(int causerid, TeamKind scoredteam, Vector3 deliveryPosition)
		{
			if (!this.Combat._data.CanSpendEP((float)this.ActivationCost))
			{
				return;
			}
			this._gadgetState.EffectState = EffectState.Running;
			this.Combat.GadgetStates.SetEffectState(this._gadgetState, EffectState.Idle);
		}

		protected virtual void FireEffectOnAction(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			if (this.Info.OnActionOfGadget != GadgetSlot.Any && (mod.GadgetInfo == null || this.Info.OnActionOfGadget != mod.GadgetInfo.GadgetSlot))
			{
				return;
			}
			if (mod.IsReactive)
			{
				return;
			}
			if (this.Info.FireExtraEffectOnAction)
			{
				this.FireExtraGadget(taker.Id.ObjId);
			}
			else
			{
				this.FireGadget(taker.Id.ObjId);
			}
		}

		protected virtual void OnPreDamageCaused(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			this.FireEffectOnAction(causer, taker, mod, ref amount, eventid);
		}

		protected virtual void OnPosDamageCaused(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			this.FireEffectOnAction(causer, taker, mod, ref amount, eventid);
		}

		protected virtual void OnPreDamageTaken(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			this.FireEffectOnAction(causer, taker, mod, ref amount, eventid);
		}

		protected virtual void OnPosDamageTaken(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			this.FireEffectOnAction(causer, taker, mod, ref amount, eventid);
		}

		protected virtual void OnPreHealingCaused(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			this.FireEffectOnAction(causer, taker, mod, ref amount, eventid);
		}

		protected virtual void OnPosHealingCaused(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			this.FireEffectOnAction(causer, taker, mod, ref amount, eventid);
		}

		protected virtual void OnPreHealingTaken(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			this.FireEffectOnAction(causer, taker, mod, ref amount, eventid);
		}

		protected virtual void OnPosHealingTaken(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			this.FireEffectOnAction(causer, taker, mod, ref amount, eventid);
		}

		protected virtual void ListenToIndestructibleAlmostDied(CombatObject causer, CombatObject taker, ModifierData mod, int eventId)
		{
			float num = 0f;
			this.FireEffectOnAction(causer, taker, mod, ref num, eventId);
		}

		protected virtual void RemoveListeners()
		{
			if (!this.Combat)
			{
				return;
			}
			this.ListenToGadgetUpgradeChanged -= this.GadgetUpgradeChanged;
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivery;
			this.Combat.ListenToPosDamageCaused -= this.DrainCheck;
			this.Combat.ListenToPosHealingCaused -= this.DrainCheck;
			this.Combat.ListenToPosHealingCaused -= this.TmpSpecialHpRepairRecharge;
			switch (this.Info.ActionKind)
			{
			case CombatObject.ActionKind.OnPreDamageCaused:
				this.Combat.ListenToPreDamageCaused -= this.OnPreDamageCaused;
				break;
			case CombatObject.ActionKind.OnPosDamageCaused:
				this.Combat.ListenToPosDamageCaused -= this.OnPosDamageCaused;
				break;
			case CombatObject.ActionKind.OnPreDamageTaken:
				this.Combat.ListenToPreDamageTaken -= this.OnPreDamageTaken;
				break;
			case CombatObject.ActionKind.OnPosDamageTaken:
				this.Combat.ListenToPosDamageTaken -= this.OnPosDamageTaken;
				break;
			case CombatObject.ActionKind.OnPreHealingCaused:
				this.Combat.ListenToPreHealingCaused -= this.OnPreDamageCaused;
				break;
			case CombatObject.ActionKind.OnPosHealingCaused:
				this.Combat.ListenToPosHealingCaused -= this.OnPosHealingCaused;
				break;
			case CombatObject.ActionKind.OnPreHealingTaken:
				this.Combat.ListenToPreHealingTaken -= this.OnPreHealingTaken;
				break;
			case CombatObject.ActionKind.OnPosHealingTaken:
				this.Combat.ListenToPosHealingTaken -= this.OnPosHealingTaken;
				break;
			case CombatObject.ActionKind.OnIndestructibleAlmostDied:
				this.Combat.ListenToIndestructibleAlmostDied -= this.ListenToIndestructibleAlmostDied;
				break;
			}
		}

		public Vector3 GetValidPosition(Vector3 from, Vector3 to)
		{
			return PhysicsUtilsTemp.GetValidPosition(from, to, this.Combat.BombBlocked, this.Combat.Team, this.Combat.CapsuleRadius);
		}

		public virtual float GetDps()
		{
			return -1f;
		}

		protected float GetDpsFromModifierInfoWithCustomCooldown(ModifierInfo[] modifierInfo, float cooldown)
		{
			float num = 0f;
			int i = 0;
			while (i < modifierInfo.Length)
			{
				ModifierInfo modifierInfo2 = modifierInfo[i++];
				if (modifierInfo2.Effect.IsHPDamage() && !modifierInfo2.NotForPlayers && !modifierInfo2.NotForEnemies && modifierInfo2.Amount > 0f)
				{
					if (modifierInfo2.LifeTime > 0f && modifierInfo2.TickDelta > 0f)
					{
						num += Mathf.Min(cooldown, modifierInfo2.LifeTime) * modifierInfo2.Amount / modifierInfo2.TickDelta;
					}
					else
					{
						num += modifierInfo2.Amount;
					}
				}
			}
			return num / cooldown;
		}

		protected float GetDpsFromModifierDataWithCustomCooldown(ModifierData[] modifierData, float cooldown)
		{
			float num = 0f;
			int i = 0;
			while (i < modifierData.Length)
			{
				ModifierData modifierData2 = modifierData[i++];
				if (modifierData2.Info.Effect.IsHPDamage() && !modifierData2.Info.NotForPlayers && !modifierData2.Info.NotForEnemies && modifierData2.Amount > 0f)
				{
					float num2;
					if (modifierData2.LifeTime > 0f && modifierData2.TickDelta > 0f)
					{
						num2 = Mathf.Min(cooldown, modifierData2.LifeTime) * modifierData2.Amount / modifierData2.TickDelta;
					}
					else
					{
						num2 = modifierData2.Amount;
					}
					if (modifierData2.Info.UsePower)
					{
						num2 *= 1f + this.Combat.Data.PowerPct;
					}
					num += num2;
				}
			}
			return num / cooldown;
		}

		protected float GetDpsFromModifierInfo(ModifierInfo[] modifierInfo)
		{
			return this.GetDpsFromModifierInfoWithCustomCooldown(modifierInfo, this.Cooldown);
		}

		protected float GetDpsFromModifierData(ModifierData[] modifierData)
		{
			return this.GetDpsFromModifierDataWithCustomCooldown(modifierData, this.Cooldown);
		}

		public virtual float GetRange()
		{
			return this._range.Get();
		}

		public virtual float GetRangeSqr()
		{
			return this._range.Get() * this._range.Get();
		}

		public int ExistingFiredEffectsCount()
		{
			return this.ExistingFiredEffects.Count;
		}

		protected void Cleanup()
		{
			for (int i = 0; i < this.ExistingFiredEffects.Count; i++)
			{
				int targetEventId = this.ExistingFiredEffects[i];
				EffectRemoveEvent content = new EffectRemoveEvent
				{
					TargetEventId = targetEventId,
					DestroyReason = BaseFX.EDestroyReason.Cleanup
				};
				GameHubBehaviour.Hub.Events.TriggerEvent(content);
			}
		}

		public void DestroyExistingFiredEffects()
		{
			for (int i = 0; i < this.ExistingFiredEffects.Count; i++)
			{
				int num = this.ExistingFiredEffects[i];
				BaseFX baseFx = GameHubBehaviour.Hub.Events.Effects.GetBaseFx(num);
				if (baseFx)
				{
					int targetId = -1;
					if (baseFx.Target)
					{
						targetId = baseFx.Target.ObjId;
					}
					baseFx.TriggerDefaultDestroy(targetId);
				}
				else
				{
					EffectRemoveEvent content = new EffectRemoveEvent
					{
						TargetEventId = num,
						DestroyReason = BaseFX.EDestroyReason.Gadget
					};
					GameHubBehaviour.Hub.Events.TriggerEvent(content);
				}
			}
		}

		public virtual void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.TargetEventId == this.LastWarmupId)
			{
				this.LastWarmupPosition = evt.RemoveData.Origin;
				this.LastWarmupDirection = evt.EffectData.Direction;
			}
			if (evt.RemoveData.TargetEventId == this.LastEffectId)
			{
				this.LastEffectPosition = evt.RemoveData.Origin;
			}
			if (this._gadgetUpdater != null)
			{
				this._gadgetUpdater.DestroyEffect(evt);
			}
			this.InnerOnDestroyEffect(evt);
			if (this.ExistingFiredEffects.Contains(evt.RemoveData.TargetEventId) && evt.RemoveData.TargetEventId != this.LastWarmupId)
			{
				this.OnMyEffectDestroyed(evt);
				this.ExistingFiredEffectsRemove(evt.RemoveData.TargetEventId);
				bool flag = !this.Combat.IsAlive() && this.Info.AbortFireExtraOnEffectDeathWhenCombatIsDead;
				if (this.FireExtraOnEffectDeath.BoolGet() && !this._setOfEventsFiredOnEffectDeath.Contains(evt.RemoveData.TargetEventId) && !flag)
				{
					int num = this.FireExtraGadgetOnDeath(evt);
					if (num != -1)
					{
						this._setOfEventsFiredOnEffectDeath.Add(num);
						this.ExistingFiredEffectsAdd(num);
					}
				}
			}
			this._setOfEventsFiredOnEffectDeath.Remove(evt.RemoveData.TargetEventId);
			if (this.ExistingFiredEffects.Contains(evt.RemoveData.TargetEventId))
			{
				this.ExistingFiredEffectsRemove(evt.RemoveData.TargetEventId);
			}
			if (this._gadgetState == null || !GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			if (this is FireOtherGadgetOverTime)
			{
				return;
			}
			if (this.ExistingFiredEffects.Count == 0)
			{
				this.Combat.GadgetStates.SetEffectState(this._gadgetState, EffectState.Idle);
			}
			else
			{
				this.Combat.GadgetStates.SetEffectState(this._gadgetState, EffectState.Running);
			}
		}

		protected virtual void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
		}

		protected virtual void OnMyEffectDestroyed(DestroyEffectMessage evt)
		{
		}

		public virtual void OnObjectUnspawned(UnspawnEvent evt)
		{
			this._blockPressedFalse = false;
			this.Pressed = false;
			if (this._gadgetUpdater != null)
			{
				this._gadgetUpdater.ObjectUnspawned(evt);
			}
		}

		public virtual void OnObjectSpawned(SpawnEvent evt)
		{
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			this.CurrentCooldownTime = this.CurrentTime;
			if (this._gadgetUpdater != null)
			{
				this._gadgetUpdater.ObjectSpawned(evt);
			}
		}

		public void ClientOnEffectStarted(BaseFX baseFx)
		{
			if (this.ClientListenToEffectStarted != null)
			{
				this.ClientListenToEffectStarted(baseFx);
			}
		}

		public virtual void Clear()
		{
			GadgetBehaviour.Log.DebugFormat("Clearing effects for gadget={0} car={1} count={2}", new object[]
			{
				this.Info.name,
				this.Combat.Id.ObjId,
				this.ExistingFiredEffects.Count
			});
			this.OnObjectSpawned(new SpawnEvent(this.Combat.Id.ObjId, this.Combat.Transform.position, SpawnReason.MatchStart));
			this.Cleanup();
			this.LastWarmupId = -1;
			this.LastEffectId = -1;
			this._setOfEventsFiredOnEffectDeath.Clear();
			this.ExistingFiredEffects.Clear();
		}

		public virtual bool CanDestroyEffect(ref EffectRemoveEvent destroy)
		{
			return !this.ExistingFiredEffects.Contains(destroy.TargetEventId) || this.CanDestroyMyEffect(ref destroy);
		}

		protected virtual bool CanDestroyMyEffect(ref EffectRemoveEvent destroy)
		{
			return true;
		}

		public virtual void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
		}

		public bool TestSecondClick()
		{
			return this.Info.DestroyOnSecondClick || this.Info.FireExtraOnSecondClick;
		}

		public virtual void ExecuteSecondClick()
		{
			if (this.ExistingFiredEffects.Count > 0)
			{
				if (this.Info.DestroyOnSecondClick)
				{
					this.DestroyExistingFiredEffects();
				}
				if (this.Info.FireExtraOnSecondClick)
				{
					this.InternalFireExtraGadget();
				}
			}
		}

		private void CheckTags()
		{
			if (this._tags == null)
			{
				this._tags = Array.ConvertAll<GadgetBehaviour.UpgradeInstance, string>(this.Upgrades, (GadgetBehaviour.UpgradeInstance x) => x.Info.Tag);
				Array.Sort<string>(this._tags);
			}
		}

		public string GetTags(byte tagMask)
		{
			this.CheckTags();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this._tags.Length; i++)
			{
				if (((int)tagMask & 1 << i) != 0)
				{
					stringBuilder.Append(this._tags[i]);
					stringBuilder.Append(',');
				}
			}
			return stringBuilder.ToString();
		}

		protected byte GetTagMask()
		{
			this.CheckTags();
			byte b = 0;
			for (int i = 0; i < this.Upgrades.Length; i++)
			{
				if (this.Upgrades[i].Level > 0)
				{
					b |= (byte)(1 << i);
				}
			}
			return b;
		}

		public void UpdateGadgetStateObjectHeat()
		{
			this._gadgetState.Heat = this.CurrentHeat;
		}

		public void ForcePressed()
		{
			this._isForcePressed = true;
		}

		public void ForceReleased()
		{
			this._isForcePressed = false;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GadgetBehaviour));

		[Inject]
		private IGadgetLevelDispatcher _levelDispatcher;

		public Identifiable Parent;

		public CombatObject Combat;

		[SerializeField]
		private GadgetInfo _info;

		public GadgetKind Kind;

		private bool _isForcePressed;

		[SerializeField]
		private GadgetSlot _slot;

		protected bool _blockPressedFalse;

		private bool _pressed;

		public bool PressedThisFrame;

		[HideInInspector]
		public int TargetId = -1;

		public Vector3 Dir;

		private Vector3 _target;

		public Vector3 Origin;

		private Upgradeable _cooldown;

		public Upgradeable ActivationOverheatHeatCost;

		public Upgradeable OverheatHeatRate;

		public Upgradeable OverheatDelayBeforeCooling;

		public Upgradeable OverheatCoolingRate;

		public Upgradeable OverheatUnblockRate;

		private Upgradeable _activationCost;

		private Upgradeable _activatedCost;

		private Upgradeable _lifeTime;

		private Upgradeable _drainLife;

		private Upgradeable _drainRepair;

		private Upgradeable _tmpSpecialDamageCharge;

		private Upgradeable _tmpSpecialRepairCharge;

		private Upgradeable _range;

		private Upgradeable _radius;

		private Upgradeable _maxChargeCount;

		private Upgradeable _maxChargeTime;

		private ModifierData[] _warmupModifiers;

		private ModifierData[] _warmupModifiersExtra;

		private ModifierData[] _onGadgetUsedModifiers;

		protected ModifierData[] ExtraModifier;

		protected Upgradeable ExtraLifeTime;

		protected Upgradeable FireNormalAndExtraEffectsTogether;

		protected Upgradeable FireExtraOnEffectDeath;

		private readonly HashSet<int> _setOfEventsFiredOnEffectDeath = new HashSet<int>();

		protected Upgradeable FireEffectOnSpecificTarget;

		protected Upgradeable ReplaceFireNormalToFireExtra;

		protected Upgradeable ChosenTarget;

		protected Upgradeable FireExtraOnEnterCallback;

		public int LastWarmupId;

		public int LastEffectId;

		public Vector3 LastWarmupPosition;

		public Vector3 LastWarmupDirection;

		public Vector3 LastEffectPosition;

		private List<int> ExistingFiredEffects = new List<int>(3);

		public float CurrentHeat;

		public long CurrentCooldownTime;

		public long CurrentTime;

		public bool Toggled;

		public int ChargeCount;

		public long ChargeTime = -1L;

		private int _lastGadgetValue;

		public int CurrentGadgetValue;

		private int _cursorDirectionSign;

		protected IGadgetUpdater _gadgetUpdater;

		private WaitForSeconds waitForSeconds;

		[NonSerialized]
		protected GadgetData.GadgetStateObject _gadgetState;

		public GadgetBehaviour.UpgradeInstance[] Upgrades = new GadgetBehaviour.UpgradeInstance[0];

		public GadgetBehaviour.UpgradeInstance[] InvisibleUpgrades = new GadgetBehaviour.UpgradeInstance[0];

		private bool _activated;

		private Stack<int> _targetIdsForSpecificTargetEffect;

		private List<CombatObject> m_cpoCombatObjects = new List<CombatObject>(20);

		private ModifierInfo _drainData = new ModifierInfo
		{
			IsReactive = true,
			HitOwner = true,
			FriendlyFire = true,
			NotForEnemies = false,
			NotForWards = true,
			NotForBuildings = true,
			NotFurTurrets = true
		};

		private ModifierData[] _attachedDamage;

		private string[] _tags;

		public bool debug;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache0;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache1;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache2;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache3;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache4;

		public delegate void OnDrainLifeDelegate(float amount);

		[Serializable]
		public class UpgradeInstance
		{
			public UpgradeInstance(GadgetInfo gadget, UpgradeInfo info)
			{
				this.Level = 0;
				this.Info = info;
				this.MaxLevel = int.MaxValue;
				bool flag = false;
				this.Available = true;
				for (int i = 0; i < gadget.UpgradesValues.Length; i++)
				{
					UpgradeableValue upgradeableValue = gadget.UpgradesValues[i];
					if (upgradeableValue.Name.StartsWith(this.Info.Name))
					{
						this.MaxLevel = Math.Min(this.MaxLevel, upgradeableValue.Values.Length);
						flag = true;
					}
				}
				if (!flag)
				{
					this.MaxLevel = 0;
					this.Available = false;
				}
			}

			public UpgradeInstance(InvisibleUpgradeInfo info)
			{
				this.Level = 0;
				this.InvisibleInfo = info;
				this.MaxLevel = int.MaxValue;
				this.Available = true;
			}

			public bool WasBought()
			{
				return this.Level > 0;
			}

			public void LevelUp()
			{
				this.Level++;
				if (this.Level > this.MaxLevel)
				{
					this.Level = this.MaxLevel;
				}
			}

			public void LevelDown()
			{
				this.Level--;
				if (this.Level < 0)
				{
					this.Level = 0;
				}
			}

			public int CurrentPrice()
			{
				HeavyMetalMachines.Utils.Debug.Assert(this.Info.LevelPrices.Length > 0, string.Format("Content: There is no LevelPrice for upgrade:{0}", this.Info.Name), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
				return this.Info.LevelPrices[Math.Min(this.Info.LevelPrices.Length - 1, this.Level)];
			}

			public UpgradeInfo Info;

			public InvisibleUpgradeInfo InvisibleInfo;

			public int Level;

			public int MaxLevel;

			public bool Available;
		}

		public delegate bool CriteriaFunction(Vector3 stPosition, CombatObject poCombatObject, ref float pfParameter);

		public delegate void OnGadgetUpraded(GadgetBehaviour gadget, string upgradeName, int level);
	}
}
