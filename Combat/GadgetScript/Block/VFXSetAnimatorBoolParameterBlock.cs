using System;
using System.Collections.Generic;
using HeavyMetalMachines.Render;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/VFX/SetAnimatorBoolParameter")]
	public class VFXSetAnimatorBoolParameterBlock : BaseBlock, ISerializationCallbackReceiver
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
			bool value = this._value;
			if (this._isParameterSet)
			{
				value = this._valueParameter.GetValue(gadgetContext);
			}
			componentInChildren.SetBool(this._animatorParameterName, value);
			combatObject.GetComponentsInChildren<IAnimatorGadgetFeedback>(true, this._gadgetFeedbacks);
			for (int i = 0; i < this._gadgetFeedbacks.Count; i++)
			{
				IAnimatorGadgetFeedback animatorGadgetFeedback = this._gadgetFeedbacks[i];
				if (animatorGadgetFeedback.TriggerType == 4)
				{
					animatorGadgetFeedback.TriggerName = this._animatorParameterName;
					animatorGadgetFeedback.TriggerBool = value;
					animatorGadgetFeedback.Activate();
				}
			}
			return this._nextBlock;
		}

		public void OnAfterDeserialize()
		{
			if (this._valueParameter != null)
			{
				this._isParameterSet = true;
			}
		}

		public void OnBeforeSerialize()
		{
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private string _animatorParameterName;

		[SerializeField]
		private bool _value;

		[Tooltip("If set will be used instead of the static value")]
		[SerializeField]
		private BoolParameter _valueParameter;

		private bool _isParameterSet;

		private readonly List<IAnimatorGadgetFeedback> _gadgetFeedbacks = new List<IAnimatorGadgetFeedback>();
	}
}
