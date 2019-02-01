using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public struct FullEffect
	{
		public FXInfo Effect;

		public ModifierInfo[] Modifiers;

		[NonSerialized]
		public ModifierData[] ModifiersData;

		public ModifierInfo[] ExtraModifiers;

		[NonSerialized]
		public ModifierData[] ExtraModifiersData;
	}
}
