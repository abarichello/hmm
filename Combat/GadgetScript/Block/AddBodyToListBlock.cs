using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/List/AddBodyToListBlock")]
	public class AddBodyToListBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			List<GadgetBody> value = this._list.GetValue(gadgetContext);
			value.Add(this._body.GetValue<GadgetBody>(gadgetContext));
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		[SerializeField]
		private BaseParameter _body;

		[Header("Write")]
		[SerializeField]
		private GadgetBodyListParameter _list;
	}
}
