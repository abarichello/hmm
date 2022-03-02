using System;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using Pocketverse;

namespace HeavyMetalMachines.SkipSwordfish.CustomWS
{
	public class SkipSwordfishCustomWS : ICustomWS
	{
		public void ExecuteCustomWS(object state, string methodName, string args, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishCustomWS.Log.Info("Try ExecuteCustomWS with SkipSwordfish");
		}

		public void ExecuteCustomWSSync(string methodName, string args)
		{
			SkipSwordfishCustomWS.Log.Info("Try ExecuteCustomWSSync with SkipSwordfish");
		}

		public void ExecuteCustom(object state, CustomMethod customMethod, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishCustomWS.Log.Info("Try ExecuteCustom with SkipSwordfish");
		}

		public void ExecuteCustomSync(CustomMethod customMethod)
		{
			SkipSwordfishCustomWS.Log.Info("Try ExecuteCustomSync with SkipSwordfish");
		}

		public void ExecuteCustomWSWithReturn(object state, string methodName, string args, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishCustomWS.Log.Info("Try ExecuteCustomWSWithReturn with SkipSwordfish");
		}

		public string ExecuteCustomWSWithReturnSync(string methodName, string args)
		{
			SkipSwordfishCustomWS.Log.Info("Try ExecuteCustomWSWithReturnSync with SkipSwordfish");
			return string.Empty;
		}

		public void ExecuteCustomWithReturn(object state, CustomMethod customMethod, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishCustomWS.Log.Info("Try ExecuteCustomWithReturn with SkipSwordfish");
		}

		public string ExecuteCustomWithReturnSync(CustomMethod customMethod)
		{
			SkipSwordfishCustomWS.Log.Info("Try ExecuteCustomWithReturnSync with SkipSwordfish");
			return string.Empty;
		}

		public void GetApiNames(object state, SwordfishClientApi.ParameterizedCallback<string[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishCustomWS.Log.Info("Try GetApiNames with SkipSwordfish");
		}

		public string[] GetApiNamesSync()
		{
			SkipSwordfishCustomWS.Log.Info("Try GetApiNamesSync with SkipSwordfish");
			return new string[0];
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SkipSwordfishCustomWS));
	}
}
