using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/List/GetBodyFromListBlock")]
	public class GetBodyFromListBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._list == null)
			{
				base.LogSanitycheckError("'List' parameter cannot be null.");
				return false;
			}
			if (this._index == null)
			{
				base.LogSanitycheckError("'Index' parameter cannot be null.");
				return false;
			}
			if (this._body == null)
			{
				base.LogSanitycheckError("'Body' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (!ihmmgadgetContext.IsServer)
			{
				ihmmeventContext.LoadParameter(this._body);
				return this._nextBlock;
			}
			List<GadgetBody> value = this._list.GetValue(gadgetContext);
			int value2 = this._index.GetValue(gadgetContext);
			if (value2 > -1 && value2 < value.Count)
			{
				GadgetBody value3 = value[value2];
				this._body.SetValue(gadgetContext, value3);
				ihmmeventContext.SaveParameter(this._body);
				return this._nextBlock;
			}
			ihmmeventContext.SaveParameter(this._body);
			base.LogSanitycheckError(string.Format("Invalid index value: {0}", value2));
			return null;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._body, parameterId) || base.CheckIsParameterWithId(this._list, parameterId) || base.CheckIsParameterWithId(this._index, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private GadgetBodyListParameter _list;

		[SerializeField]
		private IntParameter _index;

		[Header("Write")]
		[SerializeField]
		private GadgetBodyParameter _body;
	}
}
