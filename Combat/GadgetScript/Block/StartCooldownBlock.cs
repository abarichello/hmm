using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Render;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Cooldown/StartCooldown")]
	public class StartCooldownBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext context, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)context;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			int creationTime = eventContext.CreationTime;
			IParameterTomate<float> parameterTomate = this._cooldownParameter.ParameterTomate as IParameterTomate<float>;
			float value = parameterTomate.GetValue(context);
			int num = (int)(StartCooldownBlock.GetCooldown(value, ihmmgadgetContext) * 1000f);
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._currentCooldownTime);
				this.SetCooldownProperties(creationTime, num, ihmmgadgetContext);
				return this._nextBlock;
			}
			IParameterTomate<float> parameterTomate2 = this._currentCooldownTime.ParameterTomate as IParameterTomate<float>;
			parameterTomate2.SetValue(context, (float)(creationTime + num));
			ihmmeventContext.SaveParameter(this._currentCooldownTime);
			ihmmeventContext.SendToClient();
			return this._nextBlock;
		}

		private static float GetCooldown(float baseCooldown, IHMMGadgetContext context)
		{
			ICombatObject combatObject = context.Owner as ICombatObject;
			if (combatObject != null)
			{
				GadgetSlot id = (GadgetSlot)context.Id;
				CombatAttributes attributes = combatObject.Attributes;
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

		private void SetCooldownProperties(int cooldownStart, int cooldown, IHMMGadgetContext context)
		{
			CombatObject combatObject = (CombatObject)context.Owner;
			GadgetsPropertiesData componentInChildren = combatObject.GetComponentInChildren<GadgetsPropertiesData>();
			if (combatObject == null || componentInChildren == null)
			{
				return;
			}
			GadgetSlot id = (GadgetSlot)context.Id;
			componentInChildren.SetCooldown(cooldown, cooldownStart, id);
		}

		[Header("Read")]
		[SerializeField]
		private BaseParameter _cooldownParameter;

		[SerializeField]
		private BaseParameter _currentCooldownTime;
	}
}
