using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Modifier/ApplyPassiveModifier")]
	public class ApplyPassiveModifiersBlock : BaseBlock, IGadgetBlockWithAsset
	{
		public void PrecacheAssets()
		{
			for (int i = 0; i < this._modifiers.Length; i++)
			{
				if (this._modifiers[i].Feedback != null)
				{
					ResourceLoader.Instance.PreCachePrefab(this._modifiers[i].Feedback.Name, this._modifiers[i].Feedback.EffectPreCacheCount);
				}
			}
		}

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext context, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)context;
			if (ihmmgadgetContext.IsClient)
			{
				return this._nextBlock;
			}
			if (this._target == null || this._target.GetValue(context) == null)
			{
				return this._nextBlock;
			}
			INumericParameter numericParameter = this._amount as INumericParameter;
			ModifierData[] array;
			if (numericParameter != null)
			{
				array = ModifierData.CreateData(this._modifiers, numericParameter.GetFloatValue(context));
			}
			else
			{
				array = ModifierData.CreateData(this._modifiers);
			}
			ICombatController modifierController = this._target.GetValue(context).ModifierController;
			modifierController.AddPassiveModifiers(array, ihmmgadgetContext.GetCombatObject(ihmmgadgetContext.OwnerId), -1);
			this._modifiersParameter.SetValue(context, array);
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._amount, parameterId) || base.CheckIsParameterWithId(this._target, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private BaseParameter _amount;

		[SerializeField]
		private ModifierInfo[] _modifiers;

		[Header("Write")]
		[SerializeField]
		private ModifierDataArrayParameter _modifiersParameter;
	}
}
