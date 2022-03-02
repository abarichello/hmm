using System;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;

namespace HeavyMetalMachines.BI
{
	public class ClientMatchAcceptScreenBILogger : IClientMatchAcceptScreenBILogger
	{
		public ClientMatchAcceptScreenBILogger(IClientBILogger logger)
		{
			this._logger = logger;
		}

		public void LogOpen(Guid matchId)
		{
			this.Log(matchId, ClientMatchAcceptScreenBILogger.MatchAcceptScreenBIEventType.Open);
		}

		public void LogAccept(Guid matchId)
		{
			this.Log(matchId, ClientMatchAcceptScreenBILogger.MatchAcceptScreenBIEventType.Accept);
		}

		public void LogReject(Guid matchId)
		{
			this.Log(matchId, ClientMatchAcceptScreenBILogger.MatchAcceptScreenBIEventType.Reject);
		}

		public void LogTimeout(Guid matchId)
		{
			this.Log(matchId, ClientMatchAcceptScreenBILogger.MatchAcceptScreenBIEventType.Timeout);
		}

		private void Log(Guid matchId, ClientMatchAcceptScreenBILogger.MatchAcceptScreenBIEventType eventType)
		{
			ClientMatchAcceptScreenBILogger.MatchAcceptScreenBISerializable matchAcceptScreenBISerializable = new ClientMatchAcceptScreenBILogger.MatchAcceptScreenBISerializable();
			matchAcceptScreenBISerializable.MatchId = matchId;
			matchAcceptScreenBISerializable.EventType = eventType.ToString();
			this._logger.BILogClientMsg(91, matchAcceptScreenBISerializable.ToString(), true);
		}

		private readonly IClientBILogger _logger;

		public class MatchAcceptScreenBISerializable : JsonSerializeable<ClientMatchAcceptScreenBILogger.MatchAcceptScreenBISerializable>
		{
			public Guid MatchId;

			public string EventType;
		}

		private enum MatchAcceptScreenBIEventType
		{
			Open,
			Accept,
			Reject,
			Timeout
		}
	}
}
