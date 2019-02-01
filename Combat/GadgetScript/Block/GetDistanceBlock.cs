using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/GetDistanceBlock")]
	public class GetDistanceBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._positionA == null)
			{
				base.LogSanitycheckError("'Position A' parameter cannot be null.");
				return false;
			}
			if (this._positionB == null)
			{
				base.LogSanitycheckError("'Position B' parameter cannot be null.");
				return false;
			}
			if (this._distance == null)
			{
				base.LogSanitycheckError("'Distance' parameter cannot be null.");
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
				Vector3 value = this._positionA.GetValue(gadgetContext);
				Vector3 value2 = this._positionB.GetValue(gadgetContext);
				float value3 = Vector3.Distance(value, value2);
				this._distance.SetValue(gadgetContext, value3);
				ihmmeventContext.SaveParameter(this._distance);
				return this._nextBlock;
			}
			ihmmeventContext.LoadParameter(this._distance);
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._positionA, parameterId) || base.CheckIsParameterWithId(this._positionB, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private Vector3Parameter _positionA;

		[SerializeField]
		private Vector3Parameter _positionB;

		[Header("Write")]
		[SerializeField]
		private FloatParameter _distance;
	}
}
