using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/RevertLayer")]
	public class RevertLayerBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._layerParameter.GetValue<CombatLayer>(gadgetContext).RevertLayer((CombatGadget)gadgetContext);
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(CombatLayer)
		})]
		[SerializeField]
		private BaseParameter _layerParameter;
	}
}
