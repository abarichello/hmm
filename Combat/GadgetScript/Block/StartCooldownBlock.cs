using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Cooldown/StartCooldown")]
	public class StartCooldownBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return true;
			}
			if (this._cooldownParameter == null)
			{
				base.LogSanitycheckError("'Cooldown Parameter' cannot be null.");
				return false;
			}
			if (this._currentCooldownTime == null)
			{
				base.LogSanitycheckError("'Current Cooldown Time' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext context, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)context;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._currentCooldownTime);
				return this._nextBlock;
			}
			int creationTime = eventContext.CreationTime;
			float value = this._cooldownParameter.GetValue(context);
			int num = (int)(StartCooldownBlock.GetCooldown(value, ihmmgadgetContext) * 1000f);
			this._currentCooldownTime.SetValue(context, creationTime + num);
			ihmmeventContext.SaveParameter(this._currentCooldownTime);
			ihmmeventContext.SendToClient();
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._cooldownParameter, parameterId) || base.CheckIsParameterWithId(this._currentCooldownTime, parameterId);
		}

		private static float GetCooldown(float baseCooldown, IHMMGadgetContext context)
		{
			ICombatObject combatObject = context.GetCombatObject(context.OwnerId);
			if (combatObject != null)
			{
				GadgetSlot id = (GadgetSlot)context.Id;
				CombatAttributes attributes = ((CombatObject)combatObject).Attributes;
				switch (id)
				{
				case GadgetSlot.CustomGadget0:
					return baseCooldown * (1f - attributes.CooldownReductionGadget0Pct) - attributes.CooldownReductionGadget0;
				case GadgetSlot.CustomGadget1:
					return baseCooldown * (1f - attributes.CooldownReductionGadget1Pct) - attributes.CooldownReductionGadget1;
				case GadgetSlot.CustomGadget2:
					return baseCooldown * (1f - attributes.CooldownReductionGadget2Pct) - attributes.CooldownReductionGadget2;
				case GadgetSlot.BoostGadget:
					return baseCooldown * (1f - attributes.CooldownReductionGadgetBPct) - attributes.CooldownReductionGadgetB;
				}
			}
			return baseCooldown;
		}

		[SerializeField]
		private FloatParameter _cooldownParameter;

		[SerializeField]
		private IntParameter _currentCooldownTime;
	}
}
