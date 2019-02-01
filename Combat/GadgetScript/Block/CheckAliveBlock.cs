using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/CheckAlive")]
	public class CheckAliveBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (CheckAliveBlock._resultParameter == null)
			{
				CheckAliveBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._combat != null)
			{
				return true;
			}
			base.LogSanitycheckError("'Combat' parameter cannot be null.");
			return false;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				ICombatObject value = this._combat.GetValue(gadgetContext);
				CheckAliveBlock._resultParameter.SetValue(gadgetContext, value.Data.IsAlive());
				ihmmeventContext.SaveParameter(CheckAliveBlock._resultParameter);
			}
			else
			{
				ihmmeventContext.LoadParameter(CheckAliveBlock._resultParameter);
			}
			return (!CheckAliveBlock._resultParameter.GetValue(gadgetContext)) ? this._failureBlock : this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._combat, parameterId);
		}

		[SerializeField]
		private BaseBlock _failureBlock;

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _combat;

		private static BoolParameter _resultParameter;
	}
}
