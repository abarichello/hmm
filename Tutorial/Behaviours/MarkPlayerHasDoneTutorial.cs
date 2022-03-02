using System;
using ClientAPI;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tutorial.InGame;
using HeavyMetalMachines.Utils;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class MarkPlayerHasDoneTutorial : InGameTutorialBehaviourBase
	{
		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			GameHubBehaviour.Hub.User.Bag.HasDoneTutorial = true;
			string msg = string.Format("UniversalId={0}", GameHubBehaviour.Hub.User.UserSF.UniversalID);
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(71, msg, true);
			this._biUtils.MarkConversion();
		}

		protected override void OnStepCompletedOnServer()
		{
			base.OnStepCompletedOnServer();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				return;
			}
			base.StartBehaviourOnServer();
			if (!this.IsPlayersCountValid())
			{
				return;
			}
			PlayerData playerData = GameHubBehaviour.Hub.Players.Players[0];
			if (playerData.Bag.HasDoneTutorial)
			{
				MarkPlayerHasDoneTutorial.Log.InfoFormat("Player already did the tutorial.", new object[0]);
				return;
			}
			PlayerCustomWS.MarkPlayerHasDoneTutorial(playerData.UserId, new SwordfishClientApi.ParameterizedCallback<string>(this.OnMarkPlayerHasDoneTutorialSuccess), new SwordfishClientApi.ErrorCallback(this.OnMarkPlayerHasDoneTutorialError));
		}

		private void OnMarkPlayerHasDoneTutorialError(object state, Exception exception)
		{
			MarkPlayerHasDoneTutorial.Log.ErrorFormat("Failed to mark player tutorial done. ex.: {0}", new object[]
			{
				exception
			});
		}

		private void OnMarkPlayerHasDoneTutorialSuccess(object state, string msg)
		{
			MarkPlayerHasDoneTutorial.Log.InfoFormat("Successfuly marked player tutorial done. Msg: {0}", new object[]
			{
				msg
			});
		}

		private bool IsPlayersCountValid()
		{
			int count = GameHubBehaviour.Hub.Players.Players.Count;
			if (count <= 0 || count > 1)
			{
				MarkPlayerHasDoneTutorial.Log.WarnFormat("Unexpected players amount: {0}", new object[]
				{
					count
				});
				return false;
			}
			return true;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MarkPlayerHasDoneTutorial));

		[InjectOnClient]
		private IBiUtils _biUtils;
	}
}
