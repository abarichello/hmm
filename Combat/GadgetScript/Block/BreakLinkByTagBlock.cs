using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/BreakLinkByTag")]
	public class BreakLinkByTagBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			ICombatLink linkWithTag = this._targetParameter.GetValue<IPhysicalObject>(gadgetContext).GetLinkWithTag(this._linkTag);
			if (linkWithTag != null && !linkWithTag.IsBroken)
			{
				linkWithTag.Break();
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
	}
}
