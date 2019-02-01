using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/CompareEffectKind")]
	public class CompareEffectKindBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			return !((IHMMGadgetContext)gadgetContext).IsClient || true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			bool flag;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				flag = ParameterComparer.CompareParameter(gadgetContext, this._comparisons, this._booleanOperation);
				CompareEffectKindBlock._serverResult.SetValue(gadgetContext, flag);
				((IHMMEventContext)eventContext).SaveParameter(CompareEffectKindBlock._serverResult);
			}
			else
			{
				((IHMMEventContext)eventContext).LoadParameter(CompareEffectKindBlock._serverResult);
				flag = CompareEffectKindBlock._serverResult.GetValue(gadgetContext);
			}
			if (flag)
			{
				return this._nextBlock;
			}
			return this._kindNotEqualBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			for (int i = 0; i < this._comparisons.Length; i++)
			{
				BaseParameter[] parameterArray = this._comparisons[i].GetParameterArray();
				for (int j = 0; j < parameterArray.Length; j++)
				{
					if (base.CheckIsParameterWithId(parameterArray[j], parameterId))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			CompareEffectKindBlock._serverResult = ScriptableObject.CreateInstance<BoolParameter>();
		}

		[SerializeField]
		private BaseBlock _kindNotEqualBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CompareEffectKindBlock.Comparison[] _comparisons;

		private static BoolParameter _serverResult;

		[Serializable]
		private class Comparison : IParameterComparison, IUsedParametersArray
		{
			public bool Compare(IParameterContext context)
			{
				return this._effectKind == this._parameter.GetValue(context).Info.Effect;
			}

			public BaseParameter[] GetParameterArray()
			{
				return new BaseParameter[]
				{
					this._parameter
				};
			}

			[SerializeField]
			private ModifierDataParameter _parameter;

			[SerializeField]
			private EffectKind _effectKind;
		}
	}
}
