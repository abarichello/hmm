using System;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishCustomWSProxy : ICustomWS
	{
		public void ExecuteCustomWS(object state, string methodName, string args, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("ExecuteCustomWS MethodName = {0}, Args = {1}", new object[]
			{
				methodName,
				args
			});
			this.CustomWS.ExecuteCustomWS(state, methodName, args, callback, errorCallback);
		}

		public void ExecuteCustomWSSync(string methodName, string args)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("ExecuteCustomWSSync MethodName = {0}, Args = {1}", new object[]
			{
				methodName,
				args
			});
			this.CustomWS.ExecuteCustomWSSync(methodName, args);
		}

		public void ExecuteCustom(object state, CustomMethod customMethod, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("ExecuteCustom MethodName = {0}, Args = {1}", new object[]
			{
				customMethod.MethodName,
				customMethod.Args
			});
			this.CustomWS.ExecuteCustom(state, customMethod, callback, errorCallback);
		}

		public void ExecuteCustomSync(CustomMethod customMethod)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("ExecuteCustomSync MethodName = {0}, Args = {1}", new object[]
			{
				customMethod.MethodName,
				customMethod.Args
			});
			this.CustomWS.ExecuteCustomSync(customMethod);
		}

		public void ExecuteCustomWSWithReturn(object state, string methodName, string args, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("ExecuteCustomWSWithReturn MethodName = {0}, Args = {1}", new object[]
			{
				methodName,
				args
			});
			this.CustomWS.ExecuteCustomWSWithReturn(state, methodName, args, callback, errorCallback);
		}

		public string ExecuteCustomWSWithReturnSync(string methodName, string args)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("ExecuteCustomWSWithReturnSync MethodName = {0}, Args = {1}", new object[]
			{
				methodName,
				args
			});
			return this.CustomWS.ExecuteCustomWSWithReturnSync(methodName, args);
		}

		public void ExecuteCustomWithReturn(object state, CustomMethod customMethod, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("ExecuteCustomWithReturn MethodName = {0}, Args = {1}", new object[]
			{
				customMethod.MethodName,
				customMethod.Args
			});
			this.CustomWS.ExecuteCustomWithReturn(state, customMethod, callback, errorCallback);
		}

		public string ExecuteCustomWithReturnSync(CustomMethod customMethod)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("ExecuteCustomWithReturnSync MethodName = {0}, Args = {1}", new object[]
			{
				customMethod.MethodName,
				customMethod.Args
			});
			return this.CustomWS.ExecuteCustomWithReturnSync(customMethod);
		}

		public void GetApiNames(object state, SwordfishClientApi.ParameterizedCallback<string[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SwordfishCustomWSProxy.Log.InfoFormat("GetApiNames", new object[0]);
			this.CustomWS.GetApiNames(state, callback, errorCallback);
		}

		public string[] GetApiNamesSync()
		{
			SwordfishCustomWSProxy.Log.InfoFormat("GetApiNamesSync", new object[0]);
			return this.CustomWS.GetApiNamesSync();
		}

		private ICustomWS CustomWS
		{
			get
			{
				if (this._customWS != null)
				{
					return this._customWS;
				}
				this._customWS = GameHubBehaviour.Hub.ClientApi.customws;
				if (this._customWS == null)
				{
					SwordfishCustomWSProxy.Log.Error("Try to use CustomWS before connecting to swordfish. Rethink your initialization proccess.");
				}
				return this._customWS;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SwordfishCustomWSProxy));

		private ICustomWS _customWS;
	}
}
