using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Modifier/ApplyModifier")]
	public class ApplyModifiersBlock : BaseBlock, IGadgetBlockWithAsset
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
			ModifierData[] datas;
			if (numericParameter != null)
			{
				datas = ModifierData.CreateData(this._modifiers, numericParameter.GetFloatValue(context));
			}
			else
			{
				datas = ModifierData.CreateData(this._modifiers);
			}
			ICombatController modifierController = this._target.GetValue(context).ModifierController;
			if (this._direction == null || this._position == null)
			{
				modifierController.AddModifiers(datas, ihmmgadgetContext.GetCombatObject(ihmmgadgetContext.OwnerId), -1, false);
			}
			else
			{
				modifierController.AddModifiers(datas, ihmmgadgetContext.GetCombatObject(ihmmgadgetContext.OwnerId), -1, this._direction.GetValue(context), this._position.GetValue(context), false);
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._amount, parameterId) || base.CheckIsParameterWithId(this._position, parameterId) || base.CheckIsParameterWithId(this._direction, parameterId) || base.CheckIsParameterWithId(this._target, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private Vector3Parameter _direction;

		[SerializeField]
		private Vector3Parameter _position;

		[SerializeField]
		private BaseParameter _amount;

		[SerializeField]
		private ModifierInfo[] _modifiers;
	}
}
