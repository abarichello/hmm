using System;
using HeavyMetalMachines.Common;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/CompareNumericParameter")]
	public class CompareNumericParameterBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (CompareNumericParameterBlock._resultParameter == null)
			{
				CompareNumericParameterBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			return !((IHMMGadgetContext)gadgetContext).IsClient || true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				CompareNumericParameterBlock._resultParameter.SetValue(gadgetContext, ParameterComparer.CompareParameter(gadgetContext, this._comparisons, this._booleanOperation));
				ihmmeventContext.SaveParameter(CompareNumericParameterBlock._resultParameter);
			}
			else
			{
				ihmmeventContext.LoadParameter(CompareNumericParameterBlock._resultParameter);
			}
			if (CompareNumericParameterBlock._resultParameter.GetValue(gadgetContext))
			{
				return this._nextBlock;
			}
			return this._falseBlock;
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

		[SerializeField]
		private BaseBlock _falseBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CompareNumericParameterBlock.Comparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class Comparison : IParameterComparison, IUsedParametersArray
		{
			public bool Compare(IParameterContext context)
			{
				int num = ((INumericParameter)this._parameter).GetFloatValue(context).CompareTo(this._valueToCompare);
				switch (this._comparisonType)
				{
				case ComparisonType.Equal:
					return num == 0;
				case ComparisonType.Greater:
					return num > 0;
				case ComparisonType.Lesser:
					return num < 0;
				case ComparisonType.EqualOrGreater:
					return num >= 0;
				case ComparisonType.EqualOrLesser:
					return num <= 0;
				default:
					return false;
				}
			}

			public BaseParameter[] GetParameterArray()
			{
				return new BaseParameter[]
				{
					this._parameter
				};
			}

			[SerializeField]
			private ComparisonType _comparisonType;

			[SerializeField]
			private BaseParameter _parameter;

			[SerializeField]
			private float _valueToCompare;
		}
	}
}
