using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/GetClosestValidPosition")]
	public class GetClosestValidPositionBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._validPosition);
			}
			else
			{
				this.SetValidPosition(gadgetContext);
				ihmmeventContext.SaveParameter(this._validPosition);
			}
			return this._nextBlock;
		}

		private void SetValidPosition(IGadgetContext gadgetContext)
		{
			ICombatObject value = this._combatObject.GetValue(gadgetContext);
			Vector3 vector = this._targetPosition.GetValue<Vector3>(gadgetContext);
			if (value != null)
			{
				vector = value.CombatMovement.GetClosestValidPosition(vector, true);
			}
			this._validPosition.SetValue<Vector3>(gadgetContext, vector);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _combatObject;

		[SerializeField]
		private BaseParameter _targetPosition;

		[Header("Write")]
		[SerializeField]
		private BaseParameter _validPosition;
	}
}
