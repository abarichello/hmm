﻿using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class ReconnectMenuGui : StateGuiController
	{
		public ReconnectMenu ReconnectMenu
		{
			get
			{
				if (this._reconnectMenu == null)
				{
					this._reconnectMenu = (ReconnectMenu)GameHubBehaviour.Hub.State.Current;
				}
				return this._reconnectMenu;
			}
			set
			{
				this._reconnectMenu = value;
			}
		}

		public void ShowButtons()
		{
			string key;
			string key2;
			string key3;
			Action onRefuse;
			if (GameHubBehaviour.Hub.User.Bag.CurrentIsNarrator)
			{
				key = "RECONNECT_QUESTION_SPECTATOR";
				key2 = "CLOSEGAME_YES";
				key3 = "CLOSEGAME_NO";
				onRefuse = new Action(this.NarratorGoBackToMain);
			}
			else
			{
				key = "RECONNECT_QUESTION";
				key2 = "RETRY_RECONNECT_BUTTON";
				key3 = "CANCEL_RECONNECT_BUTTON";
				onRefuse = new Action(this.ExitGame);
			}
			Guid guid = Guid.NewGuid();
			this.currentConfirmWindowProperties = new ConfirmWindowProperties
			{
				Guid = guid,
				QuestionText = string.Format(Language.Get(key, TranslationSheets.MainMenuGui), new object[0]),
				OnConfirm = new Action(this.BackToGame),
				OnRefuse = onRefuse,
				ConfirmButtonText = Language.Get(key2, TranslationSheets.MainMenuGui),
				RefuseButtonText = Language.Get(key3, TranslationSheets.MainMenuGui)
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(this.currentConfirmWindowProperties);
		}

		public void ShowReconnectWindow()
		{
			this.HideConfirmWindow();
			Guid guid = Guid.NewGuid();
			this.currentConfirmWindowProperties = new ConfirmWindowProperties();
			this.currentConfirmWindowProperties.Guid = guid;
			this.currentConfirmWindowProperties.IsStackable = false;
			this.currentConfirmWindowProperties.EnableLoadGameObject = true;
			this.currentConfirmWindowProperties.QuestionText = Language.Get("RECONNECT_WAITING", TranslationSheets.MainMenuGui);
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(this.currentConfirmWindowProperties);
		}

		public void HideConfirmWindow()
		{
			if (GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible && this.currentConfirmWindowProperties != null)
			{
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(this.currentConfirmWindowProperties.Guid);
			}
		}

		public void ExitGame()
		{
			this.HideConfirmWindow();
			GameHubBehaviour.Hub.Quit();
		}

		public void NarratorGoBackToMain()
		{
			this.HideConfirmWindow();
			this.ReconnectMenu.BackToMain();
		}

		public void BackToGame()
		{
			this.ShowReconnectWindow();
			this.ReconnectMenu.BackToGame();
		}

		public void WarnMatchAlreadyEnded()
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("MatchFailedToConnectMatchEnded", TranslationSheets.MainMenuGui),
				OkButtonText = Language.Get("Ok", TranslationSheets.GUI),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					this.ReconnectMenu.BackToMain();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			this.HideConfirmWindow();
		}

		private const string tryingToReconnectTranslation = "RECONNECT_WAITING";

		private const string reconnectQuestionTranslation = "RECONNECT_QUESTION";

		private const string confirmReconnectTranslation = "RETRY_RECONNECT_BUTTON";

		private const string refuseReconnectTranslation = "CANCEL_RECONNECT_BUTTON";

		private const string narratorReconnectQuestionTranslation = "RECONNECT_QUESTION_SPECTATOR";

		private const string narratorConfirmReconnectTranslation = "CLOSEGAME_YES";

		private const string narratorRefuseReconnectTranslation = "CLOSEGAME_NO";

		private ConfirmWindowProperties currentConfirmWindowProperties;

		private ReconnectMenu _reconnectMenu;
	}
}
