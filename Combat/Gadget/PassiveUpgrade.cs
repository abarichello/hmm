using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class PassiveUpgrade : GadgetBehaviour
	{
		public PassiveUpgradeInfo MyInfo
		{
			get
			{
				return base.Info as PassiveUpgradeInfo;
			}
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			PassiveUpgradeInfo myInfo = this.MyInfo;
			this._upgrades = ModifierData.CreateData(myInfo.Damage, myInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.Combat.Controller.RemovePassiveModifiers(this._upgrades, this.Combat, -1);
			for (int i = 0; i < this._upgrades.Length; i++)
			{
				this._upgrades[i].SetLevel(upgradeName, level);
			}
			this.Combat.Controller.AddPassiveModifiers(this._upgrades, this.Combat, -1);
		}

		protected ModifierData[] _upgrades;
	}
}
