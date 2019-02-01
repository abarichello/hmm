using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Announcer
{
	public class AnnouncerManager : GameHubBehaviour, ICleanupListener
	{
		private void OnDestroy()
		{
			this.ListenToEvent = null;
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.ListenToBombDelivery;
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.ListenToBombDelivery;
		}

		private void ListenToBombDelivery(int causerid, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.BombDelivery)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(causerid);
			ScrapInfo[] scrapPerBombDelivery = GameHubBehaviour.Hub.ScrapLevel.ScrapPerBombDelivery;
			int mainReward = 0;
			if (scrapPerBombDelivery.Length > 0)
			{
				mainReward = scrapPerBombDelivery[Mathf.Min(GameHubBehaviour.Hub.BombManager.ScoreBoard.Round, scrapPerBombDelivery.Length - 1)].Value;
			}
			AnnouncerEvent announcerEvent = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.BombDelivery,
				KillerTeam = combat.Team,
				Killer = combat.Id.ObjId,
				MainReward = mainReward
			};
			this.TriggerAnnounce(new AnnouncerManager.QueuedAnnouncerLog(announcerEvent, this.Announcerinfo.GetAnnouncerLog(announcerEvent.AnnouncerEventKind)));
		}

		public bool IsHudBusy
		{
			get
			{
				return this._isHudBusy;
			}
			set
			{
				if (this._isHudBusy && value)
				{
					AnnouncerManager.Log.Warn("isHudBusy from AnnounceManager is being setted twice as true. This is an invalid behaviour.");
				}
				this._isHudBusy = value;
			}
		}

		public void Trigger(AnnouncerEvent announce, int eventId)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			string arg = string.Empty;
			if (announce.AssistPlayers != null)
			{
				for (int i = 0; i < announce.AssistPlayers.Count; i++)
				{
					int num = announce.AssistPlayers[i];
					arg = arg + num + ",";
				}
			}
			if (this.CheckAnnounceIgnore(announce))
			{
				return;
			}
			this.EnqueueAnnounce(announce);
		}

		private bool CheckAnnounceIgnore(AnnouncerEvent announce)
		{
			if (announce.AnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.None)
			{
				AnnouncerManager.Log.ErrorFormat("Log triggered with kind set to none.", new object[0]);
				return true;
			}
			AnnouncerLog announcerLog;
			if (!this.Announcerinfo.TryGetAnnouncerLog(announce.AnnouncerEventKind, out announcerLog))
			{
				AnnouncerManager.Log.WarnFormat("Log kind not found on Announcer info. Kind={0}", new object[]
				{
					announce.AnnouncerEventKind
				});
				return true;
			}
			if (announcerLog.TopAnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.None && announcerLog.LogAnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.None)
			{
				AnnouncerManager.Log.ErrorFormat("Log triggered with both logs set to none.", new object[0]);
				return true;
			}
			AnnouncerLog.AnnouncerEventKinds announcerEventKind = announce.AnnouncerEventKind;
			if (announcerEventKind == AnnouncerLog.AnnouncerEventKinds.PlayerReconnected)
			{
				if (!CombatObject.GetCombatObject(announce.Killer))
				{
					AnnouncerManager.Log.ErrorFormat("PlayerReconnected announcer event, but combat object is null. Id={0}", new object[]
					{
						announce.Killer
					});
					return true;
				}
			}
			return false;
		}

		private void EnqueueAnnounce(AnnouncerEvent announceevent)
		{
			switch (announceevent.AnnouncerEventKind)
			{
			case AnnouncerLog.AnnouncerEventKinds.KillStreak03:
			case AnnouncerLog.AnnouncerEventKinds.KillStreak06:
			case AnnouncerLog.AnnouncerEventKinds.KillStreak09:
			case AnnouncerLog.AnnouncerEventKinds.KillStreak12:
			case AnnouncerLog.AnnouncerEventKinds.KillStreak15:
				if (announceevent.Killer != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
				{
					return;
				}
				break;
			}
			this.TriggerAnnounce(new AnnouncerManager.QueuedAnnouncerLog(announceevent, this.Announcerinfo.GetAnnouncerLog(announceevent.AnnouncerEventKind)));
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<AnnouncerManager.QueuedAnnouncerLog> ListenToEvent;

		private void TriggerAnnounce(AnnouncerManager.QueuedAnnouncerLog enqueuedAnnounce)
		{
			this._lastTopLogTime = (long)GameHubBehaviour.Hub.GameTime.GetSynchTime();
			AnnouncerLog announcerLog = this.Announcerinfo.GetAnnouncerLog(enqueuedAnnounce.AnnouncerEvent.AnnouncerEventKind);
			if (announcerLog.Audio != null && announcerLog.clientOnlyAudio && !SpectatorController.IsSpectating && GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId == enqueuedAnnounce.AnnouncerEvent.Killer)
			{
				FMODAudioManager.PlayOneShotAt(announcerLog.Audio, Vector3.zero, 0);
			}
			AnnouncerLog.AnnouncerEventKinds logAnnouncerEventKind = announcerLog.LogAnnouncerEventKind;
			switch (logAnnouncerEventKind)
			{
			case AnnouncerLog.AnnouncerEventKinds.LeaverGeneric:
			case AnnouncerLog.AnnouncerEventKinds.AFKEnd:
			case AnnouncerLog.AnnouncerEventKinds.LeaverModifierWarning:
				break;
			default:
				if (logAnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerReconnected && logAnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected && logAnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.AFKGeneric)
				{
					goto IL_DC;
				}
				break;
			}
			this.TriggerLogText(enqueuedAnnounce);
			IL_DC:
			if (this.ListenToEvent != null)
			{
				this.ListenToEvent(enqueuedAnnounce);
			}
		}

		private void TriggerLogText(AnnouncerManager.QueuedAnnouncerLog enqueuedAnnounce)
		{
			AnnouncerEvent announcerEvent = enqueuedAnnounce.AnnouncerEvent;
			AnnouncerLog announcerLog = enqueuedAnnounce.AnnouncerLog;
			string text = announcerLog.LocalizedText;
			AnnouncerLog.AnnouncerEventKinds announcerEventKind = announcerEvent.AnnouncerEventKind;
			string text2 = string.Empty;
			if (enqueuedAnnounce.AnnouncerEvent.AssistPlayers != null && enqueuedAnnounce.AnnouncerEvent.AssistPlayers.Count > 0)
			{
				text2 += this.GetColoredPlayerName(announcerEvent.AssistPlayers[0]);
				for (int i = 1; i < announcerEvent.AssistPlayers.Count; i++)
				{
					text2 = text2 + ", " + this.GetColoredPlayerName(announcerEvent.AssistPlayers[i]);
				}
			}
			switch (announcerEventKind)
			{
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayer:
			case AnnouncerLog.AnnouncerEventKinds.PlayerEndedAKillStreak:
			case AnnouncerLog.AnnouncerEventKinds.PlayerEndedAKillStreakWithAssists:
				text = string.Format(text, this.GetColoredPlayerName(announcerEvent.Killer), this.GetColoredPlayerName(announcerEvent.Victim), announcerEvent.MainReward);
				goto IL_235;
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayerWithAssists:
				text = string.Format(text, new object[]
				{
					this.GetColoredPlayerName(announcerEvent.Killer),
					this.GetColoredPlayerName(announcerEvent.Victim),
					announcerEvent.MainReward,
					text2
				});
				goto IL_235;
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironment:
				text = string.Format(text, this.GetColoredPlayerName(announcerEvent.Victim), this.GetColoredTeamName(announcerEvent.KillerTeam), announcerEvent.MainReward);
				goto IL_235;
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironmentWithAssists:
				text = string.Format(text, new object[]
				{
					this.GetColoredPlayerName(announcerEvent.Victim),
					this.GetColoredTeamName(announcerEvent.KillerTeam),
					announcerEvent.MainReward,
					text2
				});
				goto IL_235;
			case AnnouncerLog.AnnouncerEventKinds.AFKGeneric:
			case AnnouncerLog.AnnouncerEventKinds.AFKEnd:
				text = string.Format(text, this.GetColoredPlayerName(announcerEvent.Killer));
				goto IL_235;
			default:
				if (announcerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerReconnected && announcerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected)
				{
					goto IL_235;
				}
				break;
			case AnnouncerLog.AnnouncerEventKinds.BombDelivery:
			case AnnouncerLog.AnnouncerEventKinds.LeaverModifierWarning:
				break;
			case AnnouncerLog.AnnouncerEventKinds.LeaverGeneric:
				base.StartCoroutine(this.TriggerLogTextAsyncWaitCombatObject(text, announcerEvent.Killer));
				return;
			}
			text = string.Format(text, this.GetColoredPlayerName(announcerEvent.Killer), announcerEvent.MainReward);
			IL_235:
			this.TriggerLogTextSend(text);
		}

		private void TriggerLogTextSend(string logText)
		{
			if (!string.IsNullOrEmpty(logText))
			{
				this._logList.Add(logText + "\n");
				this._hasNewLog = true;
				GameHubBehaviour.Hub.Chat.ClientReceiveLogMessage(logText);
			}
			if (this._logList.Count > this.MaxLines && this.MustRemoveLastLines)
			{
				this.RemoveLastLogLine();
			}
		}

		private IEnumerator TriggerLogTextAsyncWaitCombatObject(string logText, int playerId)
		{
			int attempts = 5;
			CombatObject combatObject = null;
			while (attempts > 0 && combatObject == null)
			{
				combatObject = CombatObject.GetCombatObject(playerId);
				yield return new WaitForSeconds(10f);
				attempts--;
			}
			string playerChatColorHex = HudUtils.RGBToHex(Color.gray);
			string playerName = string.Empty;
			if (combatObject != null)
			{
				playerChatColorHex = HudUtils.RGBToHex(GUIColorsInfo.GetChatColor((long)playerId, combatObject.Team));
				playerName = combatObject.Player.Name;
			}
			logText = string.Format(logText, playerChatColorHex, playerName);
			this.TriggerLogTextSend(logText);
			yield break;
		}

		public bool GetNewLog(int countLines, ref List<string> logList, ref string logLast)
		{
			if (this._hasNewLog && countLines > 0)
			{
				int num;
				if (countLines > this._logList.Count)
				{
					num = this._logList.Count;
				}
				else
				{
					num = countLines;
				}
				logList = this._logList.GetRange(this._logList.Count - num, num);
				logLast = this._logList[this._logList.Count - 1];
				this._hasNewLog = false;
				return true;
			}
			return false;
		}

		private void RemoveLastLogLine()
		{
			if (this._logList.Count == 0)
			{
				return;
			}
			this._logList.RemoveAt(0);
		}

		public string GetColoredPlayerName(int playerID)
		{
			CombatObject combatObject = CombatObject.GetCombatObject(playerID);
			if (combatObject)
			{
				Color chatColor = GUIColorsInfo.GetChatColor((long)playerID, combatObject.Team);
				string arg = HudUtils.RGBToHex(chatColor);
				return string.Format("[{0}]{1}[-]", arg, combatObject.Player.Name);
			}
			return " ";
		}

		public Color GetColorByPlayerId(int playerID)
		{
			CombatObject combatObject = CombatObject.GetCombatObject(playerID);
			if (combatObject)
			{
				return GUIColorsInfo.GetColorByPlayerCarId(combatObject.Player.PlayerCarId.GetInstanceId(), true);
			}
			return Color.black;
		}

		public string GetColoredVictimTeamName(AnnouncerLog.AnnouncerEventKinds topAnnounceKinds)
		{
			if (topAnnounceKinds == AnnouncerLog.AnnouncerEventKinds.BluWipe)
			{
				string text = HudUtils.RGBToHex(Color.cyan);
				return string.Concat(new string[]
				{
					"[",
					text,
					"]",
					Language.Get("TEAM_NAME_RAIO", TranslationSheets.Announcer),
					"[-]"
				});
			}
			if (topAnnounceKinds != AnnouncerLog.AnnouncerEventKinds.RedWipe)
			{
				AnnouncerManager.Log.Error("GRACHA - " + StackTraceUtility.ExtractStackTrace());
				return "ERROR - " + topAnnounceKinds;
			}
			string text2 = HudUtils.RGBToHex(Color.red);
			return string.Concat(new string[]
			{
				"[",
				text2,
				"]",
				Language.Get("TEAM_NAME_FOGO", TranslationSheets.Announcer),
				"[-]"
			});
		}

		private string GetColoredTeamName(TeamKind team)
		{
			switch (team)
			{
			case TeamKind.Red:
			{
				string text = HudUtils.RGBToHex(Color.red);
				return string.Concat(new string[]
				{
					"[",
					text,
					"]",
					Language.Get("TEAM_NAME_FOGO", TranslationSheets.Announcer),
					"[-]"
				});
			}
			case TeamKind.Blue:
			{
				string text2 = HudUtils.RGBToHex(Color.cyan);
				return string.Concat(new string[]
				{
					"[",
					text2,
					"]",
					Language.Get("TEAM_NAME_RAIO", TranslationSheets.Announcer),
					"[-]"
				});
			}
			case TeamKind.Neutral:
				return Language.Get("TEAM_NAME_NEUTRAL", TranslationSheets.Announcer);
			default:
				AnnouncerManager.Log.Error("GRACHA - " + StackTraceUtility.ExtractStackTrace());
				return "ERROR - " + team;
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this._lastTopLogTime = 0L;
			this._logList.Clear();
			this._hasNewLog = false;
			this._audioPlaying = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(AnnouncerManager));

		public AnnouncerInfo Announcerinfo;

		public bool MustRemoveLastLines = true;

		public int MaxLines = 50;

		public int TopAnnounceDurationMillis;

		private readonly List<string> _logList = new List<string>();

		private bool _hasNewLog;

		private bool _audioPlaying;

		private long _lastTopLogTime;

		private bool _isHudBusy;

		[Serializable]
		public class QueuedAnnouncerLog
		{
			public QueuedAnnouncerLog(AnnouncerEvent announcerEvent, AnnouncerLog announcerLog)
			{
				this.AnnouncerEvent = announcerEvent;
				this.AnnouncerLog = announcerLog;
			}

			public readonly AnnouncerEvent AnnouncerEvent;

			public readonly AnnouncerLog AnnouncerLog;
		}
	}
}
