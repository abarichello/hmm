using System;
using System.Collections.Generic;
using HeavyMetalMachines.Render;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/Set Animator Parameter")]
	public class VfxSetAnimatorIntParameterBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				((IHMMEventContext)eventContext).SendToClient();
				return this._nextBlock;
			}
			CombatObject combatObject = (CombatObject)((IHMMGadgetContext)gadgetContext).Owner;
			Animator componentInChildren = combatObject.GetComponentInChildren<Animator>(true);
			IParameterTomate<float> parameterTomate = this._parameterValue.ParameterTomate as IParameterTomate<float>;
			componentInChildren.SetInteger(this._parameterName, (int)parameterTomate.GetValue(gadgetContext));
			combatObject.GetComponentsInChildren<IAnimatorGadgetFeedback>(true, this._gadgetFeedbacks);
			for (int i = 0; i < this._gadgetFeedbacks.Count; i++)
			{
				IAnimatorGadgetFeedback animatorGadgetFeedback = this._gadgetFeedbacks[i];
				if (animatorGadgetFeedback.TriggerType == 3)
				{
					animatorGadgetFeedback.TriggerName = this._parameterName;
					animatorGadgetFeedback.TriggerInteger = (int)parameterTomate.GetValue(gadgetContext);
					animatorGadgetFeedback.Activate();
				}
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private string _parameterName;

		[SerializeField]
		private BaseParameter _parameterValue;

		private readonly List<IAnimatorGadgetFeedback> _gadgetFeedbacks = new List<IAnimatorGadgetFeedback>();
	}
}
