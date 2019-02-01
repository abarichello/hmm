using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LowHPView : GadgetBehaviour
	{
		public LowHPViewInfo MyInfo
		{
			get
			{
				return base.Info as LowHPViewInfo;
			}
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			LowHPViewInfo myInfo = this.MyInfo;
			this._hpPercent = new Upgradeable(myInfo.HPPercentUpgrade, myInfo.HPPercent, myInfo.UpgradesValues);
			this._damage = ModifierData.CreateData(myInfo.RevealStatus, myInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._hpPercent.SetLevel(upgradeName, level);
			this._damage.SetLevel(upgradeName, level);
		}

		public override void Activate()
		{
			base.Activate();
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Players[i];
				if (playerData.Team != this.Combat.Team)
				{
					CombatObject combat = CombatRef.GetCombat(playerData.CharacterInstance);
					if (combat)
					{
						this._targets.Add(combat);
					}
				}
			}
		}

		protected override void GadgetUpdate()
		{
			base.GadgetUpdate();
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._updater.ShouldHalt())
			{
				return;
			}
			for (int i = 0; i < this._targets.Count; i++)
			{
				CombatObject combatObject = this._targets[i];
				int effectId;
				if (!this.Combat.IsAlive() || !combatObject.IsAlive() || combatObject.Data.CurrentHPPercent > this._hpPercent.Get())
				{
					if (this._effects.TryGetValue(combatObject, out effectId))
					{
						this.RemoveEffect(combatObject, effectId);
					}
				}
				else if (!this._effects.TryGetValue(combatObject, out effectId))
				{
					this.AddEffect(combatObject);
				}
			}
		}

		private void RemoveEffect(CombatObject obj, int effectId)
		{
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = effectId,
				SourceId = this.Parent.ObjId,
				TargetId = obj.Id.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._effects.Remove(obj);
		}

		private void AddEffect(CombatObject obj)
		{
			LowHPViewInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.RevealEffect);
			effectEvent.TargetId = obj.Id.ObjId;
			effectEvent.Modifiers = this._damage;
			int value = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this._effects[obj] = value;
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			Dictionary<CombatObject, int> dictionary = new Dictionary<CombatObject, int>(this._effects);
			foreach (KeyValuePair<CombatObject, int> keyValuePair in dictionary)
			{
				this.RemoveEffect(keyValuePair.Key, keyValuePair.Value);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(LowHPView));

		private Upgradeable _hpPercent;

		private ModifierData[] _damage;

		private List<CombatObject> _targets = new List<CombatObject>();

		private Dictionary<CombatObject, int> _effects = new Dictionary<CombatObject, int>();

		private TimedUpdater _updater = new TimedUpdater
		{
			PeriodMillis = 50
		};
	}
}
