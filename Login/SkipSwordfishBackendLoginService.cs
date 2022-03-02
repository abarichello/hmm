using System;
using System.Collections.Generic;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Login
{
	public class SkipSwordfishBackendLoginService : IBackendLoginService
	{
		public SkipSwordfishBackendLoginService(UserInfo userInfo, IConfigLoader config, IHMMPlayerPrefs hmmPlayerPrefs)
		{
			this._userInfo = userInfo;
			this._config = config;
			this._hmmPlayerPrefs = hmmPlayerPrefs;
		}

		public IObservable<BackendSession> Login()
		{
			return Observable.Return<BackendSession>(new BackendSession());
		}

		public IObservable<Unit> GetLoginData()
		{
			return Observable.DoOnCompleted<Unit>(Observable.ReturnUnit(), new Action(this.InitializeUserInfo));
		}

		public void CancelLogin()
		{
		}

		private void InitializeUserInfo()
		{
			this._userInfo.Name = this._config.GetValue(ConfigAccess.PlayerName);
			this._userInfo.UniversalId = "FakeUniversalId";
			this._userInfo.Bag = new PlayerBag
			{
				NickNameset = true,
				Level = 99,
				CurrentMatchId = null
			};
			BattlepassProgress battlepassProgress = new BattlepassProgress
			{
				MissionProgresses = new List<MissionProgress>(),
				MissionsCompleted = new List<MissionCompleted>(),
				PremiumLevelsClaimed = new bool[0]
			};
			this._userInfo.SetBattlepassProgress(battlepassProgress.ToString());
			this._userInfo.PlayerSF = new Player
			{
				Name = this._userInfo.Name,
				Bag = this._userInfo.Bag.ToString(),
				Id = (long)this._userInfo.Name.GetHashCode()
			};
			this._hmmPlayerPrefs.SkipSwordfishLoad();
		}

		private readonly UserInfo _userInfo;

		private readonly IConfigLoader _config;

		private readonly IHMMPlayerPrefs _hmmPlayerPrefs;
	}
}
