using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Automated;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class Starter : GameState
	{
		protected override void OnStateEnabled()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.HORTA))
			{
				base.GoToState(this.Horta, false);
				return;
			}
			if (GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.FastTestChar) != -1)
			{
				this._hub = GameHubBehaviour.Hub;
				this._hub.gameObject.AddComponent<FastTestChar>();
				this._hub.PlayerPrefs.SkipSwordfishLoad();
				this._hub.Server.ServerIp = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.ServerIP);
				this._hub.Server.ServerPort = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.ServerPort);
				this._hub.Swordfish.Msg.ClientMatchId = Guid.Empty;
				this._hub.User.Name = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.PlayerName);
				this._hub.User.UserSF.Id = Guid.NewGuid();
				this._hub.User.PlayerSF.Name = this._hub.User.Name;
				this._hub.User.PlayerSF.Id = (long)this._hub.User.Name.GetHashCode();
				this._hub.User.ConnectToServer(false, delegate
				{
					GameHubBehaviour.Hub.State.GotoState(this.Splash, false);
				}, null);
				foreach (ItemTypeScriptableObject itemTypeScriptableObject in GameHubBehaviour.Hub.InventoryColletion.ItemTypes)
				{
					GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[itemTypeScriptableObject.Id] = itemTypeScriptableObject;
				}
			}
			else
			{
				Starter.StateToGo stateToGo = this.stateToGo;
				if (stateToGo != Starter.StateToGo.Splash)
				{
					if (stateToGo == Starter.StateToGo.Profile)
					{
						base.GoToState(this.Profile, false);
					}
				}
				else
				{
					base.GoToState(this.Splash, false);
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(Starter));

		private HMMHub _hub;

		[SerializeField]
		private Profile Profile;

		[SerializeField]
		private Splash Splash;

		[SerializeField]
		private Starter.StateToGo stateToGo;

		[SerializeField]
		private HORTAState Horta;

		private enum StateToGo
		{
			Profile,
			Splash
		}
	}
}
