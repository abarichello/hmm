using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuFooterController : GameHubBehaviour
	{
		private MainMenuGui MainMenuGui
		{
			get
			{
				return (!(this._mainMenuGui == null)) ? this._mainMenuGui : (this._mainMenuGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>());
			}
		}

		protected void Start()
		{
			MainMenuGui.OnLobbyUpdate += this.MainMenuGuiOnLobbyUpdate;
		}

		protected void OnDestroy()
		{
			MainMenuGui.OnLobbyUpdate -= this.MainMenuGuiOnLobbyUpdate;
		}

		private void MainMenuGuiOnLobbyUpdate(bool isInLobby)
		{
			this.WindowGameObject.SetActive(isInLobby);
		}

		public void OnClickSupportButton()
		{
			this.OpenUrl(GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SupportPageURL) + "?lang=" + Language.CurrentLanguage(), ClientBITags.SupportSiteOpenFromFooter);
		}

		public void OnClickFacebookButton()
		{
			this.OpenUrl(GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.FacebookPageURL), ClientBITags.FacebookSiteOpenFromFooter);
		}

		public void OnClickVkButton()
		{
			this.OpenUrl(GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.VkPageURL), ClientBITags.VkSiteOpenFromFooter);
		}

		public void OnClickDiscordButton()
		{
			this.OpenUrl(GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.DiscordPageURL), ClientBITags.DiscordOpenFromFooter);
		}

		public void OnClickInstagramButton()
		{
			LanguageCode languageCode = Language.CurrentLanguage();
			if (languageCode == LanguageCode.PT || languageCode == LanguageCode.PT_BR)
			{
				this.OpenUrl(GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.InstagramBRPageURL), ClientBITags.InstagramOpenFromFooter);
				return;
			}
			this.OpenUrl(GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.InstagramPageURL), ClientBITags.InstagramOpenFromFooter);
		}

		private void OpenUrl(string url, ClientBITags clientBiTags)
		{
			OpenUrlUtils.OpenUrl(url);
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(clientBiTags, true);
			}
		}

		public GameObject WindowGameObject;

		private MainMenuGui _mainMenuGui;
	}
}
