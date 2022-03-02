using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/Icon/Blades/Set Counter")]
	internal class HudBladesIconSetCounterBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			ICombatObject value = this._target.GetValue(gadgetContext);
			IHMMCombatGadgetContext ihmmcombatGadgetContext = (IHMMCombatGadgetContext)gadgetContext;
			if (ihmmcombatGadgetContext.IsServer)
			{
				((IHMMEventContext)eventContext).SendToClient();
			}
			else if (value != null && (ihmmcombatGadgetContext.Owner.IsLocalPlayer || value.IsLocalPlayer))
			{
				IParameterTomate<float> parameterTomate = this._stackedValueParameter.ParameterTomate as IParameterTomate<float>;
				float value2 = parameterTomate.GetValue(gadgetContext);
				IHudIconBar hudIconBar = ihmmcombatGadgetContext.GetHudIconBar(value);
				hudIconBar.BladesIcon.SetCounter((int)value2);
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private BaseParameter _stackedValueParameter;

		[SerializeField]
		private CombatObjectParameter _target;
	}
}
