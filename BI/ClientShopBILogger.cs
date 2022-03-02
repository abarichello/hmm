using System;
using HeavyMetalMachines.Swordfish;
using UnityEngine;

namespace HeavyMetalMachines.BI
{
	public class ClientShopBILogger : IClientShopBILogger
	{
		public ClientShopBILogger(IClientBILogger clientBiLogger)
		{
			this._clientBiLogger = clientBiLogger;
		}

		public void Log(ClientShopBiOrigin origin, ClientShopBiLanding landing)
		{
			this._clientBiLogger.BILogClientMsg(129, ClientShopBILogger.ParseLog(origin, landing), true);
		}

		private static string ParseLog(ClientShopBiOrigin origin, ClientShopBiLanding landing)
		{
			ClientShopBILogger.ClientShopBiJson clientShopBiJson = new ClientShopBILogger.ClientShopBiJson
			{
				Origin = origin.ToString(),
				LandingPage = landing.ToString()
			};
			return JsonUtility.ToJson(clientShopBiJson);
		}

		public static void LegacyLog(HMMHub hub, ClientShopBiOrigin origin, ClientShopBiLanding landing)
		{
			hub.Swordfish.Log.BILogClientMsg(129, ClientShopBILogger.ParseLog(origin, landing), true);
		}

		private readonly IClientBILogger _clientBiLogger;

		[Serializable]
		private struct ClientShopBiJson
		{
			public string Origin;

			public string LandingPage;
		}
	}
}
