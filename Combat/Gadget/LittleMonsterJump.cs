using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LittleMonsterJump : FireEffectsInArea, DamageAreaCallback.IDamageAreaCallbackListener
	{
		public new LittleMonsterJumpInfo MyInfo
		{
			get
			{
				return base.Info as LittleMonsterJumpInfo;
			}
		}

		public void OnDamageAreaCallback(DamageAreaCallback evt)
		{
			bool flag = evt.TargetGadgetSlot == base.Slot || evt.TargetGadgetSlot == GadgetSlot.None || evt.TargetGadgetSlot == GadgetSlot.Any;
			if (this._multiJumpMaxDistance.Get() > 1E-45f && evt.DamagedPlayers.Count > 0 && flag)
			{
				this._areaColliders.AddRange(Physics.OverlapSphere(evt.Origin, this._multiJumpMaxDistance.Get(), 1077054464));
				if (evt.DamagedPlayers.Find((CombatObject combat) => combat.Team != this.Combat.Team && !combat.Movement.HasLinkWith(this.Combat.Movement) && this._areaColliders.Contains(combat.GetComponent<Collider>())))
				{
					base.ExistingFiredEffectsAdd(this.FireGadget());
				}
			}
			this._areaColliders.Clear();
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._multiJumpMaxDistance = new Upgradeable(this.MyInfo.MultiJumpUpgrade, this.MyInfo.MultiJumpMaxDistance, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._multiJumpMaxDistance.SetLevel(upgradeName, level);
		}

		private Upgradeable _multiJumpMaxDistance;

		private readonly List<Collider> _areaColliders = new List<Collider>(20);
	}
}
