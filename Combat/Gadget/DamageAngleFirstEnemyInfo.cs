using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class DamageAngleFirstEnemyInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(DamageAngleFirstEnemy);
		}

		public int Angle;

		public string AngleUpgrade;

		public int InitialOverlapSphereRadius;
	}
}
