using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/BreakLink")]
	public class BreakLinkBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			ICombatLink value = this._linkParameter.GetValue<ICombatLink>(gadgetContext);
			if (value != null && !value.IsBroken)
			{
				value.Break();
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(ICombatLink)
		})]
		[SerializeField]
		private BaseParameter _linkParameter;
	}
}
