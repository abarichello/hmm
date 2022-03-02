using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/Gauge/Set Value")]
	internal class HudSetGaugeBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMCombatGadgetContext ihmmcombatGadgetContext = (IHMMCombatGadgetContext)gadgetContext;
			if (ihmmcombatGadgetContext.IsServer)
			{
				((IHMMEventContext)eventContext).SendToClient();
			}
			else if (ihmmcombatGadgetContext.Owner.IsLocalPlayer)
			{
				IParameterTomate<float> parameterTomate = this._minValueParameter.ParameterTomate as IParameterTomate<float>;
				IParameterTomate<float> parameterTomate2 = this._maxValueParameter.ParameterTomate as IParameterTomate<float>;
				IParameterTomate<float> parameterTomate3 = this._currentValueParameter.ParameterTomate as IParameterTomate<float>;
				int min = (int)parameterTomate.GetValue(gadgetContext);
				int max = (int)parameterTomate2.GetValue(gadgetContext);
				int current = (int)parameterTomate3.GetValue(gadgetContext);
				ihmmcombatGadgetContext.GadgetHudElement.Gauge.SetValue(min, max, current);
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private BaseParameter _minValueParameter;

		[SerializeField]
		private BaseParameter _maxValueParameter;

		[SerializeField]
		private BaseParameter _currentValueParameter;
	}
}
