using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/ResetPhysics")]
	public class ResetPhysicsBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._physicalObject == null)
			{
				base.LogSanitycheckError("'Physical Object' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IPhysicalObject value = this._physicalObject.GetValue(gadgetContext);
			value.ResetImpulseAndVelocity();
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._physicalObject, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private PhysicalObjectParameter _physicalObject;
	}
}
