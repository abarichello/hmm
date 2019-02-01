using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.VFX;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/VFX/InstantiateModifierFeedback")]
	public class InstantiateModifierFeedback : BaseBlock, IGadgetBlockWithAsset
	{
		public void PrecacheAssets()
		{
			ResourceLoader.Instance.PreCachePrefab(this._modifierFeedbackInfo.Name, this._modifierFeedbackInfo.EffectPreCacheCount);
		}

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return true;
			}
			if (this._causer == null)
			{
				base.LogSanitycheckError("'Causer' parameter cannot be null");
				return false;
			}
			if (this._target == null)
			{
				base.LogSanitycheckError("'Target' parameter cannot be null");
				return false;
			}
			if (this._lifetime == null)
			{
				base.LogSanitycheckError("'Life time' parameter cannot be null");
				return false;
			}
			if (this._modifierFeedbackInfo == null)
			{
				base.LogSanitycheckError("'Modifier feedback info' parameter cannot be null");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient)
			{
				return this._nextBlock;
			}
			int objId = this._causer.GetValue(gadgetContext).Identifiable.ObjId;
			GadgetSlot slot = (!(ihmmgadgetContext is CombatGadget)) ? GadgetSlot.Any : ((CombatGadget)ihmmgadgetContext).Slot;
			float value = this._lifetime.GetValue(gadgetContext);
			int num = Mathf.FloorToInt(value * 1000f);
			int currentTime = ihmmgadgetContext.CurrentTime;
			int endtime = currentTime + num;
			this._target.GetValue(gadgetContext).Feedback.Add(this._modifierFeedbackInfo, -1, objId, currentTime, endtime, 0, slot);
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._causer, parameterId) || base.CheckIsParameterWithId(this._target, parameterId) || base.CheckIsParameterWithId(this._lifetime, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _causer;

		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private FloatParameter _lifetime;

		[SerializeField]
		private ModifierFeedbackInfo _modifierFeedbackInfo;
	}
}
