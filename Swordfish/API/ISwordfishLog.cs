using System;
using HeavyMetalMachines.BI;

namespace HeavyMetalMachines.Swordfish.API
{
	public interface ISwordfishLog
	{
		void BILogServerMsg(ServerBITags biTag, string msg, bool forceSendLogs);

		void BILogClientMsg(ClientBITags biTag, string msg, bool forceSendLogs);

		void BILogClientCloseCondition(string closeData, bool isFirstLogin, bool joinedMatchmaking);

		void BILogClient(ClientBITags biTag, bool forceSendLogs);

		void BILogClientMatchMsg(ClientBITags biTag, string msg, bool forceSendLogs);

		void BILogClientMatch(ClientBITags biTag, bool forceSendLogs);

		void LogStatistic(string type, string msg, bool perm, bool forceSendLogs);

		void Update();
	}
}
