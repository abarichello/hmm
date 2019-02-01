using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Obsolete]
	public class CreepBasicAttack : BasicAttack
	{
		public new CreepBasicAttackInfo MyInfo
		{
			get
			{
				return base.Info as CreepBasicAttackInfo;
			}
		}

		public void SetTargetCombatObject(CombatObject value)
		{
			this._targetCombatObject = value;
		}

		protected override void GadgetUpdate()
		{
			base.GadgetUpdate();
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this.damageDatas.Count > 0)
			{
				while (this.damageDatas.Peek().time <= this.CurrentTime)
				{
					CreepBasicAttack.DamageData damageData = this.damageDatas.Dequeue();
					if (damageData.target && damageData.target.IsAlive())
					{
						damageData.target.Controller.AddModifiers(damageData.mods, this.Combat, -1, false);
					}
					if (this.damageDatas.Count == 0)
					{
						break;
					}
				}
			}
		}

		protected override void FireCannon()
		{
			if (this._targetCombatObject == null)
			{
				return;
			}
			base.UpdateDamage();
			ModifierData[] damage = this._damage;
			CreepBasicAttack.DamageData item = default(CreepBasicAttack.DamageData);
			item.mods = damage;
			item.target = this._targetCombatObject;
			item.time = (long)(GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + this.DamageDelay);
			this.damageDatas.Enqueue(item);
		}

		private CombatObject _targetCombatObject;

		public CreepController creepController;

		public int DamageDelay = 150;

		private Queue<CreepBasicAttack.DamageData> damageDatas = new Queue<CreepBasicAttack.DamageData>();

		private struct DamageData
		{
			public ModifierData[] mods;

			public long time;

			public CombatObject target;
		}
	}
}
