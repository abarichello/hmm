using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
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

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			return !((IHMMGadgetContext)gadgetContext).IsClient || true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
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
		private BaseBlock _combatNotNullBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CompareCombatNullBlock.Comparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class Comparison : IParameterComparison, IUsedParametersArray
		{
			public bool Compare(IParameterContext context)
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
