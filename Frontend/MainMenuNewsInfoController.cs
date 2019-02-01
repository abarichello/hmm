using System;
using ClientAPI;
using ClientAPI.Utils;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Utils;
using Pocketverse;
using Swordfish.Common.exceptions;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuNewsInfoController : GameHubBehaviour
	{
		protected void Start()
		{
			MainMenu.PlayerReloadedEvent += this.OnPlayerReloadedEvent;
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryToSetNewsButtonEnabled(false);
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryOpenNewsCallback += this.TopMenuTryOpenNewsCallback;
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.CloseNewsCallback += this.TopMenuCloseNewsCallback;
		}

		protected void OnDestroy()
		{
			MainMenu.PlayerReloadedEvent -= this.OnPlayerReloadedEvent;
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryOpenNewsCallback -= this.TopMenuTryOpenNewsCallback;
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.CloseNewsCallback -= this.TopMenuCloseNewsCallback;
			MainMenuNewsModalWindow.ClearTextureCache();
		}

		private bool TopMenuTryOpenNewsCallback(bool isAutoOpen)
		{
			if (this._newsJsonObjects == null)
			{
				return false;
			}
			if (!isAutoOpen)
			{
				this._newsWindow.SetWindowVisibility(true);
				return true;
			}
			return this._newsWindow.TryToAutoOpenWindow();
		}

		private void TopMenuCloseNewsCallback()
		{
			if (this._newsJsonObjects == null)
			{
				return;
			}
			this._newsWindow.SetWindowVisibility(false);
		}

		private void OnPlayerReloadedEvent()
		{
			GameHubBehaviour.Hub.ClientApi.recommendation.GiveMeARecommendationFor(null, GameHubBehaviour.Hub.User.UniversalId, Language.CurrentLanguage().ToString(), SingletonMonoBehaviour<RegionController>.Instance.GetBestServerSaved().Region.RegionNameI18N, "mkt.url", new SwordfishClientApi.ParameterizedCallback<string>(this.OnLoadNewsDataOk), new SwordfishClientApi.ErrorCallback(this.LoadNewsDataError));
		}

		private int GetNewsHashInt(MainMenuNewsInfoController.NewsJsonObject[] newsJsonObjects)
		{
			int num = newsJsonObjects.Length;
			int num2 = num;
			for (int i = 0; i < num; i++)
			{
				num2 = num2 * 31 + newsJsonObjects[i].id;
			}
			return num2;
		}

		private void SetNewsBag(MainMenuNewsInfoController.NewsJsonObject[] newsJsonObjects)
		{
			int newsHashInt = this.GetNewsHashInt(newsJsonObjects);
			GameHubBehaviour.Hub.User.Bag.LastNewsId = newsHashInt;
			PlayerCustomWS.UpdatePlayerNews(GameHubBehaviour.Hub.User.Bag, new SwordfishClientApi.ParameterizedCallback<string>(this.OnUpdatePlayerNewsSuccess), new SwordfishClientApi.ErrorCallback(this.OnUpdatePlayerNewsError));
		}

		private void OnUpdatePlayerNewsError(object state, Exception exception)
		{
			MainMenuNewsInfoController.Log.ErrorFormat("Error on OnUpdatePlayerNewsError. PlayerId: {0}, Error: {1}", new object[]
			{
				GameHubBehaviour.Hub.User.PlayerSF.Id,
				exception.Message
			});
		}

		private void OnUpdatePlayerNewsSuccess(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<T>)obj);
			if (!netResult.Success)
			{
				MainMenuNewsInfoController.Log.ErrorFormat("Error on OnUpdatePlayerNewsSuccess. PlayerId: {0}, Error: {1}, Msg: {2}", new object[]
				{
					GameHubBehaviour.Hub.User.PlayerSF.Id,
					netResult.Error,
					netResult.Msg
				});
			}
		}

		private void LoadNewsDataError(object state, Exception exception)
		{
			HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("[CATFISH] LoadNewsDataAsync. Service is probably offline. CurrentLanguage:[{0}] ", Language.CurrentLanguage()), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
		}

		private void OnLoadNewsDataOk(object state, string strSourceJson)
		{
			if (string.IsNullOrEmpty(strSourceJson))
			{
				this.LoadNewsDataError(state, new InvalidArgumentException("Empty json data."));
				return;
			}
			this._newsJsonObjects = Json.ToObject<MainMenuNewsInfoController.NewsJsonObject[]>(strSourceJson);
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryToSetNewsButtonEnabled(true);
			bool flag = this._newsJsonObjects.Length > 0;
			int lastNewsId = GameHubBehaviour.Hub.User.Bag.LastNewsId;
			bool flag2 = false;
			if (this._checkUserSawNews)
			{
				flag2 = (flag && this.GetNewsHashInt(this._newsJsonObjects) == lastNewsId);
			}
			bool flag3 = flag && !flag2;
			MainMenuGui stateGuiController = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			bool flag4 = stateGuiController != null && stateGuiController.ActiveScreen == MainMenuGui.ActiveScreenKind.Lobby;
			this._newsWindow.gameObject.SetActive(true);
			this._newsWindow.Setup(this._newsJsonObjects, new EventDelegate.Callback(this.OnNewsWindowClose), new MainMenuNewsModalWindow.HmmSelectionDelegate(this.OnHmmSelection));
			if (flag4)
			{
				GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryOpenNewsOnLobbyReturn();
			}
		}

		private void OnHmmSelection(MainMenuNewsInfoController.NewsHmmSelectionType selectionType, Guid itemTypeId)
		{
			MainMenuNewsInfoController.Log.InfoFormat("Item type selection. Category:[{0}], ID:[{1}]", new object[]
			{
				selectionType,
				itemTypeId
			});
			if (GameHubBehaviour.Hub.ClientApi.lobby.IsInLobby())
			{
				MainMenuNewsInfoController.Log.Warn("Ignoring item type selection because we are inside custom match lobby.");
				return;
			}
			MainMenuGui stateGuiController = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			switch (selectionType)
			{
			case MainMenuNewsInfoController.NewsHmmSelectionType.Battlepass:
				stateGuiController.TryToOpenMetallpassPremiumShopWindow();
				break;
			case MainMenuNewsInfoController.NewsHmmSelectionType.Skins:
				stateGuiController.OpenShop(ShopGUI.Tab.Drivers, itemTypeId);
				break;
			case MainMenuNewsInfoController.NewsHmmSelectionType.Boosters:
				stateGuiController.OpenShop(ShopGUI.Tab.Boosters, Guid.Empty);
				break;
			case MainMenuNewsInfoController.NewsHmmSelectionType.Effects:
				stateGuiController.OpenShop(ShopGUI.Tab.Effects, itemTypeId);
				break;
			case MainMenuNewsInfoController.NewsHmmSelectionType.Sprays:
				stateGuiController.OpenShop(ShopGUI.Tab.Sprays, itemTypeId);
				break;
			case MainMenuNewsInfoController.NewsHmmSelectionType.Cash:
				stateGuiController.OpenShop(ShopGUI.Tab.Hoplons, Guid.Empty);
				break;
			}
			this._newsWindow.DisableButtonCollider();
			this._newsWindow.SetWindowVisibility(false);
		}

		private void OnNewsWindowClose()
		{
			if (!HMMHub.IsEditorLeavingPlayMode())
			{
				GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryToSetNewsButtonEnabled(true);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MainMenuNewsInfoController));

		[SerializeField]
		private bool _checkUserSawNews;

		[SerializeField]
		private float EnableNewsButtonDelayInSec = 3f;

		[SerializeField]
		private MainMenuNewsModalWindow _newsWindow;

		private MainMenuNewsInfoController.NewsJsonObject[] _newsJsonObjects;

		public enum NewsHmmSelectionType
		{
			Battlepass,
			Skins,
			Boosters,
			Effects,
			Sprays,
			Cash
		}

		public class NewsJsonObject
		{
			public int id { get; set; }

			public string thumb { get; set; }

			public string image { get; set; }

			public string link { get; set; }
		}
	}
}
