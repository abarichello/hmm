using System;
using System.Collections.Generic;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/FilterBlock")]
	public class FilterBlock : BaseBlock
	{
		public List<ScriptableObject> FilterAssets
		{
			get
			{
				if (this._comparisonsAssets == null)
				{
					this._comparisonsAssets = new List<ScriptableObject>();
				}
				return this._comparisonsAssets;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (FilterBlock._resultParameter == null)
			{
				FilterBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
			this._comparisons = new IParameterComparison[this.FilterAssets.Count];
			for (int i = 0; i < this.FilterAssets.Count; i++)
			{
				this._comparisons[i] = (IParameterComparison)this.FilterAssets[i];
			}
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				FilterBlock._resultParameter.SetValue(gadgetContext, ParameterComparer.CompareParameter(gadgetContext, this._comparisons, this._booleanOperation));
				ihmmeventContext.SaveParameter(FilterBlock._resultParameter);
			}
			else
			{
				ihmmeventContext.LoadParameter(FilterBlock._resultParameter);
			}
			if (FilterBlock._resultParameter.GetValue(gadgetContext))
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
		[HideInInspector]
		private List<ScriptableObject> _comparisonsAssets;

		private IParameterComparison[] _comparisons;

		private static BoolParameter _resultParameter;
	}
}
