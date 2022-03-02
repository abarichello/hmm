using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/NearestCombat")]
	public class NearestCombatBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				ICombatObject value = this._currentNearestCombatParameter.GetValue(gadgetContext);
				ICombatObject combatObject = value;
				Vector3 value2 = this._positionToTestParameter.GetValue<Vector3>(gadgetContext);
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

		[Header("Write")]
		[SerializeField]
		private CombatObjectParameter _currentNearestCombatParameter;

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _targetCombatParameter;

		[SerializeField]
		private BaseParameter _positionToTestParameter;

		[Header("Network")]
		[SerializeField]
		private bool _sendToClient;
	}
}
