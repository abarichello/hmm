using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class Link : GadgetBehaviour
	{
		public LinkInfo MyInfo
		{
			get
			{
				return base.Info as LinkInfo;
			}
		}

		public bool LinkActive
		{
			get
			{
				return this._currentLinkEffectId != -1;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this._damage = ModifierData.CreateData(this.MyInfo.Damage, this.MyInfo);
			this._moveSpeed = new Upgradeable(this.MyInfo.MoveSpeedUpgrade, this.MyInfo.MoveSpeed, this.MyInfo.UpgradesValues);
			this._upgCrossDamage = ModifierData.CreateData(this.MyInfo.LinkDamage, this.MyInfo);
			this._upgSnapDamage = ModifierData.CreateData(this.MyInfo.SnapDamage, this.MyInfo);
			this._upgOngoingDamage = ModifierData.CreateData(this.MyInfo.OngoingDamage, this.MyInfo);
			this._upgBoostedOngoingDamage = ModifierData.CreateData(this.MyInfo.BoostedDamage, this.MyInfo);
			this._upgLinkRange = new Upgradeable(this.MyInfo.OngoingRangeUpgrade, this.MyInfo.OngoingRange, this.MyInfo.UpgradesValues);
			this._otherLink = (this.Combat.GetGadget(this.MyInfo.OtherLink) as Link);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._damage.SetLevel(upgradeName, level);
			this._moveSpeed.SetLevel(upgradeName, level);
			this._upgCrossDamage.SetLevel(upgradeName, level);
			this._upgSnapDamage.SetLevel(upgradeName, level);
			this._upgOngoingDamage.SetLevel(upgradeName, level);
			this._upgBoostedOngoingDamage.SetLevel(upgradeName, level);
			this._upgLinkRange.SetLevel(upgradeName, level);
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			if (this.MyInfo.SearchLength > 0f)
			{
				this.TEST2();
			}
			else
			{
				this.FireProjectile();
			}
		}

		private void FireProjectile()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.Combat.Transform.position;
			effectEvent.Target = base.Target;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.Modifiers = this._damage;
			this._currentProjectileEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.OnGadgetUsed(this._currentProjectileEffectId);
		}

		private void TEST2()
		{
			float range = this.GetRange();
			float searchLength = this.MyInfo.SearchLength;
			Transform transform = this.Combat.transform;
			Vector3 position = transform.position;
			Vector3 vector = base.Target - position;
			vector.y = 0f;
			vector.Normalize();
			Vector3 vector2 = Vector3.Cross(vector, Vector3.up);
			Vector3 vector3 = vector2 * -1f;
			Vector3 inPoint = position + vector2 * (searchLength / 2f);
			Vector3 inPoint2 = position + vector3 * (searchLength / 2f);
			Plane plane = new Plane(vector3, inPoint2);
			Plane plane2 = new Plane(vector2, inPoint);
			float radius = Mathf.Sqrt(Mathf.Pow(range / 2f, 2f) + Mathf.Pow(searchLength / 2f, 2f));
			Vector3 position2 = position + vector * (Mathf.Max(range, searchLength) / 2f);
			Collider[] array = Physics.OverlapSphere(position2, radius, 1077058560);
			List<CombatObject> list = new List<CombatObject>();
			List<CombatObject> list2 = new List<CombatObject>();
			foreach (Collider collider in array)
			{
				CombatObject combat = CombatRef.GetCombat(collider);
				if (combat)
				{
					if (combat.Team == this.Combat.Team)
					{
						if (!(combat == this.Combat))
						{
							if (!combat.IsBuilding && !combat.IsTurret)
							{
								if (collider.PlaneCast(plane) != 1 && collider.PlaneCast(plane2) != 1)
								{
									List<CombatObject> list3 = (!combat.IsPlayer) ? list2 : list;
									if (!list3.Contains(combat))
									{
										list3.Add(combat);
									}
								}
							}
						}
					}
				}
			}
			int num = -1;
			float num2 = 0f;
			for (int j = 0; j < list.Count; j++)
			{
				CombatObject combatObject = list[j];
				float sqrMagnitude = (combatObject.transform.position - position).sqrMagnitude;
				if (num == -1 || sqrMagnitude < num2)
				{
					num = combatObject.Id.ObjId;
					num2 = sqrMagnitude;
				}
			}
			if (num != -1)
			{
				this.CreateLinkToTarget(num);
				return;
			}
			for (int k = 0; k < list2.Count; k++)
			{
				CombatObject combatObject2 = list2[k];
				float sqrMagnitude2 = (combatObject2.transform.position - position).sqrMagnitude;
				if (num == -1 || sqrMagnitude2 < num2)
				{
					num = combatObject2.Id.ObjId;
					num2 = sqrMagnitude2;
				}
			}
			if (num != -1)
			{
				this.CreateLinkToTarget(num);
			}
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId == this._currentProjectileEffectId)
			{
				this._currentProjectileEffectId = -1;
				if (evt.RemoveData.TargetId == -1)
				{
					return;
				}
				Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(evt.RemoveData.TargetId);
				if (!@object)
				{
					return;
				}
				this.CreateLinkToTarget(evt.RemoveData.TargetId);
			}
			else if (evt.RemoveData.TargetEventId == this._currentLinkEffectId)
			{
				if (this._otherLink)
				{
					this.BoostOff();
					this._otherLink.BoostOff();
				}
				this._currentLinkEffectId = -1;
				this._currentTarget = -1;
			}
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			if (this._currentLinkEffectId != -1)
			{
				if (this._otherLink)
				{
					this.BoostOff();
					this._otherLink.BoostOff();
				}
				this._currentLinkEffectId = -1;
				this._currentTarget = -1;
			}
		}

		private void CreateLinkToTarget(int targetId)
		{
			if (this._currentLinkEffectId != -1)
			{
				if (this._otherLink)
				{
					this.BoostOff();
					this._otherLink.BoostOff();
				}
				EffectRemoveEvent content = new EffectRemoveEvent
				{
					TargetEventId = this._currentLinkEffectId,
					TargetId = -1
				};
				this._currentLinkEffectId = -1;
				this._currentTarget = -1;
				GameHubBehaviour.Hub.Events.TriggerEvent(content);
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.LinkEffect);
			effectEvent.LifeTime = base.LifeTime;
			EffectEvent effectEvent2 = effectEvent;
			this._currentTarget = targetId;
			effectEvent2.TargetId = targetId;
			effectEvent.Range = this.MyInfo.SnapDistance;
			effectEvent.Modifiers = this._upgCrossDamage;
			effectEvent.ExtraModifiers = this._upgSnapDamage;
			this._currentLinkEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			EffectEvent effectEvent3 = base.GetEffectEvent(this.MyInfo.LinkTargetEffect);
			effectEvent3.LifeTime = base.LifeTime;
			effectEvent3.TargetId = targetId;
			effectEvent3.TargetEventId = this._currentLinkEffectId;
			effectEvent3.Range = this._upgLinkRange.Get();
			effectEvent3.Modifiers = this._upgOngoingDamage;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent3);
			if (this._otherLink && this._otherLink.LinkActive)
			{
				this.BoostOn();
				this._otherLink.BoostOn();
			}
		}

		private void BoostOn()
		{
			if (this._currentBoostEffectId != -1)
			{
				return;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.BoostEffect);
			effectEvent.TargetId = this._currentTarget;
			effectEvent.Modifiers = this._upgBoostedOngoingDamage;
			this._currentBoostEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private void BoostOff()
		{
			if (this._currentBoostEffectId == -1)
			{
				return;
			}
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentBoostEffectId,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._currentBoostEffectId = -1;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(Link));

		protected ModifierData[] _damage;

		protected Upgradeable _moveSpeed;

		protected ModifierData[] _upgCrossDamage;

		protected ModifierData[] _upgSnapDamage;

		protected ModifierData[] _upgOngoingDamage;

		protected ModifierData[] _upgBoostedOngoingDamage;

		private Upgradeable _upgLinkRange;

		private int _currentProjectileEffectId = -1;

		private int _currentLinkEffectId = -1;

		private int _currentBoostEffectId = -1;

		private Link _otherLink;

		private int _currentTarget;
	}
}
