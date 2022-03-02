using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class DopplerDoubleCannon : MultipleEffectsCannon
	{
		private new DopplerDoubleCannonInfo MyInfo
		{
			get
			{
				return base.Info as DopplerDoubleCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._doubleShoot = new Upgradeable(this.MyInfo.DoubleShootUpgrade, this.MyInfo.DoubleShoot, this.MyInfo.UpgradesValues);
			new List<FullEffect>(this.MyInfo.AllEffects).ForEach(delegate(FullEffect data)
			{
				this._shotsId.Add(data.Effect.EffectId);
			});
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._doubleShoot.SetLevel(upgradeName, level);
		}

		protected override void OnMyEffectDestroyed(DestroyEffectMessage evt)
		{
			if (this._doubleShoot.BoolGet())
			{
				if (this._shotsId.Contains(evt.EffectData.EffectInfo.EffectId))
				{
					this._numEffectsDestroyed++;
				}
				if (this._numEffectsDestroyed == 2)
				{
					if (!this._shotTwice)
					{
						if (this.MyInfo.IncludeWarmup)
						{
							this.FireWarmup(delegate(EffectEvent data)
							{
								data.LifeTime = this.MyInfo.DoubleShootWarmupTime;
							});
						}
						else
						{
							this.FireGadget();
						}
					}
					this._numEffectsDestroyed = 0;
					this._shotTwice = !this._shotTwice;
				}
			}
			base.OnMyEffectDestroyed(evt);
		}

		public override void OnObjectSpawned(SpawnEvent evt)
		{
			base.OnObjectSpawned(evt);
			this._numEffectsDestroyed = 0;
			this._shotTwice = false;
		}

		private Upgradeable _doubleShoot;

		private int _numEffectsDestroyed;

		private bool _shotTwice;

		private HashSet<int> _shotsId = new HashSet<int>();
	}
}
