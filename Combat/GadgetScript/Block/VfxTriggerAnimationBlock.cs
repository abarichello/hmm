using System;
using System.Collections.Generic;
using HeavyMetalMachines.Render;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/TriggerAnimation")]
	public class VfxTriggerAnimationBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsServer && !ihmmgadgetContext.IsTest)
			{
				((IHMMEventContext)eventContext).SendToClient();
				return this._nextBlock;
			}
			CombatObject combatObject = (CombatObject)ihmmgadgetContext.Owner;
			Animator componentInChildren = combatObject.GetComponentInChildren<Animator>(true);
			componentInChildren.SetTrigger(this._animationTriggerName);
			combatObject.GetComponentsInChildren<IAnimatorGadgetFeedback>(true, this._gadgetFeedbacks);
			for (int i = 0; i < this._gadgetFeedbacks.Count; i++)
			{
				IAnimatorGadgetFeedback animatorGadgetFeedback = this._gadgetFeedbacks[i];
				if (animatorGadgetFeedback.TriggerType == 9)
				{
					animatorGadgetFeedback.TriggerName = this._animationTriggerName;
					animatorGadgetFeedback.Activate();
				}
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private string _animationTriggerName;

		private readonly List<IAnimatorGadgetFeedback> _gadgetFeedbacks = new List<IAnimatorGadgetFeedback>();
	}
}
