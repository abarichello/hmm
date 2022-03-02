using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/GetDistanceBlock")]
	public class GetDistanceBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				Vector3 value = this._positionA.GetValue<Vector3>(gadgetContext);
				Vector3 value2 = this._positionB.GetValue<Vector3>(gadgetContext);
				float value3 = Vector3.Distance(value, value2);
				IParameterTomate<float> parameterTomate = this._distance.ParameterTomate as IParameterTomate<float>;
				parameterTomate.SetValue(gadgetContext, value3);
				ihmmeventContext.SaveParameter(this._distance);
				return this._nextBlock;
			}
			ihmmeventContext.LoadParameter(this._distance);
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(Vector3)
		})]
		[SerializeField]
		private BaseParameter _positionA;

		[Restrict(true, new Type[]
		{
			typeof(Vector3)
		})]
		[SerializeField]
		private BaseParameter _positionB;

		[Header("Write")]
		[SerializeField]
		private BaseParameter _distance;
	}
}
