using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/ResetPhysics")]
	public class ResetPhysicsBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IPhysicalObject value = this._physicalObject.GetValue<IPhysicalObject>(gadgetContext);
			value.ResetImpulseAndVelocity();
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(IPhysicalObject)
		})]
		[SerializeField]
		private BaseParameter _physicalObject;
	}
}
