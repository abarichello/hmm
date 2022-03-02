using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[Obsolete("Obsolete! Use FilterBlock")]
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/CompareCombatNull")]
	public class CompareCombatNullBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (CompareCombatNullBlock._resultParameter == null)
			{
				CompareCombatNullBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				CompareCombatNullBlock._resultParameter.SetValue(gadgetContext, ParameterComparer.CompareParameter(gadgetContext, this._comparisons, this._booleanOperation));
				ihmmeventContext.SaveParameter(CompareCombatNullBlock._resultParameter);
			}
			else
			{
				ihmmeventContext.LoadParameter(CompareCombatNullBlock._resultParameter);
			}
			if (CompareCombatNullBlock._resultParameter.GetValue(gadgetContext))
			{
				return this._nextBlock;
			}
			return this._combatNotNullBlock;
		}

		[SerializeField]
		private BaseBlock _combatNotNullBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CompareCombatNullBlock.Comparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class Comparison : IParameterComparison
		{
			public bool Compare(object context)
			{
				return this._combat.GetValue(context) == null;
			}

			public BaseParameter[] GetParameterArray()
			{
				return new BaseParameter[]
				{
					this._combat
				};
			}

			[SerializeField]
			private CombatObjectParameter _combat;
		}
	}
}
