using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Players.Business;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Battlepass
{
	public class ObserveBattlepassProgress : IObserveBattlepassProgress
	{
		public ObserveBattlepassProgress(SharedConfigs sharedConfigs)
		{
			this._sharedConfigs = sharedConfigs;
		}

		public IObservable<int> ObserveLevelChanged()
		{
			return Observable.Select<BattlepassProgress, int>(this.ObserveProgressChanged(), new Func<BattlepassProgress, int>(this.CalculateLevel));
		}

		private int CalculateLevel(BattlepassProgress battlepassProgress)
		{
			int currentXp = battlepassProgress.CurrentXp;
			BattlepassConfig battlepass = this._sharedConfigs.Battlepass;
			return battlepass.GetLevelForXp(currentXp);
		}

		public IObservable<BattlepassProgress> ObserveProgressChanged()
		{
			return Observable.FromEvent<BattlepassProgress>(delegate(Action<BattlepassProgress> handler)
			{
				BattlepassProgressScriptableObject.OnBattlepassProgressSet += handler;
			}, delegate(Action<BattlepassProgress> handler)
			{
				BattlepassProgressScriptableObject.OnBattlepassProgressSet -= handler;
			});
		}

		private readonly SharedConfigs _sharedConfigs;
	}
}
