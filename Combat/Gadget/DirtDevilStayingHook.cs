using System;
using System.Reflection;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class DirtDevilStayingHook : BasicCannon
	{
		public DirtDevilStayingHookInfo MyInfo
		{
			get
			{
				return base.Info as DirtDevilStayingHookInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgHookGoDamage = ModifierData.CreateData(this.MyInfo.HookGoDamage, this.MyInfo);
			this._upgHookStayAndBackDamage = ModifierData.CreateData(this.MyInfo.HookStayAndBackDamage, this.MyInfo);
			this._upgHookBackDamage = ModifierData.CreateData(this.MyInfo.HookBackDamage, this.MyInfo);
			base.Pressed = false;
			this.CurrentCooldownTime = 0L;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgHookGoDamage.SetLevel(upgradeName, level);
			this._upgHookStayAndBackDamage.SetLevel(upgradeName, level);
		}

		protected void Start()
		{
			this.Combat.ListenToObjectUnspawn += this.OnCasterDeath;
		}

		protected override int FireGadget()
		{
			this._currentHookStayStartTime = -1L;
			this._currentHookStayEffect = -1;
			this._currentHookBackEffect = -1;
			this._currentOwnerBackEffect = -1;
			this._currentHookStayTarget = null;
			this._currentHookStayEffectWallPosition = Vector3.zero;
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.HookGoEffect);
			effectEvent.MoveSpeed = this.MyInfo.HookGoSpeed;
			effectEvent.Range = this.MyInfo.HookGoRange;
			effectEvent.Origin = this.DummyPosition(effectEvent);
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Modifiers = this._upgHookGoDamage;
			base.SetTargetAndDirection(effectEvent);
			this._currentGoDirection = effectEvent.Direction;
			this._currentHookGoEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			DirtDevilStayingHook.Log.DebugFormat("{0} {1}", new object[]
			{
				MethodBase.GetCurrentMethod().Name,
				effectEvent.EffectInfo.Effect
			});
			return this._currentHookGoEffect;
		}

		protected override int FireExtraGadget()
		{
			if (this._currentHookStayEffect != -1)
			{
				this._currentHookBackEffect = ((!(this._currentHookStayTarget != null)) ? this.HookBackWall() : this.HookBack());
				this.DestroyCurrentHookStay();
				this.HookBackOwnerEffect(this._currentHookBackEffect);
			}
			DirtDevilStayingHook.Log.DebugFormat("{0} {1}", new object[]
			{
				MethodBase.GetCurrentMethod().Name,
				this._currentHookBackEffect
			});
			return this._currentHookBackEffect;
		}

		private void HookBackOwnerEffect(int hookBackId)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.HookBackOwnerEffect);
			effectEvent.TargetEventId = hookBackId;
			effectEvent.Origin = this.DummyPosition(effectEvent);
			this._currentOwnerBackEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(this._currentOwnerBackEffect);
			DirtDevilStayingHook.Log.DebugFormat("{0} {1}", new object[]
			{
				MethodBase.GetCurrentMethod().Name,
				effectEvent.EffectInfo.Effect
			});
		}

		private void HookStay(CombatObject targetCombatObject)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.HookStayEffect);
			effectEvent.Range = this.MyInfo.HookStayRange;
			effectEvent.LifeTime = this.MyInfo.HookStayLifeTime;
			effectEvent.Origin = this.DummyPosition(effectEvent);
			effectEvent.TargetId = targetCombatObject.Id.ObjId;
			effectEvent.Modifiers = this._upgHookStayAndBackDamage;
			effectEvent.Direction = this._currentGoDirection;
			this._currentHookStayEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this._currentHookStayStartTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._currentHookStayTarget = targetCombatObject;
			base.ExistingFiredEffectsAdd(this._currentHookStayEffect);
			DirtDevilStayingHook.Log.DebugFormat("{0} {1}", new object[]
			{
				MethodBase.GetCurrentMethod().Name,
				effectEvent.EffectInfo.Effect
			});
		}

		private void HookStayWall(Vector3 wallPosition)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.HookStayWallEffect);
			effectEvent.Range = this.MyInfo.HookStayRangeWall;
			float num = this.MyInfo.HookStayWallLifeTime;
			if (this._currentHookStayStartTime > 0L)
			{
				float num2 = (float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._currentHookStayStartTime) / 1000f;
				num -= num2;
			}
			effectEvent.LifeTime = num;
			effectEvent.Origin = wallPosition;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			effectEvent.Modifiers = this._upgHookStayAndBackDamage;
			effectEvent.Direction = this._currentGoDirection;
			this._currentHookStayEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this._currentHookStayEffectWallPosition = wallPosition;
			base.ExistingFiredEffectsAdd(this._currentHookStayEffect);
			DirtDevilStayingHook.Log.DebugFormat("{0} {1} @={2} lifetime={3}", new object[]
			{
				MethodBase.GetCurrentMethod().Name,
				effectEvent.EffectInfo.Effect,
				wallPosition,
				num
			});
		}

		private int HookBack()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.HookBackEffect);
			effectEvent.MoveSpeed = this.MyInfo.HookBackMoveSpeed;
			effectEvent.Modifiers = this._upgHookBackDamage;
			effectEvent.LifeTime = base.LifeTime;
			effectEvent.TargetId = this._currentHookStayTarget.Id.ObjId;
			effectEvent.Origin = this._currentHookStayTarget.Transform.position;
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(num);
			DirtDevilStayingHook.Log.DebugFormat("{0} {1}", new object[]
			{
				MethodBase.GetCurrentMethod().Name,
				effectEvent.EffectInfo.Effect
			});
			return num;
		}

		private int HookBackWall()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.HookBackWallEffect);
			effectEvent.MoveSpeed = this.MyInfo.HookBackMoveSpeed;
			effectEvent.Modifiers = this._upgHookStayAndBackDamage;
			effectEvent.LifeTime = base.LifeTime;
			effectEvent.TargetId = -1;
			effectEvent.Origin = this._currentHookStayEffectWallPosition;
			effectEvent.Target = Vector3.zero;
			effectEvent.Direction = this._currentGoDirection;
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(num);
			DirtDevilStayingHook.Log.DebugFormat("{0} {1} {2}", new object[]
			{
				MethodBase.GetCurrentMethod().Name,
				effectEvent.EffectInfo.Effect,
				effectEvent.Origin
			});
			return num;
		}

		private void HookFail(Vector3 initialPosition)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.HookFailEffect);
			effectEvent.MoveSpeed = this.MyInfo.HookFailMoveSpeed;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			effectEvent.Origin = initialPosition;
			effectEvent.Direction = base.CalcDirection(this.Combat.transform.position, effectEvent.Origin);
			int effectID = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(effectID);
			DirtDevilStayingHook.Log.DebugFormat("{0} {1}", new object[]
			{
				MethodBase.GetCurrentMethod().Name,
				effectEvent.EffectInfo.Effect
			});
		}

		private void OnCasterDeath(CombatObject obj, UnspawnEvent msg)
		{
			if (this._currentHookGoEffect != -1)
			{
				this.DestroyCurrentHookGo();
			}
			if (this._currentHookStayEffect != -1)
			{
				this.DestroyCurrentHookStay();
			}
		}

		private void DestroyCurrentHookGo()
		{
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentHookGoEffect,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._currentHookGoEffect = -1;
		}

		private void DestroyCurrentHookStay()
		{
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentHookStayEffect,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._currentHookStayEffect = -1;
			this._currentHookStayTarget = null;
		}

		private void DestroyCurrentHookBack()
		{
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentHookBackEffect,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._currentHookBackEffect = -1;
		}

		private void DestroyCurrentHookBackOwner()
		{
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentOwnerBackEffect,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._currentOwnerBackEffect = -1;
		}

		protected override void GadgetUpdate()
		{
			base.GadgetUpdate();
			if (this._currentHookBackEffect != -1 && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.Combat))
			{
				CombatMovement bombMovement = GameHubBehaviour.Hub.BombManager.BombMovement;
				if (bombMovement.IsCollidingWithScenery && Vector3.Dot(this._currentGoDirection, bombMovement.SceneryNormal) < 0f)
				{
					this.DestroyCurrentHookBack();
				}
			}
		}

		protected override void OnMyEffectDestroyed(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.TargetEventId == this._currentHookGoEffect)
			{
				this._currentHookGoEffect = -1;
				if (evt.RemoveData.TargetId == -1)
				{
					this.HookStayWall(evt.RemoveData.Origin);
				}
				else
				{
					Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(evt.RemoveData.TargetId);
					if (!@object)
					{
						return;
					}
					CombatObject component = @object.GetComponent<CombatObject>();
					if (!component)
					{
						return;
					}
					this.HookStay(component);
				}
			}
			else if (evt.RemoveData.TargetEventId == this._currentHookStayEffect)
			{
				this._currentHookStayEffect = -1;
				this._currentHookStayTarget = null;
				if (this._currentHookStayStartTime > 0L && (float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._currentHookStayStartTime) < Mathf.Min(this.MyInfo.HookStayLifeTime, this.MyInfo.HookStayWallLifeTime) * 1000f && this._currentHookStayEffectWallPosition == Vector3.zero)
				{
					this.HookStayWall(evt.RemoveData.Origin);
				}
				else
				{
					this.HookFail(evt.RemoveData.Origin);
				}
			}
			else if (evt.RemoveData.TargetEventId == this._currentOwnerBackEffect)
			{
				this.DestroyCurrentHookBack();
				this._currentOwnerBackEffect = -1;
			}
			else if (evt.RemoveData.TargetEventId == this._currentHookBackEffect)
			{
				this.DestroyCurrentHookBackOwner();
				this._currentHookBackEffect = -1;
			}
		}

		public override void Clear()
		{
			this._currentHookBackEffect = -1;
			this._currentHookGoEffect = -1;
			this._currentHookStayEffect = -1;
			this._currentHookStayEffectWallPosition = Vector3.zero;
			this._currentGoDirection = Vector3.zero;
			this._currentOwnerBackEffect = -1;
			this._currentHookStayTarget = null;
			this._currentHookStayStartTime = -1L;
			base.Clear();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(DirtDevilStayingHook));

		protected ModifierData[] _upgHookGoDamage;

		protected ModifierData[] _upgHookStayAndBackDamage;

		protected ModifierData[] _upgHookBackDamage;

		private int _currentHookGoEffect = -1;

		private int _currentHookStayEffect = -1;

		private int _currentHookBackEffect = -1;

		private int _currentOwnerBackEffect = -1;

		private long _currentHookStayStartTime = -1L;

		private Vector3 _currentGoDirection = Vector3.zero;

		private CombatObject _currentHookStayTarget;

		private Vector3 _currentHookStayEffectWallPosition;
	}
}
