using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/NearestCombat")]
	public class NearestCombatBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._currentNearestCombatParameter == null)
			{
				base.LogSanitycheckError("'Current Nearest Combat Parameter' cannot be null.");
				return false;
			}
			if (this._targetCombatParameter == null)
			{
				base.LogSanitycheckError("'Target Combat Parameter' cannot be null.");
				return false;
			}
			if (this._positionToTestParameter == null)
			{
				base.LogSanitycheckError("'Position To Test Parameter' cannot be null.");
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
				ICombatObject value = this._currentNearestCombatParameter.GetValue(gadgetContext);
				ICombatObject combatObject = value;
				Vector3 value2 = this._positionToTestParameter.GetValue(gadgetContext);
				if (combatObject == null)
				{
					combatObject = this._targetCombatParameter.GetValue(gadgetContext);
				}
				else if ((combatObject.PhysicalObject.Position - value2).sqrMagnitude > (this._targetCombatParameter.GetValue(gadgetContext).PhysicalObject.Position - value2).sqrMagnitude)
				{
					combatObject = this._targetCombatParameter.GetValue(gadgetContext);
				}
				this._currentNearestCombatParameter.SetValue(gadgetContext, combatObject);
				ihmmeventContext.SaveParameter(this._currentNearestCombatParameter);
				if (this._sendToClient)
				{
					((IHMMEventContext)eventContext).SendToClient();
				}
			}
			else
			{
				ihmmeventContext.LoadParameter(this._currentNearestCombatParameter);
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._currentNearestCombatParameter, parameterId) || base.CheckIsParameterWithId(this._targetCombatParameter, parameterId) || base.CheckIsParameterWithId(this._positionToTestParameter, parameterId);
		}

		[Header("Write")]
		[SerializeField]
		private CombatObjectParameter _currentNearestCombatParameter;

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _targetCombatParameter;

		[SerializeField]
		private Vector3Parameter _positionToTestParameter;

		[Header("Network")]
		[SerializeField]
		private bool _sendToClient;
	}
}
