using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class PassiveExtraDamage : GadgetBehaviour
	{
		public PassiveExtraDamageInfo MyInfo
		{
			get
			{
				return base.Info as PassiveExtraDamageInfo;
			}
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			PassiveExtraDamageInfo myInfo = this.MyInfo;
			this._extraDamage = ModifierData.CreateData(myInfo.ExtraDamage, myInfo);
			this.Combat.GetGadget(this.MyInfo.TargetGadget).AttachDamage(this._extraDamage);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			for (int i = 0; i < this._extraDamage.Length; i++)
			{
				this._extraDamage[i].SetLevel(upgradeName, level);
			}
		}

		private ModifierData[] _extraDamage;
	}
}
