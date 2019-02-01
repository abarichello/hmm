using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class CombatController : GameHubBehaviour, IObjectSpawnListener, ICombatController
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event CombatController.OnInstantModifierAppliedDelegate OnInstantModifierApplied;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatController.OnStatusModifierAppliedDelegate OnStatusModifierApplied;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatController.OnModifierRenewDelegate OnModifierRenew;

		public List<int> GetAssists()
		{
			int time = GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - GameHubBehaviour.Hub.ScrapLevel.AssistDelayMillis;
			List<int> list = (from pair in this._assists
			orderby pair.Value
			where pair.Value >= time
			select pair.Key).ToList<int>();
			this.AddAll(list, this.PassiveEffectsList);
			this.AddAll(list, this.TimedEffectsList);
			this.AddAll(list, this.UnstableEffectsList);
			this.AddAll(list, this.PassiveAttrStatusList);
			this.AddAll(list, this.TimedAttrStatusList);
			this.AddAll(list, this.UnstableAttrStatusList);
			return list;
		}

		private void AddAll(List<int> objectIds, List<ModifierInstance> source)
		{
			for (int i = 0; i < source.Count; i++)
			{
				ModifierInstance modifierInstance = source[i];
				if (modifierInstance.Causer)
				{
					int objId = modifierInstance.Causer.Id.ObjId;
					if (!objectIds.Contains(objId))
					{
						if (modifierInstance.Causer.IsPlayer && modifierInstance.Causer.Team != this.Combat.Team)
						{
							objectIds.Add(objId);
						}
					}
				}
			}
		}

		private CombatData Data
		{
			get
			{
				return this.Combat.Data;
			}
		}

		private CombatAttributes Attributes
		{
			get
			{
				return this.Combat.Attributes;
			}
		}

		private CombatFeedback Feedback
		{
			get
			{
				return this.Combat.Feedback;
			}
		}

		private GadgetSlot GetGadgetSlot(CombatObject causer, ModifierData data)
		{
			if (data.GadgetInfo != null)
			{
				return causer.GetSlotByGadgetId(data.GadgetInfo.GadgetId);
			}
			return GadgetSlot.None;
		}

		public bool ConsumeEP(float epAmount)
		{
			if (!this.Data.CanSpendEP(epAmount))
			{
				return false;
			}
			this.Data.EP -= epAmount;
			if (this.ListenToEP != null)
			{
				this.ListenToEP(this.Combat, epAmount);
			}
			return true;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatController.OnEPSpent ListenToEP;

		private void Update()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this.Combat.SpawnController != null && this.Combat.SpawnController.State != SpawnController.StateType.Spawned)
			{
				return;
			}
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			int num = playbackTime - this._oldPlaybackTime;
			if (num > 0)
			{
				this.CheckApplyTimedEffects(playbackTime);
				this.CheckApplyPassiveEffects(playbackTime);
				this.CheckTimedAttrStatus(playbackTime);
			}
			this._oldPlaybackTime = playbackTime;
			this.HandleDeath();
		}

		private void HandleDeath()
		{
			if (!this.Combat.IsBomb && this.Data.HP <= 0f && !this.Attributes.CurrentStatus.HasFlag(StatusKind.Dead))
			{
				this.TriggerDeath(false, PlayerEvent.Kind.Death);
			}
			if (this.Attributes.CurrentStatus.HasFlag(StatusKind.Dead) && !this.Attributes.CurrentStatus.HasFlag(StatusKind.Indestructible))
			{
				this.TriggerDeath(false, PlayerEvent.Kind.Unspawn);
			}
		}

		private void CheckApplyTimedEffects(int matchTime)
		{
			if (this.TimedEffectsList.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < this.TimedEffectsList.Count; i++)
			{
				ModifierInstance modifierInstance = this.TimedEffectsList[i];
				if (modifierInstance.ShouldAccountDebuff())
				{
					PlayerStats stats = modifierInstance.Causer.Stats;
					stats.DebuffTime += Time.deltaTime;
				}
				if (modifierInstance.Tick(matchTime))
				{
					this.ApplyInstant(modifierInstance, modifierInstance.Causer, modifierInstance.EventId);
					if (modifierInstance.Data.Info.Tapered != TaperMode.None)
					{
						modifierInstance.TaperedTick++;
					}
				}
				if ((float)matchTime >= (float)modifierInstance.StartTime + modifierInstance.Data.LifeTime * 1000f)
				{
					this.TimedEffectsList.RemoveAt(i);
					i--;
				}
			}
		}

		private void CheckApplyPassiveEffects(int matchTime)
		{
			if (this.PassiveEffectsList.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < this.PassiveEffectsList.Count; i++)
			{
				ModifierInstance modifierInstance = this.PassiveEffectsList[i];
				if (modifierInstance.ShouldAccountDebuff())
				{
					PlayerStats stats = modifierInstance.Causer.Stats;
					stats.DebuffTime += Time.deltaTime;
				}
				if (modifierInstance.Tick(matchTime))
				{
					this.ApplyInstant(modifierInstance, modifierInstance.Causer, modifierInstance.EventId);
				}
			}
		}

		public void CheckTimedAttrStatus(int matchTime)
		{
			if (this.TimedAttrStatusList.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < this.TimedAttrStatusList.Count; i++)
			{
				ModifierInstance modifierInstance = this.TimedAttrStatusList[i];
				if (modifierInstance.ShouldAccountDebuff())
				{
					PlayerStats stats = modifierInstance.Causer.Stats;
					stats.DebuffTime += Time.deltaTime;
				}
				if ((float)matchTime < (float)modifierInstance.StartTime + modifierInstance.LifeTime * 1000f)
				{
					if (modifierInstance.Data.Info.Tapered != TaperMode.None)
					{
						if (modifierInstance.Tick(matchTime))
						{
							modifierInstance.TaperedTick++;
							this.Attributes.SetDirty();
						}
					}
				}
				else
				{
					this.TimedAttrStatusList.RemoveAt(i);
					this.Attributes.SetDirty();
					i--;
				}
			}
		}

		public void RefreshCrowdControlReduction()
		{
			float crowdControlReduction = this.Attributes.CrowdControlReduction;
			if (crowdControlReduction == 0f)
			{
				return;
			}
			for (int i = 0; i < this.TimedAttrStatusList.Count; i++)
			{
				ModifierInstance mi = this.TimedAttrStatusList[i];
				this.ApplyCrowdControl(mi, crowdControlReduction);
			}
		}

		private void ApplyCrowdControl(ModifierInstance mi, float currentCrowdControlReduction)
		{
			if (!mi.Info.Attribute.IsCrowdControl(mi.Amount) && !mi.Data.Status.IsCrowdControl())
			{
				return;
			}
			bool flag = false;
			if ((currentCrowdControlReduction > 0f && mi.CrowdControlApplied >= 0f) || (currentCrowdControlReduction < 0f && mi.CrowdControlApplied <= 0f))
			{
				if (Math.Abs(currentCrowdControlReduction) <= Math.Abs(mi.CrowdControlApplied))
				{
					return;
				}
				flag = true;
			}
			float crowdControlApplied = mi.CrowdControlApplied;
			mi.CrowdControlApplied = currentCrowdControlReduction;
			float num = (!flag) ? mi.CrowdControlApplied : (mi.CrowdControlApplied - crowdControlApplied);
			float num2 = (float)(GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - mi.StartTime) * 0.001f;
			float num3 = mi.LifeTime - num2;
			float num4 = num3 * (1f - num);
			float num5 = num2 + num4;
			mi.LifeTime = num5;
			if (mi.FeedbackId > 0)
			{
				ModifierFeedbackInstance feedback = this.Feedback.GetFeedback(mi.FeedbackId);
				if (feedback != null)
				{
					feedback.EndTime = (int)((float)feedback.StartTime + num5 * 1000f);
					this.Feedback.MarkChanged();
				}
			}
		}

		public bool CheckShouldApplyMod(ModifierData data, CombatObject causer, int eventId, bool passive)
		{
			if (!this.Combat.IsAlive() && data.Info.Effect.IgnoreOnDeath())
			{
				return false;
			}
			if (data == null)
			{
				return false;
			}
			if (!base.enabled)
			{
				return false;
			}
			if (data.Status == StatusKind.None && data.Info.Effect != EffectKind.Purge && data.Info.Effect != EffectKind.Dispel && data.Info.Attribute != AttributeBuffKind.SupressTargetTag && data.Amount == 0f)
			{
				return false;
			}
			if (!passive && data.Info.Effect == EffectKind.None && data.LifeTime == 0f)
			{
				return false;
			}
			bool flag = causer && this.Combat.Id.ObjId == causer.Id.ObjId;
			if (!data.Info.HitOwner && flag)
			{
				return false;
			}
			bool flag2 = (!causer) ? data.Info.FriendlyFire : (this.Combat.Team == causer.Team);
			return (!flag2 || flag || data.Info.FriendlyFire) && (flag2 || this.Combat.Team == TeamKind.Zero || this.Combat.IsBomb || !data.Info.NotForEnemies) && (!this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Banished) || data.Info.HitBanished) && (data.Info.HitBomb || !this.Combat.IsBomb) && (!data.Info.NotForBuildings || !this.Combat.IsBuilding) && (!data.Info.NotFurTurrets || !this.Combat.IsTurret) && (!data.Info.NotForWards || !this.Combat.IsWard) && (!data.Info.NotForCreeps || !this.Combat.IsCreep) && (!data.Info.NotForPlayers || !this.Combat.IsPlayer);
		}

		public void AddModifiers(ModifierData[] datas, ICombatObject causer, int eventId, Vector3 direction, Vector3 position, bool barrierHit)
		{
			foreach (ModifierData mod in datas)
			{
				mod.SetDirection(direction);
				mod.SetPosition(position);
			}
			this.AddModifiers(datas, causer, eventId, barrierHit);
		}

		public void AddModifiers(ModifierData[] datas, ICombatObject causer, int eventId, bool barrierHit)
		{
			if (datas == null)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			CombatObject combatObject = (CombatObject)causer;
			for (int i = 0; i < datas.Length; i++)
			{
				flag2 |= this.AddModifierInternal(datas[i], combatObject, eventId, barrierHit);
				flag |= CombatController.IsModifierFromPassiveGadget(datas[i], combatObject);
			}
			if (flag2)
			{
				this.Attributes.SetDirty();
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && causer != null && combatObject.IsPlayer && !combatObject.IsBot && causer != this.Combat && !flag)
			{
				GameHubBehaviour.Hub.afkController.AddModifier(combatObject);
			}
			this.Attributes.CheckDirty();
		}

		public void AddPassiveModifiers(ModifierData[] datas, ICombatObject causer, int eventId)
		{
			if (datas == null)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			CombatObject combatObject = (CombatObject)causer;
			for (int i = 0; i < datas.Length; i++)
			{
				flag |= this.AddPassiveModifierInternal(datas[i], combatObject, eventId);
				flag2 |= CombatController.IsModifierFromPassiveGadget(datas[i], combatObject);
			}
			if (flag)
			{
				this.Attributes.SetDirty();
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && causer != null && combatObject.IsPlayer && !combatObject.IsBot && causer != this.Combat && !flag2)
			{
				GameHubBehaviour.Hub.afkController.AddModifier(combatObject);
			}
		}

		public void RemovePassiveModifiers(ModifierData[] datas, ICombatObject causer, int eventId)
		{
			if (datas == null)
			{
				return;
			}
			for (int i = 0; i < datas.Length; i++)
			{
				this.RemovePassiveModifier(datas[i], (CombatObject)causer, eventId);
			}
		}

		public void RemoveModifiers(ModifierData[] datas, CombatObject causer, int eventId)
		{
			if (datas == null)
			{
				return;
			}
			for (int i = 0; i < datas.Length; i++)
			{
				this.RemoveModifier(datas[i], causer, eventId);
			}
		}

		public void AddModifier(ModifierData data, CombatObject causer, int eventId, bool barrierHit)
		{
			bool flag = CombatController.IsModifierFromPassiveGadget(data, causer);
			if (GameHubBehaviour.Hub.Net.IsServer() && causer != null && causer.IsPlayer && !causer.IsBot && causer != this.Combat && !flag)
			{
				GameHubBehaviour.Hub.afkController.AddModifier(causer);
			}
			bool flag2 = this.AddModifierInternal(data, causer, eventId, barrierHit);
			if (flag2)
			{
				this.Attributes.SetDirty();
			}
			this.Attributes.CheckDirty();
		}

		private static bool IsModifierFromPassiveGadget(ModifierData data, CombatObject causer)
		{
			return data.GadgetInfo != null && causer != null && causer.PassiveGadget.Info.GadgetId == data.GadgetInfo.GadgetId;
		}

		private bool AddModifierInternal(ModifierData data, CombatObject causer, int eventId, bool barrierHit)
		{
			if (!this.CheckShouldApplyMod(data, causer, eventId, false))
			{
				return false;
			}
			bool result = false;
			int causer2 = (!causer) ? -1 : causer.Id.ObjId;
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			GadgetSlot slot = GadgetSlot.None;
			if (data.GadgetInfo != null)
			{
				slot = data.GadgetInfo.GadgetSlot;
			}
			if (data.LifeTime > 0f)
			{
				ModifierInstance modifierInstance = new ModifierInstance(data, causer, this.Combat, eventId, playbackTime, data.LifeTime, barrierHit);
				float num = 0f;
				if (data.Info.Effect == EffectKind.None)
				{
					ModifierInstance modifierInstance2 = this.TimedAttrStatusList.Find(ModifierInstance.CheckSameModifier(modifierInstance));
					if (modifierInstance2 != null)
					{
						this.TimedAttrStatusList.Remove(modifierInstance2);
						num = (float)(playbackTime - modifierInstance2.StartTime);
						modifierInstance.FeedbackId = modifierInstance2.FeedbackId;
						if (this.OnModifierRenew != null)
						{
							this.OnModifierRenew(modifierInstance, causer, num, modifierInstance2.LifeTime);
						}
					}
					this.TimedAttrStatusList.Add(modifierInstance);
					if (this.OnStatusModifierApplied != null && num == 0f)
					{
						this.OnStatusModifierApplied(modifierInstance, causer, data.Info.LifeTime, eventId);
					}
					this.CheckAddAssist(causer, data);
					if (this.Attributes.CrowdControlReduction != 0f)
					{
						this.ApplyCrowdControl(modifierInstance, this.Attributes.CrowdControlReduction);
					}
					result = true;
				}
				else
				{
					ModifierInstance modifierInstance3 = this.TimedEffectsList.Find(ModifierInstance.CheckSameModifier(modifierInstance));
					if (modifierInstance3 != null)
					{
						this.TimedEffectsList.Remove(modifierInstance3);
						modifierInstance.NextTick = modifierInstance3.NextTick;
						modifierInstance.FeedbackId = modifierInstance3.FeedbackId;
					}
					this.TimedEffectsList.Add(modifierInstance);
					this.CheckAddAssist(causer, data);
				}
				if (data.Info.Feedback)
				{
					if (modifierInstance.FeedbackId != -1)
					{
						this.Feedback.Remove(modifierInstance.FeedbackId);
					}
					modifierInstance.FeedbackId = this.Feedback.Add(data.Info.Feedback, eventId, causer2, playbackTime, playbackTime + (int)(data.LifeTime * 1000f), data.BuffCharges, slot);
				}
			}
			else
			{
				ModifierInstance modifierInstance4 = (this._staticModifierInstances.Count <= 0) ? new ModifierInstance() : this._staticModifierInstances.Pop();
				modifierInstance4.SetupInstance(data, causer, this.Combat, eventId, playbackTime, data.LifeTime, barrierHit);
				this.ApplyInstant(modifierInstance4, causer, eventId);
				modifierInstance4.Data = null;
				this._staticModifierInstances.Push(modifierInstance4);
				if (data.Info.Feedback && (!barrierHit || data.Info.IgnoreBarrier))
				{
					this.Feedback.Add(data.Info.Feedback, eventId, causer2, playbackTime, playbackTime + (int)(data.Info.Feedback.LifeTime * 1000f), data.BuffCharges, slot);
				}
			}
			return result;
		}

		public void AddPassiveModifier(ModifierData data, CombatObject causer, int eventId)
		{
			bool flag = CombatController.IsModifierFromPassiveGadget(data, causer);
			if (GameHubBehaviour.Hub.Net.IsServer() && causer != null && causer.IsPlayer && !causer.IsBot && causer != this.Combat && !flag)
			{
				GameHubBehaviour.Hub.afkController.AddModifier(causer);
			}
			bool flag2 = this.AddPassiveModifierInternal(data, causer, eventId);
			if (flag2)
			{
				this.Attributes.SetDirty();
			}
		}

		private bool AddPassiveModifierInternal(ModifierData data, CombatObject causer, int eventId)
		{
			if (!this.CheckShouldApplyMod(data, causer, eventId, true))
			{
				return false;
			}
			bool result = false;
			ModifierInstance modifierInstance = new ModifierInstance(data, causer, this.Combat, eventId, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), data.LifeTime, false);
			if (data.Info.Effect == EffectKind.None)
			{
				ModifierInstance modifierInstance2 = this.PassiveAttrStatusList.Find(ModifierInstance.CheckSameModifier(modifierInstance));
				if (modifierInstance2 != null)
				{
					this.PassiveAttrStatusList.Remove(modifierInstance2);
					modifierInstance.FeedbackId = modifierInstance2.FeedbackId;
				}
				result = true;
				this.PassiveAttrStatusList.Add(modifierInstance);
				if (this.OnStatusModifierApplied != null)
				{
					this.OnStatusModifierApplied(modifierInstance, causer, data.Amount, eventId);
				}
				this.CheckAddAssist(causer, data);
			}
			else
			{
				ModifierInstance modifierInstance3 = this.PassiveEffectsList.Find(ModifierInstance.CheckSameModifier(modifierInstance));
				if (modifierInstance3 != null)
				{
					this.PassiveEffectsList.Remove(modifierInstance3);
					modifierInstance.NextTick = modifierInstance3.NextTick;
					modifierInstance.FeedbackId = modifierInstance3.FeedbackId;
				}
				this.PassiveEffectsList.Add(modifierInstance);
				this.CheckAddAssist(causer, data);
			}
			if (data.Info.Feedback)
			{
				if (modifierInstance.FeedbackId != -1)
				{
					this.Feedback.Remove(modifierInstance.FeedbackId);
				}
				GadgetSlot slot = GadgetSlot.None;
				if (data.GadgetInfo != null)
				{
					slot = data.GadgetInfo.GadgetSlot;
				}
				modifierInstance.FeedbackId = this.Feedback.Add(data.Info.Feedback, eventId, (!causer) ? -1 : causer.Id.ObjId, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), -1, data.BuffCharges, slot);
			}
			return result;
		}

		public void RemovePassiveModifier(ModifierData data, CombatObject causer, int eventId)
		{
			int num = -1;
			if (data.Info.Effect != EffectKind.None)
			{
				ModifierInstance modifierInstance = this.PassiveEffectsList.Find(ModifierInstance.CheckSameModifier(data, causer));
				if (modifierInstance != null)
				{
					this.PassiveEffectsList.Remove(modifierInstance);
					num = modifierInstance.FeedbackId;
					this.CheckAddAssist(causer, data);
				}
			}
			else
			{
				ModifierInstance modifierInstance2 = this.PassiveAttrStatusList.Find(ModifierInstance.CheckSameModifier(data, causer));
				if (modifierInstance2 != null)
				{
					this.PassiveAttrStatusList.Remove(modifierInstance2);
					num = modifierInstance2.FeedbackId;
					this.Attributes.SetDirty();
					this.CheckAddAssist(causer, data);
				}
			}
			if (num == -1)
			{
				return;
			}
			this.Feedback.Remove(num);
		}

		public void RemoveModifier(ModifierData data, CombatObject causer, int eventId)
		{
			int num = -1;
			if (data.Info.Effect != EffectKind.None)
			{
				ModifierInstance modifierInstance = this.TimedEffectsList.Find(ModifierInstance.CheckSameModifier(data, causer));
				if (modifierInstance != null)
				{
					this.TimedEffectsList.Remove(modifierInstance);
					num = modifierInstance.FeedbackId;
					this.CheckAddAssist(causer, data);
				}
			}
			else
			{
				ModifierInstance modifierInstance2 = this.TimedAttrStatusList.Find(ModifierInstance.CheckSameModifier(data, causer));
				if (modifierInstance2 != null)
				{
					this.TimedAttrStatusList.Remove(modifierInstance2);
					num = modifierInstance2.FeedbackId;
					this.Attributes.SetDirty();
					this.CheckAddAssist(causer, data);
				}
			}
			if (num == -1)
			{
				return;
			}
			this.Feedback.Remove(num);
		}

		private void ApplyInstant(ModifierInstance mod, CombatObject causer, int eventId)
		{
			if (!string.IsNullOrEmpty(mod.Info.Tag) && this.Attributes.SupressedTags.Contains(mod.Info.Tag))
			{
				return;
			}
			if (mod.BarrierHit && !mod.Info.IgnoreBarrier)
			{
				this.Combat.BarrierHit(causer, mod, eventId);
				return;
			}
			bool flag = this.Data.IsAlive();
			float num = this.Data.HP;
			float num2 = this.Data.EP;
			bool flag2 = causer != null;
			float amount = mod.Amount;
			ModifierInfo info = mod.Info;
			ModifierData data = mod.Data;
			if (info.IsPercent)
			{
				EffectKind effect = info.Effect;
				if (effect != EffectKind.HPRepair)
				{
					if (effect == EffectKind.EPRepair)
					{
						amount *= (float)this.Data.EPMax;
					}
				}
				else
				{
					amount *= (float)this.Data.HPMax;
				}
			}
			if (info.Effect.IsHPDamage())
			{
				float num3 = 0f;
				EffectKind effect2 = info.Effect;
				if (effect2 != EffectKind.HPPureDamage && effect2 != EffectKind.HPPureDamageNL)
				{
					if (effect2 != EffectKind.HPLightDamage)
					{
						if (effect2 == EffectKind.HPHeavyDamage)
						{
							num3 = (float)this.Combat.Data.HPHeavyArmorFinal * this.Combat.Data.Info.ArmorModifier;
						}
					}
					else
					{
						num3 = (float)this.Combat.Data.HPLightArmorFinal * this.Combat.Data.Info.ArmorModifier;
					}
				}
				else
				{
					num3 = (float)this.Combat.Data.HPPureArmor * this.Combat.Data.Info.ArmorModifier;
				}
				float num4 = 0f;
				float num5 = 0f;
				if (flag2)
				{
					EffectKind effect3 = info.Effect;
					if (effect3 != EffectKind.HPPureDamage && effect3 != EffectKind.HPPureDamageNL)
					{
						if (effect3 != EffectKind.HPLightDamage)
						{
							if (effect3 == EffectKind.HPHeavyDamage)
							{
								num4 = causer.Attributes.HPHeavyDamagePct;
								num5 = causer.Attributes.HPHeavyDamage;
							}
						}
						else
						{
							num4 = causer.Attributes.HPLightDamagePct;
							num5 = causer.Attributes.HPLightDamage;
						}
					}
					else
					{
						num4 = causer.Attributes.HPPureDamagePct;
						num5 = causer.Attributes.HPPureDamage;
					}
				}
				amount = (amount * (1f + num4) + num5) * (1f - num3);
			}
			float num6 = amount;
			if (info.Effect == EffectKind.HPRepair)
			{
				if (flag2)
				{
					causer.PreHealingCaused(data, this.Combat, ref amount, eventId);
				}
				this.Combat.PreHealingTaken(data, causer, ref amount, eventId);
			}
			if (flag2)
			{
				causer.PreDamageCaused(data, this.Combat, ref amount, eventId);
			}
			this.Combat.PreDamageTaken(data, causer, ref amount, eventId);
			bool flag3 = false;
			bool flag4 = false;
			float num7 = 0f;
			float hptemp = this.Data.HPTemp;
			EffectKind effect4 = info.Effect;
			switch (effect4)
			{
			case EffectKind.HPPureDamage:
			case EffectKind.HPLightDamage:
			case EffectKind.HPHeavyDamage:
			case EffectKind.HPGodDamage:
			{
				float num8 = this.Data.HP + this.Data.HPTemp;
				float num9;
				float num10;
				this.TakeDamageToHpTemp(amount, out num9, out num10);
				num10 = Mathf.Min(num10, this.Data.HP);
				num = this.Data.HP - num10;
				flag3 = true;
				flag4 = true;
				num7 = num10 + num9;
				num6 = num7;
				break;
			}
			case EffectKind.HPPureDamageNL:
			{
				float num11;
				float num12;
				this.TakeDamageToHpTemp(amount, out num11, out num12);
				num12 = Mathf.Min(num12, this.Data.HP - 1f);
				num12 = Mathf.Max(num12, 0f);
				num = this.Data.HP - num12;
				flag3 = true;
				flag4 = true;
				num7 = num11 + num12;
				num6 = num7;
				break;
			}
			case EffectKind.HPRepair:
			{
				if (this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.HpUnhealable) || this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Dead) || this.Data.HP <= 0f)
				{
					amount = 0f;
					num6 = 0f;
					goto IL_11EE;
				}
				amount -= amount * ((float)this.Combat.Data.HPRepairArmorFinal * this.Combat.Data.Info.ArmorModifier);
				amount = Mathf.Min(amount, (float)this.Data.HPMax - this.Data.HP);
				num = this.Data.HP + amount;
				num6 = amount;
				CombatObject combatObject = (!mod.Info.LifeStealProvider) ? causer : mod.Info.LifeStealProvider;
				this.Combat.PosRepairTaken(mod.Data, combatObject, num6, eventId);
				if (combatObject)
				{
					combatObject.PosRepairCaused(mod.Data, this.Combat, num6, eventId);
				}
				flag4 = true;
				num7 = num6;
				break;
			}
			case EffectKind.EPDmg:
				num2 = this.Data.EP - amount;
				if (num2 <= 0f)
				{
					num2 = 0f;
				}
				this.Data.EP = num2;
				goto IL_11EE;
			case EffectKind.EPRepair:
				num2 = this.Data.EP + amount;
				if (num2 > (float)this.Data.EPMax)
				{
					num2 = (float)this.Data.EPMax;
				}
				this.Data.EP = num2;
				goto IL_11EE;
			default:
				if (effect4 == EffectKind.None)
				{
					goto IL_11EE;
				}
				break;
			case EffectKind.Purge:
				this.PurgeOrDispelMods((ModifierInstance instance) => instance.IsPurgeable && amount != 0f);
				goto IL_11EE;
			case EffectKind.CooldownRepair:
			{
				int num13 = (int)(amount * 1000f);
				switch (info.TargetGadget)
				{
				case TargetGadget.Gadget0:
					this.Combat.CustomGadget0.CurrentCooldownTime -= (long)num13;
					break;
				case TargetGadget.Gadget1:
					this.Combat.CustomGadget1.CurrentCooldownTime -= (long)num13;
					break;
				case TargetGadget.Gadgets01:
					this.Combat.CustomGadget0.CurrentCooldownTime -= (long)num13;
					this.Combat.CustomGadget1.CurrentCooldownTime -= (long)num13;
					break;
				case TargetGadget.Gadget2:
					this.Combat.CustomGadget2.CurrentCooldownTime -= (long)num13;
					break;
				case TargetGadget.Gadgets12:
					this.Combat.CustomGadget1.CurrentCooldownTime -= (long)num13;
					this.Combat.CustomGadget2.CurrentCooldownTime -= (long)num13;
					break;
				case TargetGadget.GadgetBoost:
					this.Combat.BoostGadget.CurrentCooldownTime -= (long)num13;
					break;
				case TargetGadget.All:
					this.Combat.CustomGadget0.CurrentCooldownTime -= (long)num13;
					this.Combat.CustomGadget1.CurrentCooldownTime -= (long)num13;
					this.Combat.CustomGadget2.CurrentCooldownTime -= (long)num13;
					this.Combat.BoostGadget.CurrentCooldownTime -= (long)num13;
					break;
				}
				this.OnFireModifierEvent(causer, num7, mod.GetDirection(), mod.Position, mod, info.Effect, this.GetGadgetSlot(causer, data));
				goto IL_11EE;
			}
			case EffectKind.Impulse:
			{
				Vector3 direction = mod.GetDirection();
				if (this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Unstoppable) && causer != this.Combat)
				{
					this.OnFireModifierEvent(causer, 0f, direction, Vector3.zero, mod, info.Effect, this.GetGadgetSlot(causer, data));
					goto IL_11EE;
				}
				if (this.Combat.Movement)
				{
					this.Combat.Movement.Push(direction, info.IsPercent, 0f, false);
				}
				this.OnFireModifierEvent(causer, amount, direction, Vector3.zero, mod, info.Effect, this.GetGadgetSlot(causer, data));
				goto IL_11EE;
			}
			case EffectKind.Dispel:
				this.PurgeOrDispelMods((ModifierInstance instance) => instance.IsDispellable && amount != 0f);
				goto IL_11EE;
			case EffectKind.Boost:
				if (this.Combat.Movement != null && this.Combat.Movement is CarMovement)
				{
					(this.Combat.Movement as CarMovement).Boost(amount, info.IsPercent);
				}
				goto IL_11EE;
			case EffectKind.OverheatRepair:
				switch (info.TargetGadget)
				{
				case TargetGadget.Gadget0:
					this.Combat.CustomGadget0.CurrentHeat -= amount;
					break;
				case TargetGadget.Gadget1:
					this.Combat.CustomGadget1.CurrentHeat -= amount;
					break;
				case TargetGadget.Gadgets01:
					this.Combat.CustomGadget0.CurrentHeat -= amount;
					this.Combat.CustomGadget1.CurrentHeat -= amount;
					break;
				case TargetGadget.Gadget2:
					this.Combat.CustomGadget2.CurrentHeat -= amount;
					break;
				case TargetGadget.Gadgets12:
					this.Combat.CustomGadget1.CurrentHeat -= amount;
					this.Combat.CustomGadget2.CurrentHeat -= amount;
					break;
				case TargetGadget.GadgetBoost:
					this.Combat.BoostGadget.CurrentHeat -= amount;
					break;
				case TargetGadget.All:
					this.Combat.CustomGadget0.CurrentHeat -= amount;
					this.Combat.CustomGadget1.CurrentHeat -= amount;
					this.Combat.CustomGadget2.CurrentHeat -= amount;
					this.Combat.BoostGadget.CurrentHeat -= amount;
					break;
				}
				goto IL_11EE;
			case EffectKind.AddCharge:
				switch (info.TargetGadget)
				{
				case TargetGadget.Gadget0:
					this.Combat.CustomGadget0.ChargeCount = Mathf.Min(this.Combat.CustomGadget0.ChargeCount + (int)amount, this.Combat.CustomGadget0.MaxChargeCount);
					break;
				case TargetGadget.Gadget1:
					this.Combat.CustomGadget1.ChargeCount = Mathf.Min(this.Combat.CustomGadget1.ChargeCount + (int)amount, this.Combat.CustomGadget1.MaxChargeCount);
					break;
				case TargetGadget.Gadgets01:
					this.Combat.CustomGadget0.ChargeCount = Mathf.Min(this.Combat.CustomGadget0.ChargeCount + (int)amount, this.Combat.CustomGadget0.MaxChargeCount);
					this.Combat.CustomGadget1.ChargeCount = Mathf.Min(this.Combat.CustomGadget1.ChargeCount + (int)amount, this.Combat.CustomGadget1.MaxChargeCount);
					break;
				case TargetGadget.Gadget2:
					this.Combat.CustomGadget2.ChargeCount = Mathf.Min(this.Combat.CustomGadget2.ChargeCount + (int)amount, this.Combat.CustomGadget2.MaxChargeCount);
					break;
				case TargetGadget.Gadgets12:
					this.Combat.CustomGadget1.ChargeCount = Mathf.Min(this.Combat.CustomGadget1.ChargeCount + (int)amount, this.Combat.CustomGadget1.MaxChargeCount);
					this.Combat.CustomGadget2.ChargeCount = Mathf.Min(this.Combat.CustomGadget2.ChargeCount + (int)amount, this.Combat.CustomGadget2.MaxChargeCount);
					break;
				case TargetGadget.GadgetBoost:
					this.Combat.BoostGadget.ChargeCount = Mathf.Min(this.Combat.BoostGadget.ChargeCount + (int)amount, this.Combat.BoostGadget.MaxChargeCount);
					break;
				case TargetGadget.All:
					this.Combat.CustomGadget0.ChargeCount = Mathf.Min(this.Combat.CustomGadget0.ChargeCount + (int)amount, this.Combat.CustomGadget0.MaxChargeCount);
					this.Combat.CustomGadget1.ChargeCount = Mathf.Min(this.Combat.CustomGadget1.ChargeCount + (int)amount, this.Combat.CustomGadget1.MaxChargeCount);
					this.Combat.CustomGadget2.ChargeCount = Mathf.Min(this.Combat.CustomGadget2.ChargeCount + (int)amount, this.Combat.CustomGadget2.MaxChargeCount);
					this.Combat.BoostGadget.ChargeCount = Mathf.Min(this.Combat.BoostGadget.ChargeCount + (int)amount, this.Combat.BoostGadget.MaxChargeCount);
					break;
				}
				goto IL_11EE;
			case EffectKind.ChargeRepair:
			{
				int num14 = (int)(amount * 1000f);
				switch (info.TargetGadget)
				{
				case TargetGadget.Gadget0:
					this.Combat.CustomGadget0.ChargeTime -= (long)num14;
					break;
				case TargetGadget.Gadget1:
					this.Combat.CustomGadget1.ChargeTime -= (long)num14;
					break;
				case TargetGadget.Gadgets01:
					this.Combat.CustomGadget0.ChargeTime -= (long)num14;
					this.Combat.CustomGadget1.ChargeTime -= (long)num14;
					break;
				case TargetGadget.Gadget2:
					this.Combat.CustomGadget2.ChargeTime -= (long)num14;
					break;
				case TargetGadget.Gadgets12:
					this.Combat.CustomGadget1.ChargeTime -= (long)num14;
					this.Combat.CustomGadget2.ChargeTime -= (long)num14;
					break;
				case TargetGadget.GadgetBoost:
					this.Combat.BoostGadget.ChargeTime -= (long)num14;
					break;
				case TargetGadget.All:
					this.Combat.CustomGadget0.ChargeTime -= (long)num14;
					this.Combat.CustomGadget1.ChargeTime -= (long)num14;
					this.Combat.CustomGadget2.ChargeTime -= (long)num14;
					this.Combat.BoostGadget.ChargeTime -= (long)num14;
					break;
				}
				this.OnFireModifierEvent(causer, num7, mod.GetDirection(), mod.Position, mod, info.Effect, this.GetGadgetSlot(causer, data));
				goto IL_11EE;
			}
			case EffectKind.HPTemp:
				this.Data.SetHpTemp(this.Data.HPTemp + data.Amount);
				this.Data.SetHPTempDelay(info.Delay);
				flag4 = true;
				num7 = this.Data.HPTemp - hptemp;
				num6 = num7;
				break;
			case EffectKind.ModifyParameter:
			{
				List<CombatGadget> gadgets = this.GetGadgets(info);
				for (int i = 0; i < gadgets.Count; i++)
				{
					INumericParameter modifiableParameter = gadgets[i].GetModifiableParameter(info.ParameterName);
					modifiableParameter.SetFloatValue(gadgets[i], modifiableParameter.GetFloatValue(gadgets[i]) + info.Amount);
				}
				goto IL_11EE;
			}
			}
			bool flag5 = mod.Info.Effect == EffectKind.HPGodDamage;
			if (!this.Attributes.IsInvulnerable || flag5 || !mod.Info.Effect.IsHPDamage())
			{
				if (flag4 && num7 != 0f)
				{
					this.OnFireModifierEvent(causer, num7, mod.GetDirection(), mod.Position, mod, info.Effect, this.GetGadgetSlot(causer, data));
				}
				if (this.Data.HP > num)
				{
					this.SetDamageCausers(causer, num);
				}
				this.Data.HP = num;
			}
			IL_11EE:
			if (info.Effect == EffectKind.HPRepair)
			{
				if (flag2)
				{
					causer.PosHealingCaused(data, this.Combat, num6, eventId);
				}
				this.Combat.PosHealingTaken(data, causer, num6, eventId);
			}
			if (info.Effect.IsHPDamage())
			{
				if (flag2)
				{
					causer.PosDamageCaused(data, this.Combat, amount, eventId);
				}
				this.Combat.PosDamageTaken(data, causer, amount, eventId);
			}
			if (flag3)
			{
				this.RemoveUnstableMods();
			}
			if (flag2)
			{
				this.CheckAddAssist(causer, data);
			}
			if (this.Data.HP <= 0f && !this.Attributes.CurrentStatus.HasFlag(StatusKind.Dead))
			{
				this.Data.HP = 0f;
			}
			if (this.Data.EP <= 0f)
			{
				this.Data.EP = 0f;
			}
			if (CombatController.OnInstantModifierApplied != null)
			{
				CombatController.OnInstantModifierApplied(mod, causer, this.Combat, num6, eventId);
			}
		}

		private List<CombatGadget> GetGadgets(ModifierInfo info)
		{
			List<CombatGadget> list = new List<CombatGadget>();
			switch (info.TargetGadget)
			{
			case TargetGadget.Gadget0:
			{
				CombatGadget item;
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget0, out item))
				{
					list.Add(item);
				}
				break;
			}
			case TargetGadget.Gadget1:
			{
				CombatGadget item;
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget1, out item))
				{
					list.Add(item);
				}
				break;
			}
			case TargetGadget.Gadgets01:
			{
				CombatGadget item;
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget0, out item))
				{
					list.Add(item);
				}
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget1, out item))
				{
					list.Add(item);
				}
				break;
			}
			case TargetGadget.Gadget2:
			{
				CombatGadget item;
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget2, out item))
				{
					list.Add(item);
				}
				break;
			}
			case TargetGadget.Gadgets12:
			{
				CombatGadget item;
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget1, out item))
				{
					list.Add(item);
				}
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget2, out item))
				{
					list.Add(item);
				}
				break;
			}
			case TargetGadget.GadgetBoost:
			{
				CombatGadget item;
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.BoostGadget, out item))
				{
					list.Add(item);
				}
				break;
			}
			case TargetGadget.All:
			{
				CombatGadget item;
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget0, out item))
				{
					list.Add(item);
				}
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget1, out item))
				{
					list.Add(item);
				}
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.CustomGadget2, out item))
				{
					list.Add(item);
				}
				if (this.Combat.CustomGadgets.TryGetValue(GadgetSlot.BoostGadget, out item))
				{
					list.Add(item);
				}
				break;
			}
			}
			return list;
		}

		private void SetDamageCausers(CombatObject causer, float resultingHP)
		{
			if (causer != null)
			{
				if (resultingHP <= 0f && this._playerKiller == -1)
				{
					this._playerKiller = causer.Id.ObjId;
				}
				this._lastDamageCauser = causer.Id.ObjId;
			}
		}

		public void RemoveUnstableMods()
		{
			Func<ModifierInstance, bool> shouldRemove = (ModifierInstance instance) => instance.Data.Info.Unstable;
			this.RemoveModsFromList(this.PassiveEffectsList, shouldRemove, null);
			this.RemoveModsFromList(this.TimedEffectsList, shouldRemove, null);
			this.RemoveModsFromList(this.PassiveAttrStatusList, shouldRemove, new Action<ModifierInstance>(this.OnRemoveModifierFromList));
			this.RemoveModsFromList(this.TimedAttrStatusList, shouldRemove, new Action<ModifierInstance>(this.OnRemoveModifierFromList));
		}

		private void PurgeOrDispelMods(Func<ModifierInstance, bool> shouldRemove)
		{
			this.RemoveModsFromList(this.TimedEffectsList, shouldRemove, null);
			this.RemoveModsFromList(this.TimedAttrStatusList, shouldRemove, new Action<ModifierInstance>(this.OnRemoveModifierFromList));
			this.RemoveModsFromList(this.UnstableEffectsList, shouldRemove, null);
			this.RemoveModsFromList(this.UnstableAttrStatusList, shouldRemove, delegate(ModifierInstance instance)
			{
				this.Attributes.SetDirty();
			});
		}

		private void OnRemoveModifierFromList(ModifierInstance instance)
		{
			this.Attributes.SetDirty();
		}

		private void TakeDamageToHpTemp(float amount, out float taken, out float remaining)
		{
			float hptemp = this.Data.HPTemp;
			if (hptemp <= 0f)
			{
				taken = 0f;
				remaining = amount;
				return;
			}
			this.Data.SetHpTemp(hptemp - amount);
			bool flag = this.Data.HPTemp < 0f;
			if (flag)
			{
				remaining = Math.Abs(this.Data.HPTemp);
				taken = hptemp;
				this.Data.SetHpTemp(0f);
			}
			else
			{
				taken = hptemp - this.Data.HPTemp;
				remaining = 0f;
			}
		}

		private void RemoveModsFromList(List<ModifierInstance> modifierInstances, Func<ModifierInstance, bool> shouldRemove, Action<ModifierInstance> onRemove)
		{
			for (int i = 0; i < modifierInstances.Count; i++)
			{
				ModifierInstance modifierInstance = modifierInstances[i];
				if (shouldRemove(modifierInstance))
				{
					if (onRemove != null)
					{
						onRemove(modifierInstance);
					}
					modifierInstances.RemoveAt(i);
					if (modifierInstance.FeedbackId != -1)
					{
						this.Feedback.Remove(modifierInstance.FeedbackId);
					}
					i--;
				}
			}
		}

		private void OnFireModifierEvent(CombatObject causer, float amount, Vector3 direction, Vector3 position, ModifierInstance mod, EffectKind effectKind, GadgetSlot causerGadgetSlot)
		{
			if (mod.Info.LifeStealProvider != null)
			{
				causer = mod.Info.LifeStealProvider;
			}
			int otherId = -1;
			if (causer != null)
			{
				otherId = causer.Id.ObjId;
			}
			ModifierEvent evt = new ModifierEvent
			{
				Amount = amount,
				ObjId = this.Combat.Id.ObjId,
				Effect = effectKind,
				OtherId = otherId,
				Slot = causerGadgetSlot
			};
			this.ModifierEvents.AddData(evt);
		}

		private void CheckAddAssist(CombatObject causer, ModifierData modifierData)
		{
			if (!this.Combat.IsPlayer || !causer || !causer.IsPlayer || causer.Team == this.Combat.Team)
			{
				return;
			}
			this._assists[causer.Id.ObjId] = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (modifierData.Info.Effect.IsHPDamage())
			{
				this._lastDamageCauser = causer.Id.ObjId;
			}
		}

		public void ForceDeath()
		{
			this.Data.HP = 0f;
			this.TriggerDeath(true, PlayerEvent.Kind.Death);
		}

		private void TriggerDeath(bool forced, PlayerEvent.Kind kind)
		{
			if (this.Combat.IsPlayer)
			{
				PlayerEvent playerEvent;
				if (this.Combat.IsBot)
				{
					playerEvent = new BotAIEvent();
				}
				else
				{
					playerEvent = new PlayerEvent();
				}
				playerEvent.EventKind = kind;
				playerEvent.Reason = ((!forced) ? SpawnReason.Death : SpawnReason.Hide);
				playerEvent.PossibleKiller = ((this._playerKiller != -1) ? this._playerKiller : this._lastDamageCauser);
				playerEvent.TargetId = base.Id.ObjId;
				playerEvent.SourceEventId = -1;
				playerEvent.Location = base.transform.position;
				playerEvent.Assists = this.GetAssists();
				if (!playerEvent.Assists.Contains(playerEvent.PossibleKiller))
				{
					playerEvent.PossibleKiller = -1;
				}
				playerEvent.CauserId = playerEvent.PossibleKiller;
				if (kind == PlayerEvent.Kind.Death)
				{
					this._deathModDatas = ModifierData.CreateData(this.DeathModifiers);
					for (int i = 0; i < this._deathModDatas.Length; i++)
					{
						this.AddPassiveModifier(this._deathModDatas[i], null, -1);
					}
				}
				GameHubBehaviour.Hub.Events.TriggerEvent(playerEvent);
				return;
			}
			if (this.Combat.IsCreep)
			{
				CreepRemoveEvent content = new CreepRemoveEvent
				{
					Location = base.transform.position,
					CreepId = base.Id.ObjId,
					CauserId = this._playerKiller,
					Reason = SpawnReason.Death
				};
				GameHubBehaviour.Hub.Events.TriggerEvent(content);
				return;
			}
		}

		public void OnObjectSpawned(SpawnEvent msg)
		{
		}

		public void OnObjectUnspawned(UnspawnEvent msg)
		{
			this.Clear();
		}

		public void Clear()
		{
			if (this._deathModDatas != null)
			{
				this.RemovePassiveModifiers(this._deathModDatas, null, -1);
				this._deathModDatas = null;
			}
			this.TimedEffectsList.Clear();
			this.TimedAttrStatusList.Clear();
			this.Attributes.SetDirty();
			this._assists.Clear();
			this._playerKiller = -1;
			this._lastDamageCauser = -1;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CombatController));

		public readonly ModifierEventHolder ModifierEvents = new ModifierEventHolder();

		public List<ModifierInstance> PassiveEffectsList = new List<ModifierInstance>();

		public List<ModifierInstance> TimedEffectsList = new List<ModifierInstance>();

		public List<ModifierInstance> UnstableEffectsList = new List<ModifierInstance>();

		public List<ModifierInstance> PassiveAttrStatusList = new List<ModifierInstance>();

		public List<ModifierInstance> TimedAttrStatusList = new List<ModifierInstance>();

		public List<ModifierInstance> UnstableAttrStatusList = new List<ModifierInstance>();

		private readonly Dictionary<int, int> _assists = new Dictionary<int, int>();

		private int _playerKiller = -1;

		private int _lastDamageCauser = -1;

		private int _oldPlaybackTime;

		public ModifierInfo[] DeathModifiers;

		private ModifierData[] _deathModDatas;

		public CombatObject Combat;

		private readonly Stack<ModifierInstance> _staticModifierInstances = new Stack<ModifierInstance>();

		public delegate void OnInstantModifierAppliedDelegate(ModifierInstance mod, CombatObject causer, CombatObject target, float amount, int eventId);

		public delegate void OnStatusModifierAppliedDelegate(ModifierInstance mod, CombatObject causer, float amount, int eventId);

		public delegate void OnModifierRenewDelegate(ModifierInstance instance, CombatObject causer, float previousLifeTime, float lifeTime);

		public delegate void OnEPSpent(CombatObject combat, float amount);
	}
}
