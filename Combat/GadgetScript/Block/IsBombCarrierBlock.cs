using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Bomb/IsBombCarrier")]
	public class IsBombCarrierBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._result);
			}
			else
			{
				bool value = this.CheckIsCarryingBomb(ihmmgadgetContext);
				this._result.SetValue(gadgetContext, value);
				ihmmeventContext.SaveParameter(this._result);
			}
			return this._nextBlock;
		}

		private bool CheckIsCarryingBomb(IHMMGadgetContext hmmGadgetContext)
		{
			ICombatObject value = this._combatObject.GetValue(hmmGadgetContext);
			return value != null && hmmGadgetContext.IsCarryingBomb(value);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _combatObject;

		[Header("Write")]
		[SerializeField]
		private BoolParameter _result;
	}
}
