using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/NearestGadgetBody")]
	public class NearestGadgetBodyBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._position == null)
			{
				base.LogSanitycheckError("'Position' parameter cannot be null.");
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
			if (ihmmgadgetContext.IsServer)
			{
				Vector3 value = this._position.GetValue(gadgetContext);
				List<IGadgetBody> list = new List<IGadgetBody>(gadgetContext.Bodies.Values);
				GadgetBody gadgetBody = null;
				bool flag = false;
				bool flag2 = !string.IsNullOrEmpty(this._tag);
				for (int i = 0; i < list.Count; i++)
				{
					if ((!flag2 || list[i].Tag == this._tag) && (!flag || (gadgetBody.Position - value).sqrMagnitude > (list[i].Position - value).sqrMagnitude))
					{
						flag = true;
						gadgetBody = (GadgetBody)list[i];
					}
				}
				this._body.SetValue(gadgetContext, gadgetBody);
				ihmmeventContext.SaveParameter(this._body);
			}
			else
			{
				ihmmeventContext.LoadParameter(this._body);
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._body, parameterId) || base.CheckIsParameterWithId(this._position, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private Vector3Parameter _position;

		[TagSelector]
		[SerializeField]
		[Tooltip("TAG used to identify which bodies will be tested. Only include the bodies with this TAG.")]
		private string _tag;

		[Header("Write")]
		[SerializeField]
		private GadgetBodyParameter _body;
	}
}
