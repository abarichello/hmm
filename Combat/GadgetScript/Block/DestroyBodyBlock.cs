using System;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/DestroyBody")]
	public class DestroyBodyBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext hmmEventContext = (IHMMEventContext)eventContext;
			IHMMGadgetContext hmmGadgetContext = (IHMMGadgetContext)gadgetContext;
			DestroyGadgetBody.Destroy(hmmEventContext, hmmGadgetContext, this._body);
			return this._nextBlock;
		}

		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		[SerializeField]
		private BaseParameter _body;
	}
}
