using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Combat.GadgetScript.Block;

namespace HeavyMetalMachines.Combat.Modifier
{
	public sealed class EffectBehaviour : BaseGadget
	{
		protected override Queue<BaseBlock> _blocksToInitialize
		{
			get
			{
				return EffectModifier.BlocksToInitialize;
			}
		}

		public override void ForcePressed()
		{
			throw new NotImplementedException();
		}

		public override void ForceReleased()
		{
			throw new NotImplementedException();
		}
	}
}
