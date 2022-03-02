using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public class ToggleLinkPhysicsByTagBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			ICombatLink linkWithTag = this._targetParameter.GetValue<IPhysicalObject>(gadgetContext).GetLinkWithTag(this._linkTag);
			if (linkWithTag != null)
			{
				if (this._changeTo == ToggleLinkPhysicsOption.Enabled)
				{
					linkWithTag.Enable();
				}
				else
				{
					linkWithTag.Disable();
				}
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(IPhysicalObject)
		})]
		[SerializeField]
		private BaseParameter _targetParameter;

		[SerializeField]
		private string _linkTag;

		[SerializeField]
		private ToggleLinkPhysicsOption _changeTo;
	}
}
