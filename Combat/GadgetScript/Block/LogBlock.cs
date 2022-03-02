using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Debug/Log")]
	internal class LogBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			LogBlock.Log.DebugFormat(this.LogString(gadgetContext), new object[0]);
			return this._nextBlock;
		}

		private string LogString(object context)
		{
			this._logLine = string.Format("{0} Context: {1}", this._message, context);
			foreach (BaseParameter baseParameter in this._parametersToPrint)
			{
				this._logLine += string.Format(" {0}: {1}", baseParameter.name, baseParameter.ParameterTomate.GetBoxedValue(context));
			}
			return this._logLine;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(LogBlock));

		[SerializeField]
		private BaseParameter[] _parametersToPrint;

		[SerializeField]
		private string _message;

		private string _logLine;
	}
}
