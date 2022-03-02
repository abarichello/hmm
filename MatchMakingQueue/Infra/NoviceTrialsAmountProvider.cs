using System;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.DataTransferObjects.Matchs;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public class NoviceTrialsAmountProvider : INoviceTrialsAmountProvider
	{
		public NoviceTrialsAmountProvider(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public int GetNoviceTrialsAmount()
		{
			return this._noviceTrialsAmount;
		}

		public IObservable<Unit> InitializeProvider()
		{
			return Observable.AsUnitObservable<SerializableNoviceTrialsData>(Observable.Do<SerializableNoviceTrialsData>(this._customWs.ExecuteAsObservable("GetNoviceTrialsAmount", string.Empty), delegate(SerializableNoviceTrialsData novice)
			{
				this.SetNoviceTrialsAmount(novice.NoviceTrialsAmount);
			}));
		}

		private void SetNoviceTrialsAmount(int noviceTrials)
		{
			this._noviceTrialsAmount = noviceTrials;
		}

		private readonly ICustomWS _customWs;

		private int _noviceTrialsAmount;
	}
}
