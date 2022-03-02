using System;
using System.Collections;
using Assets.ClientApiObjects;
using HeavyMetalMachines.CharacterSelection.Client;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Welcome;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class Starter : GameState
	{
		protected override void OnStateEnabled()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.DoOnCompleted<Unit>(this._diContainer.Resolve<IExecuteStarterState>().Execute(), new Action(this.ContinueToNextState)));
		}

		private void ContinueToNextState()
		{
			if (this._config.GetBoolValue(ConfigAccess.HORTA))
			{
				this._horta.Enabled = true;
				base.GoToState(this.Horta, false);
				return;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.DirectMatch) && GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				GameHubBehaviour.Hub.PlayerPrefs.SkipSwordfishLoad();
				GameHubBehaviour.Hub.Server.ServerIp = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.ServerIP);
				GameHubBehaviour.Hub.Server.ServerPort = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.ServerPort);
				GameHubBehaviour.Hub.Swordfish.Msg.ClientMatchId = Guid.Empty;
				GameHubBehaviour.Hub.User.Name = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.PlayerName);
				GameHubBehaviour.Hub.User.UserSF.Id = Guid.NewGuid();
				GameHubBehaviour.Hub.User.PlayerSF.Name = GameHubBehaviour.Hub.User.Name;
				GameHubBehaviour.Hub.User.PlayerSF.Id = (long)GameHubBehaviour.Hub.User.Name.GetHashCode();
				foreach (ItemTypeScriptableObject itemTypeScriptableObject in GameHubBehaviour.Hub.InventoryColletion.ItemTypes)
				{
					if (itemTypeScriptableObject.IsActive)
					{
						GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[itemTypeScriptableObject.Id] = itemTypeScriptableObject;
					}
				}
				base.StartCoroutine(this.DirectMatchConnectToServer());
				GameHubBehaviour.Hub.User.OnMatchDataReceived += this.SkipSwordfishUserDataReceived;
			}
			else
			{
				this.GoToWelcome();
			}
		}

		private IEnumerator DirectMatchConnectToServer()
		{
			yield return new WaitForEndOfFrame();
			GameHubBehaviour.Hub.User.ConnectToServer(false, new Action(this.GoToWelcome), null);
			yield break;
		}

		private void SkipSwordfishUserDataReceived(MatchData.MatchState matchstate)
		{
			IProceedToClientCharacterSelectionState proceedToClientCharacterSelectionState = this._diContainer.Resolve<IProceedToClientCharacterSelectionState>();
			proceedToClientCharacterSelectionState.Proceed();
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.User.OnMatchDataReceived -= this.SkipSwordfishUserDataReceived;
		}

		private void GoToWelcome()
		{
			base.GoToState(this._welcomeGameStateBehaviour, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(Starter));

		[Inject]
		private HORTAComponent _horta;

		[Inject]
		private IConfigLoader _config;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		[Inject]
		private DiContainer _diContainer;

		[SerializeField]
		private WelcomeGameStateBehaviour _welcomeGameStateBehaviour;

		[SerializeField]
		private HORTAState Horta;
	}
}
