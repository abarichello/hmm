using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/GetClosestValidPosition")]
	public class GetClosestValidPositionBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._combatObject == null)
			{
				base.LogSanitycheckError("'Combat Object' parameter cannot be null.");
				return false;
			}
			if (this._targetPosition == null)
			{
				base.LogSanitycheckError("'Target Position' parameter cannot be null.");
				return false;
			}
			if (this._validPosition == null)
			{
				base.LogSanitycheckError("'Valid Position' parameter cannot be null.");
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
				ihmmeventContext.LoadParameter(this._validPosition);
			}
			else
			{
				this.SetValidPosition(gadgetContext);
				ihmmeventContext.SaveParameter(this._validPosition);
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._combatObject, parameterId) || base.CheckIsParameterWithId(this._targetPosition, parameterId) || base.CheckIsParameterWithId(this._validPosition, parameterId);
		}

		private void SetValidPosition(IGadgetContext gadgetContext)
		{
			ICombatObject value = this._combatObject.GetValue(gadgetContext);
			Vector3 vector = this._targetPosition.GetValue(gadgetContext);
			if (value != null)
			{
				vector = value.CombatMovement.GetClosestValidPosition(vector, true);
			}
			this._validPosition.SetValue(gadgetContext, vector);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _combatObject;

		[SerializeField]
		private Vector3Parameter _targetPosition;

		[Header("Write")]
		[SerializeField]
		private Vector3Parameter _validPosition;
	}
}
