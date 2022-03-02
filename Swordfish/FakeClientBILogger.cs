using System;
using HeavyMetalMachines.BI;

namespace HeavyMetalMachines.Swordfish
{
	public class FakeClientBILogger : IClientBILogger
	{
		public void BILogClientMsg(ClientBITags biTag, string msg, bool forceSendLogs)
		{
		}
	}
}
