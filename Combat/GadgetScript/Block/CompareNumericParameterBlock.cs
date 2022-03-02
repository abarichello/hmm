using System;
using HeavyMetalMachines.Common;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[Obsolete("Obsolete! Use FilterBlock")]
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

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
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

		[SerializeField]
		private BaseBlock _falseBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CompareNumericParameterBlock.Comparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class Comparison : IParameterComparison
		{
			public bool Compare(object context)
			{
				IParameterTomate<float> parameterTomate = this._parameter.ParameterTomate as IParameterTomate<float>;
				int num = parameterTomate.GetValue(context).CompareTo(this._valueToCompare);
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
