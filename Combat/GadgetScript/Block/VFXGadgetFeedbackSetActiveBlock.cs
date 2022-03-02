using System;
using System.Collections.Generic;
using HeavyMetalMachines.Render;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/VFX/Set Feedback Active")]
	public class VFXGadgetFeedbackSetActiveBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsServer)
			{
				((IHMMEventContext)eventContext).SendToClient();
				return this._nextBlock;
			}
			CombatObject combatObject = (CombatObject)ihmmgadgetContext.Owner;
			combatObject.GetComponentsInChildren<IActivatableGadgetFeedback>(true, this._gadgetFeedbacks);
			for (int i = 0; i < this._gadgetFeedbacks.Count; i++)
			{
				IActivatableGadgetFeedback activatableGadgetFeedback = this._gadgetFeedbacks[i];
				if (activatableGadgetFeedback.Slot == ((CombatGadget)ihmmgadgetContext).Slot)
				{
					activatableGadgetFeedback.IsActive = this._isActive;
				}
			}
			return this._nextBlock;
		}

		[SerializeField]
		private bool _isActive;

		private readonly List<IActivatableGadgetFeedback> _gadgetFeedbacks = new List<IActivatableGadgetFeedback>();
	}
}
