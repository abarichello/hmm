using System;
using HeavyMetalMachines.Combat.Infra;

namespace HeavyMetalMachines.Arena.Infra
{
	[Serializable]
	public struct ArenaModifierConfiguration
	{
		public ArenaModifierCondition Condition;

		public ModifierInfoArrayParameter Modifier;
	}
}
