using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public abstract class BaseApplyModifierBlock : BaseBlock
	{
		protected override void InternalInitialize(ref IList<BaseBlock> referencedBlocks, IHMMContext context)
		{
			this._modsInfos = new ModifierInfo[this._modifierInfoContainer.Length];
			for (int i = 0; i < this._modifierInfoContainer.Length; i++)
			{
				if (this._modifierInfoContainer[i].Modifier.Feedback != null)
				{
					ResourceLoader.Instance.PreCachePrefab(this._modifierInfoContainer[i].Modifier.Feedback.Name, this._modifierInfoContainer[i].Modifier.Feedback.EffectPreCacheCount);
				}
				this._modsInfos[i] = this._modifierInfoContainer[i].Modifier;
				this._modifierInfoContainer[i].Initialize();
				if (this._modifierInfoContainer[i].HasEvent)
				{
					referencedBlocks.Add(this._modifierInfoContainer[i].OnModifierAppliedBlock);
				}
			}
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient)
			{
				return this._nextBlock;
			}
			if (this._target == null || this._target.GetValue<ICombatObject>(gadgetContext) == null)
			{
				return this._nextBlock;
			}
			ICombatController modifierController = this._target.GetValue<ICombatObject>(gadgetContext).ModifierController;
			for (int i = 0; i < this._modsInfos.Length; i++)
			{
				float amount = this._modsInfos[i].Amount;
				if (this._modifierInfoContainer[i].Amount != null)
				{
					IParameterTomate<float> parameterTomate = (IParameterTomate<float>)this._modifierInfoContainer[i].Amount.ParameterTomate;
					amount = parameterTomate.GetValue(gadgetContext);
				}
				ModifierData modifierData = new ModifierData(this._modsInfos[i], amount);
				if (this._modifierInfoContainer[i].HasEvent)
				{
					modifierData.OnModifierApplied += new BaseApplyModifierBlock.ModifierEventHandler(gadgetContext, this._modifierInfoContainer[i].OnModifierAppliedBlock, this._modifierAppliedEventParameters).ModifierApplied;
				}
				this.ApplyModifier(modifierData, ihmmgadgetContext, modifierController);
			}
			return this._nextBlock;
		}

		protected abstract void ApplyModifier(ModifierData data, IHMMGadgetContext gadgetContext, ICombatController target);

		[Header("Write")]
		[SerializeField]
		protected BaseApplyModifierBlock.ModifierEventParameters _modifierAppliedEventParameters;

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(ICombatObject)
		})]
		[SerializeField]
		private BaseParameter _target;

		[SerializeField]
		protected BaseApplyModifierBlock.ModifierInfoContainer[] _modifierInfoContainer;

		private ModifierInfo[] _modsInfos;

		private struct ModifierEventHandler
		{
			public ModifierEventHandler(IGadgetContext gadgetContext, IBlock onModifierAppliedBlock, BaseApplyModifierBlock.ModifierEventParameters parameters)
			{
				this.GadgetContext = gadgetContext;
				this.OnModifierAppliedBlock = onModifierAppliedBlock;
				this.Target = ((!(null != parameters.TargetParameter)) ? null : (parameters.TargetParameter.ParameterTomate as IParameterTomate<ICombatObject>));
				this.Amount = ((!(null != parameters.AmountParameter)) ? null : (parameters.AmountParameter.ParameterTomate as IParameterTomate<float>));
				this.OriginalAmount = ((!(null != parameters.OriginalAmountParameter)) ? null : (parameters.OriginalAmountParameter.ParameterTomate as IParameterTomate<float>));
				this.Modifier = ((!(null != parameters.ModifierParameter)) ? null : (parameters.ModifierParameter.ParameterTomate as IParameterTomate<ModifierData>));
			}

			public void ModifierApplied(ModifierData mod, CombatObject causer, CombatObject target, float amount, float originalAmount)
			{
				if (this.Target != null)
				{
					this.Target.SetValue(this.GadgetContext, target);
				}
				if (this.Amount != null)
				{
					this.Amount.SetValue(this.GadgetContext, amount);
				}
				if (this.OriginalAmount != null)
				{
					this.OriginalAmount.SetValue(this.GadgetContext, originalAmount);
				}
				if (this.Modifier != null)
				{
					this.Modifier.SetValue(this.GadgetContext, mod);
				}
				((BaseGadget)this.GadgetContext).TriggerEvent(this.OnModifierAppliedBlock.Id);
			}

			public IGadgetContext GadgetContext;

			public IBlock OnModifierAppliedBlock;

			public IParameterTomate<ICombatObject> Target;

			public IParameterTomate<float> Amount;

			public IParameterTomate<ModifierData> Modifier;

			private IParameterTomate<float> OriginalAmount;
		}

		[Serializable]
		protected class ModifierEventParameters
		{
			[Restrict(false, new Type[]
			{
				typeof(float)
			})]
			public BaseParameter AmountParameter;

			[Restrict(false, new Type[]
			{
				typeof(float)
			})]
			public BaseParameter OriginalAmountParameter;

			[Restrict(false, new Type[]
			{
				typeof(ICombatObject)
			})]
			public BaseParameter TargetParameter;

			[Restrict(false, new Type[]
			{
				typeof(ModifierData)
			})]
			public BaseParameter ModifierParameter;
		}

		[Serializable]
		protected class ModifierInfoContainer
		{
			public bool HasEvent
			{
				get
				{
					return this._hasEvent;
				}
			}

			public void Initialize()
			{
				this._hasEvent = (this.OnModifierAppliedBlock != null);
			}

			public string Name;

			public BaseBlock OnModifierAppliedBlock;

			[Restrict(false, new Type[]
			{
				typeof(float)
			})]
			public BaseParameter Amount;

			public ModifierInfo Modifier;

			private bool _hasEvent;
		}
	}
}
