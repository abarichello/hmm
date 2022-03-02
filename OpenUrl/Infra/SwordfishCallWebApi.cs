using System;
using System.Text;
using ClientAPI;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.OpenUrl.Infra
{
	public class SwordfishCallWebApi : ICallWebApi
	{
		public SwordfishCallWebApi(SwordfishClientApi swordfishClientApi, ILogger<SwordfishCallWebApi> logger)
		{
			this._swordfishClientApi = swordfishClientApi;
			this._logger = logger;
		}

		public IObservable<string> Get(string url, params string[] getParameters)
		{
			string built = this.BuildGetUrl(url, getParameters);
			return Observable.Do<string>(Observable.ContinueWith<Unit, string>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._logger.InfoFormat("Calling  Get {0}", new object[]
				{
					built
				});
			}), this.CallUrl(built)), delegate(string _)
			{
				this._logger.InfoFormat("Received Get {0}", new object[]
				{
					built
				});
			});
		}

		private IObservable<string> CallUrl(string url)
		{
			return SwordfishObservable.FromSwordfishCall<string>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				this._swordfishClientApi.internet.Get(null, url, success, error);
			});
		}

		private string BuildGetUrl(string url, string[] getParameters)
		{
			if (getParameters == null || getParameters.Length == 0)
			{
				return url;
			}
			StringBuilder stringBuilder = new StringBuilder(url);
			bool flag = url.Contains("?");
			for (int i = 1; i < getParameters.Length; i += 2)
			{
				if (flag)
				{
					stringBuilder.AppendFormat("&{0}={1}", getParameters[i - 1], getParameters[i]);
				}
				else
				{
					stringBuilder.AppendFormat("?{0}={1}", getParameters[i - 1], getParameters[i]);
					flag = true;
				}
			}
			return stringBuilder.ToString();
		}

		private readonly SwordfishClientApi _swordfishClientApi;

		private readonly ILogger<SwordfishCallWebApi> _logger;
	}
}
