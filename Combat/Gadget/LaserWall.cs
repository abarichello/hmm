using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LaserWall : MortarEffect
	{
		public LaserWallInfo MyInfo
		{
			get
			{
				return base.Info as LaserWallInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			LaserWallInfo myInfo = this.MyInfo;
			this._upgPlayerDamage = ModifierData.CreateData(myInfo.PlayerDamage, myInfo);
			this._upgWallLifeTime = new Upgradeable(myInfo.WallLifeTimeUpgrade, myInfo.WallLifeTime, myInfo.UpgradesValues);
			this._upgWallAmount = new Upgradeable(myInfo.WallAmountUpgrade, (float)myInfo.WallAmount, myInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgPlayerDamage.SetLevel(upgradeName, level);
			this._upgWallLifeTime.SetLevel(upgradeName, level);
			this._upgWallAmount.SetLevel(upgradeName, level);
		}

		protected override EffectEvent SetAdditionalMortarData(EffectEvent data)
		{
			this._casterOriginalPosition = this.Combat.transform.position;
			return base.SetAdditionalMortarData(data);
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId != this._currentMortar || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._currentMortar = -1;
			LaserWallInfo myInfo = this.MyInfo;
			float lifeTime = this._upgWallLifeTime;
			ModifierData[] modifiers = ModifierData.CopyData(this._upgPlayerDamage);
			Vector3 target = evt.EffectData.Target;
			Vector3 casterOriginalPosition = this._casterOriginalPosition;
			Vector3 vector = base.CalcDirection(casterOriginalPosition, target);
			float playerRadius = myInfo.PlayerRadius;
			float num = myInfo.PlayerRadius * 2f;
			int num2 = (int)this._upgWallAmount;
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 90f, 0f), Vector3.one);
			float num3 = playerRadius * (float)(1 - num2);
			Vector3 normalized = matrix4x.MultiplyPoint3x4(vector).normalized;
			for (int i = 0; i < num2; i++)
			{
				Vector3 vector2 = (num3 + (float)i * num) * normalized + target;
				EffectEvent effectEvent = base.GetEffectEvent(myInfo.PlayerEffect);
				effectEvent.LifeTime = lifeTime;
				effectEvent.Origin = vector2;
				effectEvent.Target = vector2;
				effectEvent.Direction = vector;
				effectEvent.Modifiers = modifiers;
				effectEvent.Range = playerRadius;
				EffectEvent effectEvent2 = base.GetEffectEvent(myInfo.ProjectileEffect);
				effectEvent2.LifeTime = lifeTime;
				effectEvent2.Origin = vector2;
				effectEvent2.Target = vector2;
				effectEvent2.Direction = vector;
				GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
				GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent2);
			}
		}

		public override float GetRange()
		{
			return (!(this.MyInfo != null)) ? base.GetRange() : this.MyInfo.MortarRange;
		}

		public override float GetRangeSqr()
		{
			return (!(this.MyInfo != null)) ? base.GetRange() : (this.MyInfo.MortarRange * this.MyInfo.MortarRange);
		}

		protected ModifierData[] _upgPlayerDamage;

		private Upgradeable _upgWallLifeTime;

		private Upgradeable _upgWallAmount;

		private Vector3 _casterOriginalPosition;
	}
}
