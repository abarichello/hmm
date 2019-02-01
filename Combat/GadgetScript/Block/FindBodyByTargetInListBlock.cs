using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/List/FindBodyByTargetInListBlock")]
	public class FindBodyByTargetInListBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._list == null)
			{
				base.LogSanitycheckError("'List' parameter cannot be null.");
				return false;
			}
			if (this._combat == null)
			{
				base.LogSanitycheckError("'Combat' parameter cannot be null.");
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
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._body);
				return this._nextBlock;
			}
			List<GadgetBody> value = this._list.GetValue(gadgetContext);
			for (int i = 0; i < value.Count; i++)
			{
				GadgetBody gadgetBody = value[i];
				if (!(gadgetBody == null))
				{
					GadgetBodyLinkedMovement component = gadgetBody.GetComponent<GadgetBodyLinkedMovement>();
					if (!(component == null))
					{
						ICombatObject target = component.GetTarget();
						if (target.Identifiable.ObjId == this._combat.GetValue(gadgetContext).Identifiable.ObjId)
						{
							this._body.SetValue(gadgetContext, gadgetBody);
							ihmmeventContext.SaveParameter(this._body);
							return this._nextBlock;
						}
					}
				}
			}
			this._body.SetValue(gadgetContext, null);
			ihmmeventContext.SaveParameter(this._body);
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._body, parameterId) || base.CheckIsParameterWithId(this._list, parameterId) || base.CheckIsParameterWithId(this._combat, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private GadgetBodyListParameter _list;

		[SerializeField]
		private CombatObjectParameter _combat;

		[Header("Write")]
		[SerializeField]
		private GadgetBodyParameter _body;
	}
}
