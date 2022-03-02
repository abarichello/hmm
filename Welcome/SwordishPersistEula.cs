using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Login;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.Welcome
{
	public class SwordishPersistEula : IPersistCurrentEulaVersion
	{
		public SwordishPersistEula(ICustomWS customWs, IGetExpectedEulaVersion getExpectedEulaVersion)
		{
			this._customWs = customWs;
			this._getExpectedEulaVersion = getExpectedEulaVersion;
		}

		public IObservable<Unit> Persist()
		{
			return Observable.ContinueWith<string, Unit>(this._getExpectedEulaVersion.Get(), new Func<string, IObservable<Unit>>(this.CallSwordishPersist));
		}

		private IObservable<Unit> CallSwordishPersist(string eulaVersion)
		{
			return SwordfishObservable.FromNetResultSwordfishCall(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				this._customWs.ExecuteCustomWSWithReturn(null, "PersistEulaVersionAccepted", eulaVersion, success, error);
			});
		}

		private readonly ICustomWS _customWs;

		private readonly IGetExpectedEulaVersion _getExpectedEulaVersion;
	}
}
