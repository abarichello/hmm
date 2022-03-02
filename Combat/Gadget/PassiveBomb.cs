using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveBomb : GadgetBehaviour
	{
		public PassiveBombInfo MyInfo
		{
			get
			{
				return base.Info as PassiveBombInfo;
			}
		}

		private void Start()
		{
			this.StackedObjects = new List<CombatObject>();
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this._updater.ShouldHalt())
			{
				return;
			}
			if (this.StackedObjects == null || this.StackedObjects.Count < 1)
			{
				return;
			}
			if (this._bombStackLifeTimeCounter < (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime())
			{
				for (int i = 0; i < this.StackedObjects.Count; i++)
				{
					CombatObject combatObject = this.StackedObjects[i];
					combatObject.ListenToObjectUnspawn -= this.OnTargetDeath;
				}
				this.StackedObjects.Clear();
			}
		}

		public override void Activate()
		{
			base.Activate();
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			PassiveBombInfo myInfo = this.MyInfo;
			this._buffs = ModifierData.CreateData(myInfo.Buff, myInfo);
			this.LifeTimeUpgrade = new Upgradeable(myInfo.BombLifeTimeUpgrade, myInfo.BombLifeTime, myInfo.UpgradesValues);
			this.BombStackLifeTime = new Upgradeable(myInfo.BombStackLifeTimeUpgrade, myInfo.BombStackLifeTime, myInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			for (int i = 0; i < this._buffs.Length; i++)
			{
				this._buffs[i].SetLevel(upgradeName, level);
			}
			this.LifeTimeUpgrade.SetLevel(upgradeName, level);
			this.BombStackLifeTime.SetLevel(upgradeName, level);
		}

		private void OnTargetDeath(CombatObject obj, UnspawnEvent msg)
		{
			if (!base.Activated)
			{
				return;
			}
			this.StackedObjects.Remove(obj);
			obj.ListenToObjectUnspawn -= this.OnTargetDeath;
			PassiveBombInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = new EffectEvent();
			effectEvent.CopyInfo(myInfo.BombEffect);
			effectEvent.Origin = obj.transform.position;
			effectEvent.LifeTime = this.LifeTimeUpgrade.Get();
			effectEvent.Modifiers = this._buffs;
			effectEvent.SourceGadget = this;
			effectEvent.SourceCombat = this.Combat;
			effectEvent.SourceSlot = base.Slot;
			effectEvent.SourceId = this.Parent.ObjId;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			if (!base.Activated)
			{
				return;
			}
			if (evt.EffectData.SourceSlot == GadgetSlot.CustomGadget0)
			{
				if (evt.RemoveData.TargetId == -1)
				{
					return;
				}
				Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(evt.RemoveData.TargetId);
				if (@object == null)
				{
					return;
				}
				CombatObject component = @object.GetComponent<CombatObject>();
				if (component == null || !component.IsAlive() || !component.IsPlayer)
				{
					return;
				}
				if (this.StackedObjects.Contains(component))
				{
					this._bombStackLifeTimeCounter = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (long)(this.BombStackLifeTime.Get() * 1000f);
				}
				else
				{
					this.StackedObjects.Add(component);
					component.ListenToObjectUnspawn += this.OnTargetDeath;
					this._bombStackLifeTimeCounter = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (long)(this.BombStackLifeTime.Get() * 1000f);
				}
			}
		}

		private List<CombatObject> StackedObjects;

		private Upgradeable LifeTimeUpgrade;

		private Upgradeable BombStackLifeTime;

		private long _bombStackLifeTimeCounter;

		private ModifierData[] _buffs;

		private TimedUpdater _updater = new TimedUpdater
		{
			PeriodMillis = 100
		};
	}
}
