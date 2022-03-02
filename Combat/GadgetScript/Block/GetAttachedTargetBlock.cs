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
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._targetParameter);
				return this._nextBlock;
			}
			GadgetBody value = this._body.GetValue<GadgetBody>(gadgetContext);
			AttachToDummyBodyMovement component = value.GetComponent<AttachToDummyBodyMovement>();
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

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		[SerializeField]
		private BaseParameter _body;

		[Header("Write")]
		[SerializeField]
		private CombatObjectParameter _targetParameter;
	}
}
