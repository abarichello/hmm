using System;
using HeavyMetalMachines.BI;

namespace HeavyMetalMachines.Swordfish
{
	public class UnidentifiedPlayerBiLogger : IUnidentifiedPlayerBiLogger
	{
		public UnidentifiedPlayerBiLogger(SwordfishServices swordfish)
		{
			this._swordfish = swordfish;
		}

		public void BiLogClientMsg(IUnidentifiedPlayerBiLog unidentifiedPlayerBiLog)
		{
			this._swordfish.Log.LogInstallationMessage(unidentifiedPlayerBiLog);
		}

		private readonly SwordfishServices _swordfish;
	}
}
