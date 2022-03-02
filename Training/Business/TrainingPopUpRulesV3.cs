using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Players.Business;
using Pocketverse;

namespace HeavyMetalMachines.Training.Business
{
	public class TrainingPopUpRulesV3 : ITrainingPopUpRulesV3, ITrainingPopUpRules
	{
		public TrainingPopUpRulesV3(IConfigLoader config, ILocalPlayerStorage playerStorage)
		{
			this._config = config;
			this._playerStorage = playerStorage;
		}

		public bool CanOpenPopUp()
		{
			int intValue = this._config.GetIntValue(ConfigAccess.EnableTrainingPopUpV3);
			if (intValue == 1)
			{
				return this.PlayerGroupUsesNewVersion() && this.PlayerNeverSeenPopUp();
			}
			if (intValue != 2)
			{
				return intValue == 666;
			}
			return this.PlayerNeverSeenPopUp();
		}

		private bool PlayerGroupUsesNewVersion()
		{
			return SplitTesting.GetGroupAB(this._playerStorage.Player.PlayerId) == 0;
		}

		private bool PlayerNeverSeenPopUp()
		{
			return false;
		}

		private IConfigLoader _config;

		private ILocalPlayerStorage _playerStorage;
	}
}
