using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/SetCombatPhysicsState")]
	public class SetCombatPhysicsStateBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				ihmmeventContext.SaveParameter(this._enabled);
				ihmmeventContext.SaveParameter(this._combat);
			}
			else
			{
				ihmmeventContext.LoadParameter(this._enabled);
				ihmmeventContext.LoadParameter(this._combat);
			}
			bool value = this._enabled.GetValue(gadgetContext);
			ICombatObject value2 = this._combat.GetValue(gadgetContext);
			List<Rigidbody> list = new List<Rigidbody>(4);
			value2.Transform.GetComponentsInChildren<Rigidbody>(list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].isKinematic = !value;
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private BoolParameter _enabled;

		[SerializeField]
		private CombatObjectParameter _combat;
	}
}
