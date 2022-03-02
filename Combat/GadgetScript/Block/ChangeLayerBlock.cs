using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/ChangeLayer")]
	public class ChangeLayerBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._layerParameter.GetValue<CombatLayer>(gadgetContext).ChangeLayer(this._layer, (CombatGadget)gadgetContext);
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(CombatLayer)
		})]
		[SerializeField]
		private BaseParameter _layerParameter;

		[SerializeField]
		private LayerManager.Layer _layer;
	}
}
