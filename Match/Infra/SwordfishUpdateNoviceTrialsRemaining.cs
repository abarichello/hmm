using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.Match.Infra
{
	public class SwordfishUpdateNoviceTrialsRemaining : IUpdateNoviceTrialsRemaining
	{
		public SwordfishUpdateNoviceTrialsRemaining(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<Unit> Update(long playerId)
		{
			return SwordfishObservable.FromNetResultSwordfishCall(delegate(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
			{
				this._customWs.ExecuteCustomWSWithReturn(null, "UpdateNoviceTrialsRemaining", playerId.ToString(), onSuccess, onError);
			});
		}

		private readonly ICustomWS _customWs;
	}
}
