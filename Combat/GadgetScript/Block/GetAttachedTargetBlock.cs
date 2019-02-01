using System;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/GetAttachedTargetBlock")]
	public class GetAttachedTargetBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return true;
			}
			if (this._body == null)
			{
				base.LogSanitycheckError("'Body' parameter cannot be null.");
				return false;
			}
			if (this._targetParameter == null)
			{
				base.LogSanitycheckError("'Target Parameter' cannot be null.");
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
				ihmmeventContext.LoadParameter(this._targetParameter);
				return this._nextBlock;
			}
			GadgetBody value = this._body.GetValue(gadgetContext);
			GadgetBodyLinkedMovement component = value.GetComponent<GadgetBodyLinkedMovement>();
			if (component != null)
			{
				ICombatObject target = component.GetTarget();
				if (target != null)
				{
					this._targetParameter.SetValue(gadgetContext, target);
				}
			}
			else
			{
				this._targetParameter.SetValue(gadgetContext, null);
			}
			ihmmeventContext.SaveParameter(this._targetParameter);
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._body, parameterId) || base.CheckIsParameterWithId(this._targetParameter, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private GadgetBodyParameter _body;

		[Header("Write")]
		[SerializeField]
		private CombatObjectParameter _targetParameter;
	}
}
