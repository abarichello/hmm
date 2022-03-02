using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[Obsolete("Obsolete! Use FilterBlock")]
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/CompareEffectKind")]
	public class CompareEffectKindBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
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
		private class Comparison : IParameterComparison
		{
			public bool Compare(object context)
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
