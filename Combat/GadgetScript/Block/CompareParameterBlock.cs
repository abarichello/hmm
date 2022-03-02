using System;
using HeavyMetalMachines.Common;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[Obsolete("Obsolete! Use FilterBlock")]
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/CompareParameter")]
	public class CompareParameterBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (CompareParameterBlock._resultParameter == null)
			{
				CompareParameterBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				CompareParameterBlock._resultParameter.SetValue(gadgetContext, ParameterComparer.CompareParameter(gadgetContext, this._comparisons, this._booleanOperation));
				ihmmeventContext.SaveParameter(CompareParameterBlock._resultParameter);
			}
			else
			{
				ihmmeventContext.LoadParameter(CompareParameterBlock._resultParameter);
			}
			if (CompareParameterBlock._resultParameter.GetValue(gadgetContext))
			{
				return this._nextBlock;
			}
			return this._failureBlock;
		}

		[SerializeField]
		private BaseBlock _failureBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CompareParameterBlock.Comparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class Comparison : IParameterComparison
		{
			public bool Compare(object context)
			{
				int num = this._firstParameter.CompareTo(context, this._secondParameter);
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
					this._firstParameter,
					this._secondParameter
				};
			}

			[SerializeField]
			private ComparisonType _comparisonType;

			[SerializeField]
			private BaseParameter _firstParameter;

			[SerializeField]
			private BaseParameter _secondParameter;
		}
	}
}
