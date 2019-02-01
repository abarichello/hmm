using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/SetCombatCollidersState")]
	public class SetCombatCollidersStateBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._enabled == null)
			{
				base.LogSanitycheckError("'Enabled' parameter cannot be null.");
				return false;
			}
			if (this._combat == null)
			{
				base.LogSanitycheckError("'Combat' parameter cannot be null.");
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
			List<Collider> list = new List<Collider>(4);
			value2.Transform.GetComponentsInChildren<Collider>(list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].enabled = value;
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._enabled, parameterId) || base.CheckIsParameterWithId(this._combat, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private BoolParameter _enabled;

		[SerializeField]
		private CombatObjectParameter _combat;
	}
}
