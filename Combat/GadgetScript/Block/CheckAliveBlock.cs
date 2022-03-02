using System;
using System.Diagnostics;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/CheckAlive")]
	[Obsolete("Obsolete! Use FilterBlock.")]
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

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
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

		[Conditional("AllowHacks")]
		private void LogPossibleNulls(ICombatObject combatObject)
		{
			if (CheckAliveBlock._resultParameter == null)
			{
				CheckAliveBlock.Log.Debug("_resultParameter is null when it shouldn't.");
			}
			if (combatObject == null)
			{
				CheckAliveBlock.Log.Debug("combatObject is null when it shouldn't.");
			}
			if (combatObject.Data == null)
			{
				CheckAliveBlock.Log.Debug("combatObject.Data is null when it shouldn't.");
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CheckAliveBlock));

		[SerializeField]
		private BaseBlock _failureBlock;

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _combat;

		private static BoolParameter _resultParameter;
	}
}
