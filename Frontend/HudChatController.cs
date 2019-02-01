using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudChatController : GameHubBehaviour
	{
		private GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = this._gameGui) == null)
				{
					result = (this._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		protected void Start()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				base.gameObject.SetActive(false);
				return;
			}
			if (this._isPickMode)
			{
				GameHubBehaviour.Hub.GuiScripts.Loading.OnHideLoading += this.OnHideLoading;
				this.OnChatStateChanged(HudChatController.ChatState.PickMode);
			}
			this._gameState = (GameHubBehaviour.Hub.State.Current as Game);
			if (this._gameState != null)
			{
				this._isInGame = true;
				this._gameState.FinishedLoading += this.GameStateOnFinishedLoading;
				this._gameState.OnGameOver += this.GameStateOnGameOver;
				this.OnChatStateChanged(HudChatController.ChatState.InGame);
			}
			this._spamFilter = new SpamFilter(SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamMessageCountThreshold, SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamBlockedChatDuration);
			GameHubBehaviour.Hub.Announcer.ListenToEvent += this.OnAnnouncerEvent;
			SingletonMonoBehaviour<VoiceChatController>.Instance.OnVoiceChatStatusChanged += this.OnVoiceChatStatusChanged;
			PauseController.OnInGamePauseStateChanged += this.OnPauseStateChanged;
			PauseController.OnNotification += this.OnPauseNotification;
			if (this.ChatMsgInput != null)
			{
				this.ChatMsgInput.selectAllTextOnFocus = false;
				EventDelegate item = new EventDelegate(new EventDelegate.Callback(this.OnChange_ChatMsgInput));
				this.ChatMsgInput.onChange.Add(item);
				return;
			}
			HudChatController.Log.Error("[ERROR] ChatMsgInput not set in " + base.GetType().Name);
		}

		private void OnChange_ChatMsgInput()
		{
			this._undoTextControl.PushNewText(this.ChatMsgInput.value);
		}

		protected void OnDestroy()
		{
			if (GameHubBehaviour.Hub == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.Loading.OnHideLoading -= this.OnHideLoading;
			}
			GameHubBehaviour.Hub.Announcer.ListenToEvent -= this.OnAnnouncerEvent;
			SingletonMonoBehaviour<VoiceChatController>.Instance.OnVoiceChatStatusChanged -= this.OnVoiceChatStatusChanged;
			PauseController.OnInGamePauseStateChanged -= this.OnPauseStateChanged;
			PauseController.OnNotification -= this.OnPauseNotification;
			if (this._gameState != null)
			{
				this._gameState.FinishedLoading -= this.GameStateOnFinishedLoading;
				this._gameState.OnGameOver -= this.GameStateOnGameOver;
			}
		}

		protected void OnEnable()
		{
			this._updater = new TimedUpdater(100, true, true);
			this._isPickMode = (GameHubBehaviour.Hub.State.Current as PickModeSetup);
			this.ChatLinesCounter = 0;
			HudChatController.ChatMsgInputVisible = false;
			this.ChatMsgInputGO.SetActive(false);
			this.ChatMsgCollider.enabled = false;
			this.Background.alpha = 0f;
			this.ChatMessageReferenceGameObject.SetActive(false);
			this.ChatMsgsHubGrid.sorting = UIGrid.Sorting.Custom;
			UIGrid chatMsgsHubGrid = this.ChatMsgsHubGrid;
			if (HudChatController.<>f__mg$cache0 == null)
			{
				HudChatController.<>f__mg$cache0 = new Comparison<Transform>(GUIUtils.InverseSortByName<Transform>);
			}
			chatMsgsHubGrid.onCustomSort = HudChatController.<>f__mg$cache0;
			this.LastChatMsgsHubGrid.sorting = UIGrid.Sorting.Custom;
			UIGrid lastChatMsgsHubGrid = this.LastChatMsgsHubGrid;
			if (HudChatController.<>f__mg$cache1 == null)
			{
				HudChatController.<>f__mg$cache1 = new Comparison<Transform>(GUIUtils.InverseSortByName<Transform>);
			}
			lastChatMsgsHubGrid.onCustomSort = HudChatController.<>f__mg$cache1;
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback += this.ConfigureSendButtonLabel;
			this.ConfigureSendButtonLabel(ControlAction.ChatSend);
			this.HideChatInput();
			if (GameHubBehaviour.Hub != null)
			{
				GameHubBehaviour.Hub.Chat.OnClientChatMessageReceived += this.AddNewChatMsgs;
			}
		}

		protected void OnDisable()
		{
			this.HideChatInput();
			if (GameHubBehaviour.Hub != null)
			{
				GameHubBehaviour.Hub.Chat.OnClientChatMessageReceived -= this.AddNewChatMsgs;
			}
			for (int i = 0; i < this._chatMsgList.Count; i++)
			{
				HudChatMsgController hudChatMsgController = this._chatMsgList[i];
				UnityEngine.Object.Destroy(hudChatMsgController.gameObject);
			}
			for (int j = 0; j < this._lastChatMsgList.Count; j++)
			{
				HudChatMsgController hudChatMsgController2 = this._lastChatMsgList[j];
				UnityEngine.Object.Destroy(hudChatMsgController2.gameObject);
			}
			for (int k = 0; k < this._chatLabelsCache.Count; k++)
			{
				HudChatMsgController hudChatMsgController3 = this._chatLabelsCache.Pop();
				UnityEngine.Object.Destroy(hudChatMsgController3.gameObject);
			}
			this._chatMsgList.Clear();
			this._lastChatMsgList.Clear();
			this._chatLabelsCache.Clear();
			HudChatController.ChatMsgInputVisible = false;
		}

		protected void GameStateOnFinishedLoading()
		{
			if (GameHubBehaviour.Hub.Match.State != MatchData.MatchState.PreMatch && GameHubBehaviour.Hub.Match.State != MatchData.MatchState.MatchStarted)
			{
				return;
			}
			this._showInGameVoiceChatMesage = true;
			this._inGameTimeStarted = Time.time;
			this._showInGameWelcomeChatMessage = true;
			this.GameGui.HudTabController.OnVisibilityChange += this.HudTabOnVisibilityChange;
		}

		protected void Update()
		{
			this.ChatVisibilityUpdate();
			if (HudChatController.ChatMsgInputVisible)
			{
				if (UndoTextControl.CheckUndoInput())
				{
					this.ChatMsgInput.Set(this._undoTextControl.Undo(), false);
				}
				if (UndoTextControl.CheckRedoInput())
				{
					this.ChatMsgInput.Set(this._undoTextControl.Redo(), false);
				}
			}
			this.UpdateAtFixedInterval();
		}

		private void UpdateAtFixedInterval()
		{
			if (this._updater.ShouldHalt())
			{
				return;
			}
			this.SendPickVoiceChatNotification();
			this.TrySendInGameNotifications();
			this.TryUpdateChatMessagesGrid();
		}

		private void TrySendInGameNotifications()
		{
			if (!this._isInGame)
			{
				return;
			}
			this.SendInGameVoiceChatNotification();
			this.SendInGameWelcomeChatNotification();
			this.SendChatClosedNotification();
			this.SendEndGameFeedbackMessage();
		}

		private void TryUpdateChatMessagesGrid()
		{
			if (HudChatController.ChatMsgInputVisible)
			{
				return;
			}
			if (!this.LastChatMsgsHubGO.activeSelf)
			{
				return;
			}
			if (!this._isMessagesFadeEnabled)
			{
				return;
			}
			for (int i = 0; i < this._lastChatMsgList.Count; i++)
			{
				HudChatMsgController hudChatMsgController = this._lastChatMsgList[i];
				float num = Mathf.Max(hudChatMsgController.Time - Time.time, 0f);
				if (num <= 0f && hudChatMsgController.gameObject.activeSelf)
				{
					hudChatMsgController.gameObject.SetActive(false);
				}
				else if (num < 1f && num > 0f)
				{
					hudChatMsgController.UiLabel.alpha = num;
				}
			}
			this.LastChatMsgsHubGrid.repositionNow = true;
			this.LastChatMsgsHubGrid.Reposition();
		}

		private void OnAnnouncerEvent(AnnouncerManager.QueuedAnnouncerLog announcerLog)
		{
			AnnouncerLog.AnnouncerEventKinds announcerEventKind = announcerLog.AnnouncerLog.AnnouncerEventKind;
			if (announcerEventKind == AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected)
			{
				if (GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchStarted)
				{
					this.AddNewChatMsgs(new HudChatController.NewChatMsg
					{
						text = Language.Get("HUDCHAT_PAUSE_HINT", TranslationSheets.HUDChat)
					}, false);
				}
			}
		}

		private void OnVoiceChatStatusChanged(VoiceChatTeamStatus currentStatus)
		{
			this.ShowVoiceChatMessageWithoutDelay();
		}

		private void ShowVoiceChatMessageWithoutDelay()
		{
			if (this._isPickMode)
			{
				this._showPickVoiceChatMesage = true;
				this._pickTimeStarted = 0f;
			}
			if (this._isInGame)
			{
				this._showInGameVoiceChatMesage = true;
				this._inGameTimeStarted = 0f;
			}
		}

		private void OnHideLoading()
		{
			this._showPickVoiceChatMesage = true;
			this._pickTimeStarted = Time.time;
		}

		private void SendPickVoiceChatNotification()
		{
			if (!this._showPickVoiceChatMesage || this._pickTimeStarted + this.DelayToShowPickVoiceChatMesage > Time.time)
			{
				return;
			}
			this._showPickVoiceChatMesage = false;
			if (!HudChatController.IsVoiceChatMessageEnabled())
			{
				return;
			}
			string textlocalized;
			if (ControlOptions.IsUsingControllerJoystick(GameHubBehaviour.Hub))
			{
				textlocalized = ControlOptions.GetTextlocalized(ControlAction.PushToTalk, ControlOptions.ControlActionInputType.Secondary);
			}
			else
			{
				textlocalized = ControlOptions.GetTextlocalized(ControlAction.PushToTalk, ControlOptions.ControlActionInputType.Primary);
			}
			FMODAudioManager.PlayOneShotAt(this.sfx_ui_pick_voicechat_notification, base.transform.position, 0);
			HudChatController.NewChatMsg newChatMsg = new HudChatController.NewChatMsg();
			string arg = string.Empty;
			string key;
			if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus == VoiceChatTeamStatus.Disable)
			{
				key = "PICK_VOICE_CHAT_DISABLED_NOTIFICATION";
			}
			else if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType == VoiceChatInputType.AlwaysActive)
			{
				key = "PICK_VOICE_CHAT_ALWAYS_ON_NOTIFICATION";
			}
			else if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType == VoiceChatInputType.Toggle)
			{
				key = "PICK_VOICE_CHAT_TOGGLE_NOTIFICATION";
			}
			else
			{
				key = "PICK_VOICE_CHAT_NOTIFICATION";
				arg = string.Format(Language.Get("VOICE_CHAT_NOTIFICATION_KEY", TranslationSheets.HUDChat), textlocalized);
			}
			string text = Language.Get(key, TranslationSheets.HUDChat);
			text = string.Format("{0} {1}", text, arg);
			newChatMsg.text = string.Format(text, textlocalized);
			this.AddNewChatMsgs(newChatMsg, true);
		}

		private static bool IsVoiceChatMessageEnabled()
		{
			int num = 0;
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Players[i];
				if (playerData.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
				{
					num++;
				}
			}
			return num > 1;
		}

		private void SendInGameWelcomeChatNotification()
		{
			if (!this._showInGameWelcomeChatMessage || this._inGameTimeStarted + this.DelayToShowWelcomeChatMessage > Time.time)
			{
				return;
			}
			this._showInGameWelcomeChatMessage = false;
			this.AddNewChatMsgs(new HudChatController.NewChatMsg
			{
				text = Language.Get(this.DraftWelcomeMessage, TranslationSheets.Chat)
			}, false);
		}

		private void SendInGameVoiceChatNotification()
		{
			if (!this._showInGameVoiceChatMesage || this._inGameTimeStarted + this.DelayToShowInGameVoiceChatMesage > Time.time)
			{
				return;
			}
			this._showInGameVoiceChatMesage = false;
			if (!HudChatController.IsVoiceChatMessageEnabled())
			{
				return;
			}
			string textlocalized;
			if (ControlOptions.IsUsingControllerJoystick(GameHubBehaviour.Hub))
			{
				textlocalized = ControlOptions.GetTextlocalized(ControlAction.PushToTalk, ControlOptions.ControlActionInputType.Secondary);
			}
			else
			{
				textlocalized = ControlOptions.GetTextlocalized(ControlAction.PushToTalk, ControlOptions.ControlActionInputType.Primary);
			}
			FMODAudioManager.PlayOneShotAt(this.sfx_ui_game_voicechat_notification, base.transform.position, 0);
			HudChatController.NewChatMsg newChatMsg = new HudChatController.NewChatMsg();
			string arg = string.Empty;
			string key;
			if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus == VoiceChatTeamStatus.Disable)
			{
				key = "GAME_VOICE_CHAT_DISABLED_NOTIFICATION";
			}
			else if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType == VoiceChatInputType.AlwaysActive)
			{
				key = "GAME_VOICE_CHAT_ALWAYS_ON_NOTIFICATION";
			}
			else if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType == VoiceChatInputType.Toggle)
			{
				key = "GAME_VOICE_CHAT_TOGGLE_NOTIFICATION";
			}
			else
			{
				key = "GAME_VOICE_CHAT_NOTIFICATION";
				arg = string.Format(Language.Get("VOICE_CHAT_NOTIFICATION_KEY", TranslationSheets.HUDChat), textlocalized);
			}
			string text = Language.Get(key, TranslationSheets.HUDChat);
			text = string.Format("{0} {1}", text, arg);
			newChatMsg.text = string.Format(text, textlocalized);
			this.AddNewChatMsgs(newChatMsg, true);
		}

		public void OnPauseNotification(PauseController.PauseNotification notification)
		{
			HudChatController.NewChatMsg newChatMsg = new HudChatController.NewChatMsg();
			string text = string.Empty;
			switch (notification.kind)
			{
			case PauseController.PauseNotificationKind.None:
				return;
			case PauseController.PauseNotificationKind.InvalidGameState:
				if (GameHubBehaviour.Hub.BombManager.IsGameFinished())
				{
					return;
				}
				switch (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState)
				{
				case BombScoreBoard.State.PreBomb:
				case BombScoreBoard.State.Shop:
					text = string.Format(Language.Get(this.DraftCannotPauseInStatePreBomb, TranslationSheets.Chat), GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState);
					goto IL_10F;
				case BombScoreBoard.State.PreReplay:
				case BombScoreBoard.State.Replay:
					text = string.Format(Language.Get(this.DraftCannotPauseInStateReplay, TranslationSheets.Chat), GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState);
					goto IL_10F;
				}
				return;
				IL_10F:
				break;
			case PauseController.PauseNotificationKind.UnPauseDelay:
				text = string.Format(Language.Get(this.DraftUnpauseDelay, TranslationSheets.Chat), Mathf.RoundToInt(notification.delay));
				break;
			case PauseController.PauseNotificationKind.InputBlocked:
				text = Language.Get(this.DraftCannotUseInput, TranslationSheets.Chat);
				break;
			case PauseController.PauseNotificationKind.PauseCountdown:
				text = Language.Get(this.DraftPauseCountdownBlock, TranslationSheets.Chat);
				break;
			case PauseController.PauseNotificationKind.UnpauseCountdown:
				text = Language.Get(this.DraftUnpauseCountdownBlock, TranslationSheets.Chat);
				break;
			case PauseController.PauseNotificationKind.PlayerCooldown:
				text = string.Format(Language.Get(this.DraftPlayerPauseCooldown, TranslationSheets.Chat), Mathf.RoundToInt(notification.delay));
				break;
			}
			newChatMsg.text = text;
			this.AddNewChatMsgs(newChatMsg, false);
		}

		private void OnPauseStateChanged(PauseController.PauseState oldState, PauseController.PauseState newState, PlayerData playerData)
		{
			HudChatController.NewChatMsg newChatMsg = new HudChatController.NewChatMsg();
			string text;
			if (newState != PauseController.PauseState.Paused)
			{
				if (newState != PauseController.PauseState.PauseCountDown)
				{
					return;
				}
				if (playerData == null)
				{
					return;
				}
				if (GameHubBehaviour.Hub.Players.CurrentPlayerData == playerData)
				{
					text = string.Format(Language.Get(this.DraftCountdownStartMe, TranslationSheets.Chat), Mathf.Max(PauseController.Instance.PauseSettingsData.PauseCountDownTime - 1f, 0f));
				}
				else if (playerData.IsNarrator)
				{
					text = string.Format(Language.Get(this.DraftCountdownStartNarrator, TranslationSheets.Chat), playerData.Name, Mathf.Max(PauseController.Instance.PauseSettingsData.PauseCountDownTime - 1f, 0f));
				}
				else
				{
					text = string.Format(Language.Get(this.DraftCountdownStartOther, TranslationSheets.Chat), playerData.Name, Mathf.Max(PauseController.Instance.PauseSettingsData.PauseCountDownTime - 1f, 0f));
				}
			}
			else
			{
				string textlocalized = ControlOptions.GetTextlocalized(ControlAction.Pause, ControlOptions.ControlActionInputType.Primary);
				text = string.Format(Language.Get(this.DraftPausedNotification, TranslationSheets.Chat), textlocalized);
			}
			newChatMsg.text = text;
			this.AddNewChatMsgs(newChatMsg, false);
		}

		[Obsolete]
		public void OnPauseCountDown(int time)
		{
			HudChatController.NewChatMsg newChatMsg = new HudChatController.NewChatMsg();
			string text = string.Empty;
			PauseController.PauseState currentState = PauseController.Instance.CurrentState;
			if (currentState != PauseController.PauseState.PauseCountDown)
			{
				if (currentState == PauseController.PauseState.UnpauseCountDown)
				{
					text = string.Format(Language.Get(this.DraftUnpauseCountdown, TranslationSheets.Chat), time);
				}
			}
			else
			{
				text = string.Format(Language.Get(this.DraftPauseCountdown, TranslationSheets.Chat), time);
			}
			newChatMsg.text = text;
			this.AddNewChatMsgs(newChatMsg, false);
		}

		private void GameStateOnGameOver(MatchData.MatchState matchwinner)
		{
			this._gameOverTime = Time.time;
		}

		private void SendChatClosedNotification()
		{
			if (this._serverClosedNotificationTimer < 0f || this._serverClosedNotificationTimer >= Time.time)
			{
				return;
			}
			this.SendEndGameCloseServerNotificationTimerFinished();
			this._serverClosedNotificationTimer = -1f;
		}

		public void OnProgressionControllerVisibilityChange()
		{
			this.OnChatStateChanged(HudChatController.ChatState.EndGame);
			this.SendEndGameCloseServerNotificationTimerStarted();
			this._delayFeedbackMessageTimer = Time.time + this.DelayToShowFeedbackChatMessage;
			base.transform.localPosition += this.EndGamePositionChange;
		}

		private void SendEndGameFeedbackMessage()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			if (this._delayFeedbackMessageTimer < 0f)
			{
				return;
			}
			if (Time.time < this._delayFeedbackMessageTimer)
			{
				return;
			}
			this._delayFeedbackMessageTimer = -1f;
			this.AddNewChatMsgs(new HudChatController.NewChatMsg
			{
				text = Language.Get(this.DraftFeedbackMessage, TranslationSheets.Chat)
			}, false);
		}

		private void SendEndGameCloseServerNotificationTimerStarted()
		{
			float num = Time.time - this._gameOverTime;
			num = Mathf.Max((float)GameHubBehaviour.Hub.SharedConfigs.TimeOutSecondsToCloseServer - num, 0f);
			this._serverClosedNotificationTimer = Time.time + num;
			HudChatController.NewChatMsg newChatMsg = new HudChatController.NewChatMsg();
			string text = Language.Get("ENDGAME_CLOSER_SERVER_NOTIFICATION_START", TranslationSheets.Chat);
			text = string.Format(text, (int)num);
			newChatMsg.text = text;
			this.AddNewChatMsgs(newChatMsg, false);
		}

		private void SendEndGameCloseServerNotificationTimerFinished()
		{
			this.AddNewChatMsgs(new HudChatController.NewChatMsg
			{
				text = Language.Get("ENDGAME_CLOSER_SERVER_NOTIFICATION_END", TranslationSheets.Chat)
			}, false);
		}

		private void ChatVisibilityUpdate()
		{
			if (GameHubBehaviour.Hub.GuiScripts.Esc.IsWindowVisible())
			{
				return;
			}
			if (HudChatController.ChatMsgInputVisible)
			{
				if (ControlOptions.GetButtonUp(ControlAction.ChatSend))
				{
					this.SendChatMessage();
					this.HideChatInput();
				}
				return;
			}
			if (HudChatController.ShouldOpenHudChat() && HudChatController.HasOpenChatInput())
			{
				this.OpenChatAndInput();
			}
		}

		private static bool ShouldOpenHudChat()
		{
			return !SocialModalGUI.IsWindowOpened;
		}

		private static bool HasOpenChatInput()
		{
			return ControlOptions.GetButtonUp(ControlAction.ChatAll) || ControlOptions.GetButtonUp(ControlAction.ChatTeam);
		}

		private void ConfigureSendButtonLabel(ControlAction controlAction)
		{
			this.ChatSendMessageButtonLabel.text = ControlOptions.GetTextlocalized(ControlAction.ChatSend, ControlOptions.ControlActionInputType.Primary);
		}

		public void AllowHacksAddNewChatMsgs(HudChatController.NewChatMsg newChatMsg)
		{
		}

		private void AddNewChatMsgs(HudChatController.NewChatMsg newChatMsg)
		{
			this.AddNewChatMsgs(newChatMsg, false);
		}

		private void AddNewChatMsgs(HudChatController.NewChatMsg newChatMsg, bool showIcon)
		{
			if (string.IsNullOrEmpty(newChatMsg.text))
			{
				HudChatController.Log.WarnFormat("tried to add empty message to chat. why? Stack:{0}", new object[]
				{
					StackTraceUtility.ExtractStackTrace()
				});
				return;
			}
			if (this._chatMsgList == null)
			{
				this._chatMsgList = new List<HudChatMsgController>();
			}
			if (this._lastChatMsgList == null)
			{
				this._lastChatMsgList = new List<HudChatMsgController>();
			}
			string text = newChatMsg.text.Trim();
			bool flag = false;
			string text2 = string.Empty;
			bool flag2;
			do
			{
				text = text.Substring(text2.Length).TrimStart(new char[0]);
				if (flag)
				{
					text = HudChatController.NguiTextGetSymbols(text2) + text;
				}
				HudChatMsgController hudChatMsgController = this.SetupChatLabel(this.ChatMsgsHubGrid.transform);
				hudChatMsgController.UiLabel.text = text;
				hudChatMsgController.UiLabel.TryUpdateText();
				hudChatMsgController.Sprite.gameObject.SetActive(showIcon && !flag);
				hudChatMsgController.Sprite.UpdateAnchors();
				this._chatMsgList.Add(hudChatMsgController);
				HudChatMsgController hudChatMsgController2 = this.SetupChatLabel(this.LastChatMsgsHubGrid.transform);
				hudChatMsgController2.UiLabel.text = text;
				hudChatMsgController2.UiLabel.TryUpdateText();
				hudChatMsgController2.Sprite.gameObject.SetActive(showIcon && !flag);
				hudChatMsgController.Sprite.UpdateAnchors();
				this._lastChatMsgList.Add(hudChatMsgController2);
				if (this._lastChatMsgList.Count > this.LastChatMsgMaxLineCount)
				{
					HudChatMsgController labelGameObject = this._lastChatMsgList[0];
					this.SendLabelToCache(labelGameObject);
					this._lastChatMsgList.RemoveAt(0);
				}
				flag = true;
				this.ChatLinesCounter++;
				text2 = hudChatMsgController.UiLabel.processedText;
				flag2 = (text2 != text);
				int num = text2.LastIndexOf(' ');
				if (flag2 && num != -1)
				{
					text2 = text2.Remove(num);
					hudChatMsgController.UiLabel.text = text2;
					hudChatMsgController.UiLabel.TryUpdateText();
					hudChatMsgController2.UiLabel.text = text2;
					hudChatMsgController2.UiLabel.TryUpdateText();
				}
			}
			while (flag2);
			if (this.ChatMsgsHubGrid.gameObject.activeInHierarchy)
			{
				this.RepositionFullChatScroll();
			}
			else
			{
				this.RepositionLastMsgChat();
			}
			this.ChatMsgsScroll.value = 1f;
		}

		private HudChatMsgController SetupChatLabel(Transform hubTransform)
		{
			HudChatMsgController chatLabel = this.GetChatLabel();
			chatLabel.gameObject.name = "chatmsg_" + this.ChatLinesCounter.ToString("000000");
			chatLabel.gameObject.transform.parent = hubTransform;
			chatLabel.gameObject.transform.localScale = Vector3.one;
			chatLabel.gameObject.transform.localPosition = Vector3.zero;
			chatLabel.gameObject.SetActive(true);
			chatLabel.Time = Time.time + (float)this._maxVisibleTimeMillis;
			return chatLabel;
		}

		public void OnChatStateChanged(HudChatController.ChatState newState)
		{
			if (newState != HudChatController.ChatState.PickMode)
			{
				if (newState != HudChatController.ChatState.InGame)
				{
					if (newState == HudChatController.ChatState.EndGame)
					{
						this._isMessagesFadeEnabled = this.EnableMessagesFadeForEndGame;
						this._maxVisibleTimeMillis = this.EndGameMaxVisibleTimeMillis;
						this.SetTextFieldBorderVisible();
					}
				}
				else
				{
					this._isMessagesFadeEnabled = this.EnableMessagesFadeForGame;
					this._maxVisibleTimeMillis = this.InGameMaxVisibleTimeMillis;
				}
			}
			else
			{
				this._isMessagesFadeEnabled = this.EnableMessagesFadeForPick;
				this._maxVisibleTimeMillis = this.PickMaxVisibleTimeMillis;
				this.SetTextFieldBorderVisible();
			}
			if (!this._isMessagesFadeEnabled)
			{
				for (int i = 0; i < this._lastChatMsgList.Count; i++)
				{
					HudChatMsgController hudChatMsgController = this._lastChatMsgList[i];
					hudChatMsgController.gameObject.SetActive(true);
					hudChatMsgController.Time = Time.time + (float)this._maxVisibleTimeMillis;
					hudChatMsgController.UiLabel.alpha = 1f;
				}
			}
			this._currentState = newState;
		}

		private HudChatMsgController GetChatLabel()
		{
			if (this._chatLabelsCache.Count > 0)
			{
				HudChatMsgController hudChatMsgController = this._chatLabelsCache.Pop();
				hudChatMsgController.gameObject.SetActive(true);
				hudChatMsgController.UiLabel.alpha = 1f;
				return hudChatMsgController;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.ChatMessageReferenceGameObject, Vector3.zero, Quaternion.identity);
			return gameObject.GetComponent<HudChatMsgController>();
		}

		private void SendLabelToCache(HudChatMsgController labelGameObject)
		{
			labelGameObject.gameObject.SetActive(false);
			this._chatLabelsCache.Push(labelGameObject);
		}

		private void OpenChatAndInput()
		{
			this._isGroup = !ControlOptions.GetButtonUp(ControlAction.ChatAll);
			if (SpectatorController.IsSpectating)
			{
				this._isGroup = false;
			}
			this.SetChatInputVisible(true);
			this.SetChatVisible(true);
		}

		private void SetChatInputVisible(bool visible)
		{
			HudChatController.ChatMsgInputVisible = visible;
			this.ChatMsgInputGO.SetActive(true);
			this.ChatMsgCollider.enabled = visible;
			this.ChatMsgInput.isSelected = visible;
			this.ChatMsgInputGO.SetActive(visible);
			this.ChatMsgInputTitle.text = ((!visible) ? string.Empty : this.GetChatMsgInputTitleText());
			this.ChatMsgInputTitle.TryUpdateText();
			this.ChatMsgInput.value = ((!visible) ? Language.Get("CHAT_MESSAGE_PICKMODE", TranslationSheets.Chat) : string.Empty);
			this.ChatMsgInput.label.alpha = ((!visible) ? this.InsertMessageAlpha : 1f);
			this.ChatMsgInput.label.UpdateAnchors();
			if (!visible)
			{
				this._undoTextControl.Clear();
			}
		}

		private void SetChatVisible(bool visibile)
		{
			this.ChatMsgsHubGO.SetActive(visibile);
			this.ChatMsgsScroll.gameObject.SetActive(visibile);
			this.LastChatMsgsHubGO.SetActive(!visibile);
			this.Background.alpha = ((!visibile) ? 0f : this.BackgroundAlpha);
			if (visibile)
			{
				this.RepositionFullChatScroll();
			}
			else
			{
				this.RepositionLastMsgChat();
			}
			if (GameHubBehaviour.Hub == null || GameHubBehaviour.Hub.Players.CurrentPlayerData == null || GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance == null)
			{
				if (this._lastPlayerController != null)
				{
					this._lastPlayerController.HudChatOpen = visibile;
				}
				return;
			}
			PlayerController bitComponent = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<PlayerController>();
			bitComponent.HudChatOpen = visibile;
		}

		private void RepositionFullChatScroll()
		{
			this.ChatMsgsHubGrid.repositionNow = true;
			this.ChatMsgsHubGrid.Reposition();
			this.ChatMsgsScroll.value = 1f;
		}

		private void RepositionLastMsgChat()
		{
			this.LastChatMsgsHubGrid.repositionNow = true;
			this.LastChatMsgsHubGrid.Reposition();
		}

		private string GetChatMsgInputTitleText()
		{
			return string.Format("[{0}] ", Language.Get((!this._isGroup) ? "CHAT_SCOPE_ALL" : "CHAT_SCOPE_TEAM", TranslationSheets.Chat));
		}

		private void HideChatInput()
		{
			this.SetChatInputVisible(false);
			this.SetChatVisible(false);
			if (this._currentState == HudChatController.ChatState.PickMode || this._currentState == HudChatController.ChatState.EndGame)
			{
				this.SetTextFieldBorderVisible();
			}
			this.TryToPopCursor();
		}

		private void SetTextFieldBorderVisible()
		{
			this.ChatMsgInputGO.SetActive(true);
		}

		private void SendChatMessage()
		{
			this.ChatMsgInput.value = NGUIText.StripSymbols(this.ChatMsgInput.value);
			string value = this.ChatMsgInput.value;
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			if (this._spamFilter.IsSpam(value, Time.unscaledTime))
			{
				this.ThrowSystemSpamAlert();
				return;
			}
			GameHubBehaviour.Hub.Chat.ClientSendMessage(this._isGroup, value);
		}

		private void ThrowSystemSpamAlert()
		{
			this.AddNewChatMsgs(new HudChatController.NewChatMsg
			{
				text = this._spamFilter.GetSpamBlockMessage(Time.unscaledTime)
			});
		}

		public static string NguiTextGetSymbols(string text)
		{
			string text2 = string.Empty;
			if (text == null)
			{
				return text2;
			}
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '[')
				{
					int num = 0;
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					bool flag4 = false;
					bool flag5 = false;
					int num2 = i;
					if (NGUIText.ParseSymbol(text, ref num2, null, false, ref num, ref flag, ref flag2, ref flag3, ref flag4, ref flag5))
					{
						text2 += text.Substring(i, num2 - i);
						i = num2;
						continue;
					}
				}
			}
			return text2;
		}

		private void HudTabOnVisibilityChange(bool visible)
		{
			base.GetComponent<UIPanel>().alpha = (visible ? 0.2f : 1f);
		}

		public void OnClickInputArea()
		{
			if (HudChatController.ChatMsgInputVisible)
			{
				return;
			}
			if (HudChatController.ShouldOpenHudChat())
			{
				this.OpenChatAndInput();
			}
		}

		public void OnHoverOverTextArea()
		{
			if (!this._cursorPush)
			{
				this._cursorPush = true;
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
			}
		}

		public void OnHoverOutTextArea()
		{
			this.TryToPopCursor();
		}

		private void TryToPopCursor()
		{
			if (this._cursorPush)
			{
				this._cursorPush = false;
				GameHubBehaviour.Hub.CursorManager.Pop();
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudChatController));

		private UndoTextControl _undoTextControl = new UndoTextControl();

		public GameObject ChatMsgInputGO;

		public UIInput ChatMsgInput;

		public Collider ChatMsgCollider;

		public UILabel ChatMsgInputTitle;

		public static bool ChatMsgInputVisible;

		public UILabel ChatSendMessageButtonLabel;

		public bool EnableMessagesFadeForPick;

		public int PickMaxVisibleTimeMillis;

		public bool EnableMessagesFadeForGame;

		public int InGameMaxVisibleTimeMillis;

		public bool EnableMessagesFadeForEndGame;

		public int EndGameMaxVisibleTimeMillis;

		public Vector3 EndGamePositionChange;

		public int MaxLineCharacters;

		public int ChatLinesCounter;

		public float InsertMessageAlpha = 0.2f;

		public UI2DSprite Background;

		public float BackgroundAlpha = 0.9f;

		private List<HudChatMsgController> _chatMsgList = new List<HudChatMsgController>();

		private List<HudChatMsgController> _lastChatMsgList = new List<HudChatMsgController>();

		private readonly Stack<HudChatMsgController> _chatLabelsCache = new Stack<HudChatMsgController>();

		private TimedUpdater _updater;

		public UIGrid ChatMsgsHubGrid;

		public GameObject ChatMsgsHubGO;

		public UIScrollBar ChatMsgsScroll;

		public UIGrid LastChatMsgsHubGrid;

		public GameObject LastChatMsgsHubGO;

		public GameObject ChatMessageReferenceGameObject;

		private bool _isGroup;

		public int LastChatMsgMaxLineCount = 5;

		private bool _showInGameVoiceChatMesage;

		private bool _showPickVoiceChatMesage;

		public float DelayToShowPickVoiceChatMesage = 1.5f;

		private float _pickTimeStarted;

		private bool _showInGameWelcomeChatMessage;

		public float DelayToShowWelcomeChatMessage = 2f;

		public float DelayToShowFeedbackChatMessage = 2f;

		private float _delayFeedbackMessageTimer = -1f;

		public float DelayToShowInGameVoiceChatMesage = 5f;

		private float _inGameTimeStarted;

		public FMODAsset sfx_ui_pick_voicechat_notification;

		public FMODAsset sfx_ui_game_voicechat_notification;

		private bool _isPickMode;

		private bool _isInGame;

		private float _gameOverTime = -1f;

		private float _serverClosedNotificationTimer = -1f;

		private bool _isMessagesFadeEnabled;

		private int _maxVisibleTimeMillis;

		private HudChatController.ChatState _currentState;

		private GameGui _gameGui;

		private bool _cursorPush;

		private Game _gameState;

		private SpamFilter _spamFilter;

		public string DraftUnpauseDelay = "PAUSE_GAME_UNPAUSE_DELAY";

		public string DraftCannotPauseInStatePreBomb = "PAUSE_GAME_WRONG_STATE_PREBOMB";

		public string DraftCannotPauseInStateReplay = "PAUSE_GAME_WRONG_STATE_REPLAY";

		public string DraftCannotUseInput = "PAUSE_GAME_INPUT_USED";

		public string DraftPlayerPauseCooldown = "PAUSE_GAME_PLAYER_COOLDOWN";

		public string DraftPauseCountdownBlock = "PAUSE_GAME_COUNTDOWN_BLOCK";

		public string DraftUnpauseCountdownBlock = "PAUSE_GAME_UNPAUSE_COUNTDOWN_BLOCK";

		public string DraftPausedNotification = "PAUSE_GAME_PAUSED_NOTIFICATION";

		public string DraftCountdownStartMe = "PAUSE_GAME_COUNTDOWN_START_ME";

		public string DraftCountdownStartOther = "PAUSE_GAME_COUNTDOWN_START_OTHER";

		public string DraftCountdownStartNarrator = "PAUSE_GAME_COUNTDOWN_START_NARRATOR";

		public string DraftPauseCountdown = "PAUSE_GAME_COUNTDOWN";

		public string DraftUnpauseCountdown = "PAUSE_GAME_UNPAUSE_COUNTDOWN";

		public string DraftWelcomeMessage = "ENDGAME_MESSAGE_WELCOME_GPI";

		public string DraftFeedbackMessage = "ENDGAME_MESSAGE_FEEDBACK_GPI";

		private PlayerController _lastPlayerController;

		[CompilerGenerated]
		private static Comparison<Transform> <>f__mg$cache0;

		[CompilerGenerated]
		private static Comparison<Transform> <>f__mg$cache1;

		public enum ChatState
		{
			None,
			PickMode,
			InGame,
			EndGame
		}

		public class NewChatMsg
		{
			public string text;
		}
	}
}
