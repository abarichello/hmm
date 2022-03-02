using System;
using HeavyMetalMachines.BI;

namespace HeavyMetalMachines.Swordfish
{
	public class ClientBILogger : IClientBILogger
	{
		public ClientBILogger(SwordfishServices swordfish)
		{
			this._services = swordfish;
		}

		public void BILogClientMsg(ClientBITags biTag, string msg, bool forceSendLogs)
		{
			this._services.Log.BILogClientMsg(biTag, msg, forceSendLogs);
		}

		private SwordfishServices _services;
	}
}
