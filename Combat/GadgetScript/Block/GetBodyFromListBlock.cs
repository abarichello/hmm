using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/List/GetBodyFromListBlock")]
	public class GetBodyFromListBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (!ihmmgadgetContext.IsServer)
			{
				ihmmeventContext.LoadParameter(this._body);
				return this._nextBlock;
			}
			List<GadgetBody> value = this._list.GetValue(gadgetContext);
			IParameterTomate<float> parameterTomate = this._index.ParameterTomate as IParameterTomate<float>;
			int num = (int)parameterTomate.GetValue(gadgetContext);
			if (num > -1 && num < value.Count)
			{
				GadgetBody value2 = value[num];
				this._body.SetValue<GadgetBody>(gadgetContext, value2);
				ihmmeventContext.SaveParameter(this._body);
				return this._nextBlock;
			}
			ihmmeventContext.SaveParameter(this._body);
			GetBodyFromListBlock.Log.ErrorFormat(string.Format("Invalid index value: {0} Block: {1}", num, base.name), new object[0]);
			return null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GetBodyFromListBlock));

		[Header("Read")]
		[SerializeField]
		private GadgetBodyListParameter _list;

		[SerializeField]
		private BaseParameter _index;

		[Header("Write")]
		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		[SerializeField]
		private BaseParameter _body;
	}
}
