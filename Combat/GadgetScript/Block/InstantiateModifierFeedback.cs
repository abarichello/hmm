using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.VFX;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/VFX/InstantiateModifierFeedback")]
	public class InstantiateModifierFeedback : BaseBlock
	{
		protected override void InternalInitialize(ref IList<BaseBlock> referencedBlocks, IHMMContext context)
		{
			base.InternalInitialize(ref referencedBlocks, context);
			ResourceLoader.Instance.PreCachePrefab(this._modifierFeedbackInfo.Name, this._modifierFeedbackInfo.EffectPreCacheCount);
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient)
			{
				return this._nextBlock;
			}
			int objId = this._causer.GetValue(gadgetContext).Identifiable.ObjId;
			GadgetSlot slot = (!(ihmmgadgetContext is CombatGadget)) ? GadgetSlot.Any : ((CombatGadget)ihmmgadgetContext).Slot;
			IParameterTomate<float> parameterTomate = this._lifetime.ParameterTomate as IParameterTomate<float>;
			float value = parameterTomate.GetValue(gadgetContext);
			int num = Mathf.FloorToInt(value * 1000f);
			int currentTime = ihmmgadgetContext.CurrentTime;
			int endtime = currentTime + num;
			this._target.GetValue(gadgetContext).Feedback.Add(this._modifierFeedbackInfo, -1, objId, currentTime, endtime, 0, slot);
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _causer;

		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private BaseParameter _lifetime;

		[SerializeField]
		private ModifierFeedbackInfo _modifierFeedbackInfo;
	}
}
