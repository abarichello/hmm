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
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Players.Presenting;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Announcer
{
	public class AnnouncerManager : GameHubBehaviour, ICleanupListener, IAnnouncerService
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
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(causerid);
			ScrapInfo[] scrapPerBombDelivery = GameHubBehaviour.Hub.ScrapLevel.ScrapPerBombDelivery;
			AnnouncerManager.Log.DebugFormat("BombDelivery event received - ScrapPerBombDelivery.Length[{0}], Round[{1}]", new object[]
			{
				scrapPerBombDelivery.Length,
				GameHubBehaviour.Hub.BombManager.ScoreBoard.Round
			});
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
			this.TriggerAnnounce(new QueuedAnnouncerLog(announcerEvent, this.Announcerinfo.GetAnnouncerLog(announcerEvent.AnnouncerEventKind)));
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
			string text = string.Empty;
			if (announce.AssistPlayers != null)
			{
				for (int i = 0; i < announce.AssistPlayers.Count; i++)
				{
					int num = announce.AssistPlayers[i];
					text = text + num + ",";
				}
			}
			AnnouncerManager.Log.DebugFormat("AnnouncerManager.Trigger - AnnouncerEventKind[{0}], Killer:[{1}], Victim[{2}], MainReward[{3}], AssistReward[{4}], AssistPlayers[{5}], LastKillStreak[{6}] Spree [{7}]", new object[]
			{
				announce.AnnouncerEventKind,
				announce.Killer,
				announce.Victim,
				announce.MainReward,
				announce.AssistReward,
				text,
				announce.LastKillStreak,
				announce.CurrentKillingSpree
			});
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
			this.TriggerAnnounce(new QueuedAnnouncerLog(announceevent, this.Announcerinfo.GetAnnouncerLog(announceevent.AnnouncerEventKind)));
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<QueuedAnnouncerLog> ListenToEvent;

		private void TriggerAnnounce(QueuedAnnouncerLog enqueuedAnnounce)
		{
			this._lastTopLogTime = (long)GameHubBehaviour.Hub.GameTime.GetSynchTime();
			AnnouncerLog announcerLog = this.Announcerinfo.GetAnnouncerLog(enqueuedAnnounce.AnnouncerEvent.AnnouncerEventKind);
			if (announcerLog.Audio != null && !SpectatorController.IsSpectating && announcerLog.clientOnlyAudio && GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId == enqueuedAnnounce.AnnouncerEvent.Killer)
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
				if (logAnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerReconnected && logAnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected && logAnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.AFKGeneric && logAnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.SpectatorConnected)
				{
					goto IL_E4;
				}
				break;
			}
			this.TriggerLogText(enqueuedAnnounce);
			IL_E4:
			if (this.ListenToEvent != null)
			{
				this.ListenToEvent(enqueuedAnnounce);
			}
		}

		private void TriggerLogText(QueuedAnnouncerLog enqueuedAnnounce)
		{
			AnnouncerEvent announcerEvent = enqueuedAnnounce.AnnouncerEvent;
			AnnouncerLog announcerLog = enqueuedAnnounce.AnnouncerLog;
			string logText = announcerLog.LocalizedText;
			AnnouncerLog.AnnouncerEventKinds announcerEventKind = announcerEvent.AnnouncerEventKind;
			string text = string.Empty;
			if (enqueuedAnnounce.AnnouncerEvent.AssistPlayers != null && enqueuedAnnounce.AnnouncerEvent.AssistPlayers.Count > 0)
			{
				text += this.GetColoredPlayerName(announcerEvent.AssistPlayers[0]);
				for (int i = 1; i < announcerEvent.AssistPlayers.Count; i++)
				{
					text = text + ", " + this.GetColoredPlayerName(announcerEvent.AssistPlayers[i]);
				}
			}
			switch (announcerEventKind)
			{
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayer:
			case AnnouncerLog.AnnouncerEventKinds.PlayerEndedAKillStreak:
			case AnnouncerLog.AnnouncerEventKinds.PlayerEndedAKillStreakWithAssists:
				logText = Language.Format(logText, new object[]
				{
					this.GetColoredPlayerName(announcerEvent.Killer),
					this.GetColoredPlayerName(announcerEvent.Victim),
					announcerEvent.MainReward
				});
				goto IL_328;
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayerWithAssists:
				logText = Language.Format(logText, new object[]
				{
					this.GetColoredPlayerName(announcerEvent.Killer),
					this.GetColoredPlayerName(announcerEvent.Victim),
					announcerEvent.MainReward,
					text
				});
				goto IL_328;
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironment:
				logText = Language.Format(logText, new object[]
				{
					this.GetColoredPlayerName(announcerEvent.Victim),
					this.GetColoredTeamName(announcerEvent.KillerTeam),
					announcerEvent.MainReward
				});
				goto IL_328;
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironmentWithAssists:
				logText = Language.Format(logText, new object[]
				{
					this.GetColoredPlayerName(announcerEvent.Victim),
					this.GetColoredTeamName(announcerEvent.KillerTeam),
					announcerEvent.MainReward,
					text
				});
				goto IL_328;
			case AnnouncerLog.AnnouncerEventKinds.AFKGeneric:
			case AnnouncerLog.AnnouncerEventKinds.AFKEnd:
				logText = Language.Format(logText, new object[]
				{
					this.GetColoredPlayerName(announcerEvent.Killer)
				});
				goto IL_328;
			default:
				if (announcerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerReconnected && announcerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected)
				{
					goto IL_328;
				}
				break;
			case AnnouncerLog.AnnouncerEventKinds.BombDelivery:
			case AnnouncerLog.AnnouncerEventKinds.LeaverModifierWarning:
				break;
			case AnnouncerLog.AnnouncerEventKinds.LeaverGeneric:
				base.StartCoroutine(this.TriggerLogTextAsyncWaitCombatObject((GameHubBehaviour.Hub.Match.Kind != 4) ? logText : Language.Get("LEAVER_GENERIC_ON_CUSTOM", TranslationContext.Announcer), announcerEvent.Killer));
				return;
			case AnnouncerLog.AnnouncerEventKinds.SpectatorConnected:
				DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<string>(Observable.Do<string>(Observable.Select<string, string>(this.GetSpectatorName(announcerEvent.Killer), (string playerName) => Language.Format(logText, new object[]
				{
					playerName
				})), new Action<string>(this.TriggerLogTextSend))), this);
				return;
			}
			logText = Language.Format(logText, new object[]
			{
				this.GetColoredPlayerName(announcerEvent.Killer),
				announcerEvent.MainReward
			});
			IL_328:
			this.TriggerLogTextSend(logText);
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
				playerChatColorHex = HudUtils.RGBToHex(GUIColorsInfo.GetChatColor((long)playerId, combatObject.Team, false));
				playerName = this._diContainer.Resolve<IGetDisplayableNickName>().GetFormattedNickNameWithPlayerTag(combatObject.Player.PlayerId, combatObject.Player.Name, new long?(combatObject.Player.PlayerTag));
			}
			logText = Language.Format(logText, new object[]
			{
				playerChatColorHex,
				playerName
			});
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
				Color chatColor = GUIColorsInfo.GetChatColor((long)playerID, combatObject.Team, false);
				string arg = HudUtils.RGBToHex(chatColor);
				string formattedNickNameWithPlayerTag = this._diContainer.Resolve<IGetDisplayableNickName>().GetFormattedNickNameWithPlayerTag(combatObject.Player.PlayerId, combatObject.Player.Name, new long?(combatObject.Player.PlayerTag));
				return string.Format("[{0}]{1}[-]", arg, formattedNickNameWithPlayerTag);
			}
			return " ";
		}

		public IObservable<string> GetSpectatorName(int spectatorAddress)
		{
			PlayerData anyByAddress = GameHubBehaviour.Hub.Players.GetAnyByAddress((byte)spectatorAddress);
			return this._diContainer.Resolve<IGetDisplayableNickName>().GetLatestFormattedNickName(new DisplayableNicknameParameters
			{
				PlayerId = anyByAddress.PlayerId,
				PlayerName = anyByAddress.Name,
				UniversalId = anyByAddress.UserId
			});
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
					Language.Get("TEAM_NAME_RAIO", TranslationContext.Announcer),
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
				Language.Get("TEAM_NAME_FOGO", TranslationContext.Announcer),
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
					Language.Get("TEAM_NAME_FOGO", TranslationContext.Announcer),
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
					Language.Get("TEAM_NAME_RAIO", TranslationContext.Announcer),
					"[-]"
				});
			}
			case TeamKind.Neutral:
				return Language.Get("TEAM_NAME_NEUTRAL", TranslationContext.Announcer);
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

		[InjectOnClient]
		private DiContainer _diContainer;

		private bool _hasNewLog;

		private bool _audioPlaying;

		private long _lastTopLogTime;

		private bool _isHudBusy;
	}
}
