using System;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using Hoplon.SensorSystem;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class BotGadgetScanner : IScanner
	{
		public BotGadgetScanner(SensorController controller, string shouldUseParameterName, string isIdleParameterName, BotAIGoalManager.GadgetAIState gadgetAiState)
		{
			this._shouldUseParameterHash = controller.GetHash(shouldUseParameterName);
			this._isIdleParameterHash = controller.GetHash(isIdleParameterName);
			this._gadgetAiState = gadgetAiState;
		}

		public void UpdateContext(SensorController context)
		{
			context.SetParameter(this._shouldUseParameterHash, (!this._gadgetAiState.Using) ? 0f : 1f);
			context.SetParameter(this._isIdleParameterHash, (this._gadgetAiState.GadgetState.GadgetState != GadgetState.Ready) ? 0f : 1f);
		}

		public override string ToString()
		{
			return string.Format("BotGadgetScanner _shouldUseParameterHash {0} _isIdleParameterHash {1}", this._shouldUseParameterHash, this._isIdleParameterHash);
		}

		public void Reset()
		{
		}

		private BotAIGoalManager.GadgetAIState _gadgetAiState;

		private int _shouldUseParameterHash;

		private int _isIdleParameterHash;

		private int _combatObjId;
	}
}
