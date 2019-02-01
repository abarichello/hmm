using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BasicAttackReloadInfo : BasicAttackInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicAttackReload);
		}

		public FXInfo ReloadEffect;

		public float ReloadTime;

		public int MaxAmmo;
	}
}
