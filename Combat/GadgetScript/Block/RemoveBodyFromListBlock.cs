using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/List/RemoveBodyFromListBlock")]
	public class RemoveBodyFromListBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._body == null)
			{
				base.LogSanitycheckError("'Body' parameter cannot be null.");
				return false;
			}
			if (this._list == null)
			{
				base.LogSanitycheckError("'List' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			List<GadgetBody> value = this._list.GetValue(gadgetContext);
			value.Remove(this._body.GetValue(gadgetContext));
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._body, parameterId) || base.CheckIsParameterWithId(this._list, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private GadgetBodyParameter _body;

		[Header("Write")]
		[SerializeField]
		private GadgetBodyListParameter _list;
	}
}
