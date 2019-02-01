using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class CallbackOnTriggerEnterInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(CallbackOnTriggerEnter);
		}

		public bool TrailMustFollowCar;

		public FXInfo TrailEffect;

		public ModifierInfo[] TrailModifier;

		public float TrailPiecesLifeTime;

		public float TrailPiecesDropIntervalMillis;

		public int TrailColliderRadius;
	}
}
