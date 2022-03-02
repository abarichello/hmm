using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Print Float")]
	internal class TESTPrintFloatBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IParameterTomate<float> parameterTomate = this._parameterToPrint.ParameterTomate as IParameterTomate<float>;
			TESTPrintFloatBlock.Log.DebugFormat("PrintBlock {0}: {1}", new object[]
			{
				this._prefix,
				parameterTomate.GetValue(gadgetContext)
			});
			return this._nextBlock;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(TESTPrintFloatBlock));

		[SerializeField]
		private BaseParameter _parameterToPrint;

		[SerializeField]
		private string _prefix;
	}
}
