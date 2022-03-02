using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[Obsolete("Obsolete! Use FilterBlock")]
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/CompareBoolParameter")]
	public class CompareBoolParameterBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (CompareBoolParameterBlock._resultParameter == null)
			{
				CompareBoolParameterBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				CompareBoolParameterBlock._resultParameter.SetValue(gadgetContext, ParameterComparer.CompareParameter(gadgetContext, this._comparisons, this._booleanOperation));
				ihmmeventContext.SaveParameter(CompareBoolParameterBlock._resultParameter);
			}
			else
			{
				ihmmeventContext.LoadParameter(CompareBoolParameterBlock._resultParameter);
			}
			if (CompareBoolParameterBlock._resultParameter.GetValue(gadgetContext))
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
		private CompareBoolParameterBlock.Comparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class Comparison : IParameterComparison
		{
			public bool Compare(object context)
			{
				return this._parameter.GetValue(context) == this._valueToCompare;
			}

			public BaseParameter[] GetParameterArray()
			{
				return new BaseParameter[]
				{
					this._parameter
				};
			}

			[SerializeField]
			private BoolParameter _parameter;

			[SerializeField]
			private bool _valueToCompare;
		}
	}
}
