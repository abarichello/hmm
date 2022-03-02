using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/OrientatePhysicalObject")]
	public class OrientatePhysicalObjectBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			IPhysicalObject value = this._target.GetValue(gadgetContext);
			value.LookTowards(this._lookDirection.GetValue<Vector3>(gadgetContext));
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private PhysicalObjectParameter _target;

		[SerializeField]
		private BaseParameter _lookDirection;
	}
}
