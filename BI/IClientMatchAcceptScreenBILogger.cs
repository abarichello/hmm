using System;

namespace HeavyMetalMachines.BI
{
	public interface IClientMatchAcceptScreenBILogger
	{
		void LogOpen(Guid matchId);

		void LogAccept(Guid matchId);

		void LogReject(Guid matchId);

		void LogTimeout(Guid matchId);
	}
}
