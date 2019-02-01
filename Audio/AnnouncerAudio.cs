using System;
using FMod;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class AnnouncerAudio : AudioQueue
	{
		private new void Cleanup()
		{
			if (this.bombDeliveredSnapshot != null && !this.bombDeliveredSnapshot.IsInvalidated())
			{
				this.bombDeliveredSnapshot.KeyOff();
				this.bombDeliveredSnapshot.Stop();
			}
			if (this.matchendSnapshot != null && !this.matchendSnapshot.IsInvalidated())
			{
				this.matchendSnapshot.Stop();
			}
			if (this.deathSnapshotAudio != null && !this.deathSnapshotAudio.IsInvalidated())
			{
				this.deathSnapshotAudio.KeyOff();
				this.deathSnapshotAudio.Stop();
			}
			if (this.crowdLoopInstance != null)
			{
				this.crowdLoopInstance.Stop();
			}
		}

		public void Play(AnnouncerVoiceOverType announcerVOType)
		{
			switch (announcerVOType)
			{
			case AnnouncerVoiceOverType.IntroArena:
				this.PlayAudio(this.announcerVO.IntroArena, true);
				break;
			case AnnouncerVoiceOverType.MatchFound:
				this.PlayAudio(this.announcerVO.MatchFound, true);
				break;
			case AnnouncerVoiceOverType.PickStart:
				this.PlayAudio(this.announcerVO.PickStart, true);
				break;
			case AnnouncerVoiceOverType.PickCountdown:
				this.PlayAudio(this.announcerVO.PickCountdown, true);
				break;
			case AnnouncerVoiceOverType.PickEnd:
				this.PlayAudio(this.announcerVO.PickEnd, true);
				break;
			}
		}

		public void Initialize()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
				return;
			}
			base.SetSettings(GameHubBehaviour.Hub.AudioSettings);
			this.announcerVO = GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex].voiceOver;
			this.crowdConfiguration = GameHubBehaviour.Hub.AudioSettings.defaultCrowd;
			this.RegisterEvents();
		}

		private void OnDisable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			this.Cleanup();
			this.UnregisterEvents();
		}

		private void RegisterEvents()
		{
			Game game = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Game) as Game;
			if (game != null)
			{
				game.FinishedLoading += this.GameFinishedLoading;
			}
			AudioOptions audio = GameHubBehaviour.Hub.Options.Audio;
			audio.OnAnnouncerIndexChanged = (Action)Delegate.Combine(audio.OnAnnouncerIndexChanged, new Action(this.OnAnnouncerIndexChanged));
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop += this.OnBombDrop;
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted += this.ListenToOvertimeStarted;
			GameHubBehaviour.Hub.BombManager.ListenToBombAlmostDeliveredTriggerEnter += this.OnBombEnterAlmostDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToBombLastCurveTriggerEnter += this.OnBombEnterLastCurve;
			GameHubBehaviour.Hub.BombManager.ListenToBombFirstCurveTriggerEnter += this.OnBombEnterFirstCurve;
			GameHubBehaviour.Hub.BombManager.ListenToBombTrackEntryTriggerEnter += this.OnBombEnterTrackEntrace;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.Players_ListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.Players_ListenToObjectSpawn;
			GameHubBehaviour.Hub.Announcer.ListenToEvent += this.OnHudAnnounceTriggered;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
		}

		private void OnAnnouncerIndexChanged()
		{
			this.announcerVO = GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex].voiceOver;
			if (GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.Game)
			{
				GameHubBehaviour.Hub.Swordfish.MatchBI.ClientAnnouncerConfigured(1);
			}
		}

		private void Players_ListenToObjectSpawn(PlayerEvent data)
		{
			PlayerData playerByObjectId = GameHubBehaviour.Hub.Players.GetPlayerByObjectId(data.TargetId);
			if (playerByObjectId.CharacterId != GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterId)
			{
				return;
			}
			this.ClearDeathSnapshot();
		}

		private void Players_ListenToObjectUnspawn(PlayerEvent data)
		{
			if (data.Reason != SpawnReason.Death || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.BombDelivery)
			{
				return;
			}
			PlayerData playerByObjectId = GameHubBehaviour.Hub.Players.GetPlayerByObjectId(data.TargetId);
			if (playerByObjectId.CharacterId != GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterId)
			{
				return;
			}
			this.ClearDeathSnapshot();
			this.deathSnapshotAudio = FMODAudioManager.PlayAt(GameHubBehaviour.Hub.AudioSettings.DeathSnapshot, base.transform);
		}

		public void CommentOnScore()
		{
			int num = 0;
			int num2 = 0;
			TeamKind team = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team;
			if (team != TeamKind.Blue)
			{
				if (team == TeamKind.Red)
				{
					num = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed;
					num2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue;
				}
			}
			else
			{
				num = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue;
				num2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed;
			}
			if (num != 0)
			{
				if (num != 1)
				{
					if (num != 2)
					{
						return;
					}
					if (num2 == 0)
					{
						this.PlayAudio(this.announcerVO.BombRespawn20, true);
						return;
					}
					if (num2 == 1)
					{
						this.PlayAudio(this.announcerVO.BombRespawn21, true);
						return;
					}
					if (num2 != 2)
					{
						return;
					}
					this.PlayAudio(this.announcerVO.BombRespawn22, true);
					return;
				}
				else
				{
					if (num2 == 0)
					{
						this.PlayAudio(this.announcerVO.BombRespawn10, true);
						return;
					}
					if (num2 == 1)
					{
						this.PlayAudio(this.announcerVO.BombRespawn11, true);
						return;
					}
					if (num2 != 2)
					{
						return;
					}
					this.PlayAudio(this.announcerVO.BombRespawn12, true);
					return;
				}
			}
			else
			{
				if (num2 == 1)
				{
					this.PlayAudio(this.announcerVO.BombRespawn01, true);
					return;
				}
				if (num2 != 2)
				{
					return;
				}
				this.PlayAudio(this.announcerVO.BombRespawn02, true);
				return;
			}
		}

		public void CallResult(int playerScore, int enemyScore)
		{
			switch (playerScore)
			{
			case 0:
				if (enemyScore == 3)
				{
					this.PlayAudio(this.announcerVO.Defeat30, true);
					return;
				}
				return;
			case 1:
				if (enemyScore == 3)
				{
					this.PlayAudio(this.announcerVO.Defeat31, true);
					return;
				}
				return;
			case 2:
				if (enemyScore == 3)
				{
					this.PlayAudio(this.announcerVO.Defeat32, true);
					return;
				}
				return;
			case 3:
				if (enemyScore == 0)
				{
					this.PlayAudio(this.announcerVO.Victory30, true);
					return;
				}
				if (enemyScore == 1)
				{
					this.PlayAudio(this.announcerVO.Victory31, true);
					return;
				}
				if (enemyScore != 2)
				{
					return;
				}
				this.PlayAudio(this.announcerVO.Victory32, true);
				return;
			default:
				return;
			}
		}

		private void GameFinishedLoading()
		{
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				MusicManager.StopMusic();
				MusicManager.PlayAmbienceOnly(MusicManager.State.InGame);
				if (this.crowdLoopInstance == null)
				{
					this.crowdLoopInstance = FMODAudioManager.PlayAt(this.crowdConfiguration.CrowdLoop, base.transform);
				}
			}
		}

		private void UnregisterEvents()
		{
			Game game = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Game) as Game;
			if (game != null)
			{
				game.FinishedLoading -= this.GameFinishedLoading;
			}
			AudioOptions audio = GameHubBehaviour.Hub.Options.Audio;
			audio.OnAnnouncerIndexChanged = (Action)Delegate.Remove(audio.OnAnnouncerIndexChanged, new Action(this.OnAnnouncerIndexChanged));
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop -= this.OnBombDrop;
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted -= this.ListenToOvertimeStarted;
			GameHubBehaviour.Hub.BombManager.ListenToBombAlmostDeliveredTriggerEnter -= this.OnBombEnterAlmostDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToBombLastCurveTriggerEnter -= this.OnBombEnterLastCurve;
			GameHubBehaviour.Hub.BombManager.ListenToBombFirstCurveTriggerEnter -= this.OnBombEnterFirstCurve;
			GameHubBehaviour.Hub.BombManager.ListenToBombTrackEntryTriggerEnter -= this.OnBombEnterTrackEntrace;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.Players_ListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.Players_ListenToObjectSpawn;
			GameHubBehaviour.Hub.Announcer.ListenToEvent -= this.OnHudAnnounceTriggered;
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
		}

		private void ListenToStateChanged(GameState ChangedState)
		{
			if (ChangedState.StateKind != GameState.GameStateKind.Game)
			{
				this.Cleanup();
			}
			else
			{
				this.lastBombOwnerTeam = TeamKind.Neutral;
			}
		}

		private void AnnounceBombDelivery(TeamKind scoredTeam)
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerTeam == scoredTeam)
			{
				this.PlayAudio(this.announcerVO.BombDeliverAlly, false);
				this.PlayCrowdAudio(this.crowdConfiguration.CrowdAllyDelivery);
			}
			else
			{
				this.PlayAudio(this.announcerVO.BombDeliverEnemy, false);
				this.PlayCrowdAudio(this.crowdConfiguration.CrowdEnemyDelivery);
			}
		}

		private void OnBombEnterSection(FMODVoiceOverAsset assetAllyTeam, FMODVoiceOverAsset assetEnemyTeam, TeamKind trackedTeam)
		{
			TeamKind team = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team;
			if (team == trackedTeam)
			{
				this.PlayAudio(assetEnemyTeam, true);
			}
			else
			{
				this.PlayAudio(assetAllyTeam, true);
			}
		}

		private void OnBombEnterAlmostDelivery(TeamKind trackTeamKind)
		{
			TeamKind team = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team;
			if (team == trackTeamKind)
			{
				this.PlayAudio(this.announcerVO.BombAlmostDeliveryEnemy, true);
				this.PlayCrowdAudio(this.crowdConfiguration.CrowdEnemyNearDelivery);
			}
			else
			{
				this.PlayCrowdAudio(this.crowdConfiguration.CrowdAllyNearDelivery);
			}
		}

		private void OnBombEnterTrackEntrace(TeamKind trackTeamKind)
		{
			this.OnBombEnterSection(this.announcerVO.BombEntersTrackAlly, this.announcerVO.BombEntersTrackEnemy, trackTeamKind);
		}

		private void OnBombEnterFirstCurve(TeamKind trackTeamKind)
		{
			this.OnBombEnterSection(this.announcerVO.BombFirstCurveAlly, this.announcerVO.BombFirstCurveEnemy, trackTeamKind);
		}

		private void OnBombEnterLastCurve(TeamKind trackTeamKind)
		{
			this.OnBombEnterSection(this.announcerVO.BombLastCurveAlly, this.announcerVO.BombLastCurveEnemy, trackTeamKind);
		}

		private void OnPhaseChange(BombScoreBoard.State state)
		{
			this.ClearDeathSnapshot();
			BombScoreBoard.State previouState = GameHubBehaviour.Hub.BombManager.ScoreBoard.PreviouState;
			if (previouState == BombScoreBoard.State.Replay)
			{
				this.CommentOnScore();
			}
			if (state != BombScoreBoard.State.Replay)
			{
				if (state != BombScoreBoard.State.BombDelivery)
				{
					if (state == BombScoreBoard.State.Shop)
					{
						if (this.bombDeliveredSnapshot != null)
						{
							this.bombDeliveredSnapshot.KeyOff();
							this.bombDeliveredSnapshot = null;
						}
					}
				}
				else
				{
					this.PlayAudio(this.announcerVO.StartRound, true);
					if (this.bombDeliveredSnapshot != null)
					{
						this.bombDeliveredSnapshot.KeyOff();
						this.bombDeliveredSnapshot = null;
					}
				}
			}
			else
			{
				this.bombDeliveredSnapshot = FMODAudioManager.PlayAt(GameHubBehaviour.Hub.AudioSettings.BombDeliverySnapshot, base.transform);
				this.AnnounceReplay();
			}
		}

		private void ListenToOvertimeStarted()
		{
			this.PlayAudio(this.announcerVO.Overtime, true);
		}

		private void AnnounceReplay()
		{
			int num = 0;
			int num2 = 0;
			TeamKind team = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team;
			if (team != TeamKind.Blue)
			{
				if (team == TeamKind.Red)
				{
					num = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed;
					num2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue;
				}
			}
			else
			{
				num = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue;
				num2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed;
			}
			if (num == 3 || num2 == 3)
			{
				this.CallResult(num, num2);
			}
			else
			{
				RoundStats roundStats = GameHubBehaviour.Hub.BombManager.ScoreBoard.Rounds[GameHubBehaviour.Hub.BombManager.ScoreBoard.Rounds.Count - 1];
				if (roundStats.DeliverTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
				{
					this.PlayAudio(this.announcerVO.ReplayAttack, false);
				}
				else
				{
					this.PlayAudio(this.announcerVO.ReplayProtect, false);
				}
			}
		}

		private void OnBombPickup(TeamKind carrierTeam, bool nearGoal)
		{
			this.lastBombOwnerTeam = carrierTeam;
			TeamKind team = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team;
			bool flag = carrierTeam == team;
			if (flag)
			{
				this.PlayCrowdAudio(this.crowdConfiguration.CrowdAllyBombPicked);
			}
			else
			{
				this.PlayCrowdAudio(this.crowdConfiguration.CrowdEnemyBombPicked);
				if (nearGoal)
				{
					this.PlayAudio(this.announcerVO.BombPickedAlmostDeliveryEnemy, true);
				}
				else
				{
					this.PlayAudio(this.announcerVO.BombPickedEnemy, true);
				}
			}
		}

		private void OnBombDrop(BombInstance bombinstance, SpawnReason reason, int causer)
		{
			if (reason != SpawnReason.TriggerDrop && reason != SpawnReason.BrokenLink)
			{
				return;
			}
			TeamKind team = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team;
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(causer);
			bool flag = team == playerOrBotsByObjectId.Team;
			if (flag)
			{
				this.PlayCrowdAudio(this.crowdConfiguration.CrowdAllyBombDrop);
			}
			else
			{
				this.PlayAudio(this.announcerVO.BombDropped, true);
				this.PlayCrowdAudio(this.crowdConfiguration.CrowdEnemyBombDrop);
			}
		}

		private void OnBombDelivery(int causerId, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			this.AnnounceBombDelivery(scoredTeam);
		}

		private void PlayKillAudio(AnnouncerManager.QueuedAnnouncerLog queuedAnnouncerLog)
		{
			switch (queuedAnnouncerLog.AnnouncerEvent.CurrentKillingSpree)
			{
			case 2:
				if (queuedAnnouncerLog.AnnouncerEvent.KillerTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
				{
					this.PlayAudio(this.announcerVO.DoubleKill, true);
				}
				break;
			case 3:
				if (queuedAnnouncerLog.AnnouncerEvent.KillerTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
				{
					this.PlayAudio(this.announcerVO.TripleKill, true);
				}
				break;
			case 4:
				if (queuedAnnouncerLog.AnnouncerEvent.KillerTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
				{
					this.PlayAudio(this.announcerVO.QuadKill, true);
				}
				break;
			default:
				this.PlayAudio(this.announcerVO.NormalKill, true);
				break;
			}
		}

		private void OnHudAnnounceTriggered(AnnouncerManager.QueuedAnnouncerLog queuedAnnouncerLog)
		{
			if (GameHubBehaviour.Hub.User.IsNarrator)
			{
				return;
			}
			AnnouncerLog.AnnouncerEventKinds announcerEventKind = queuedAnnouncerLog.AnnouncerLog.AnnouncerEventKind;
			bool flag = false;
			switch (announcerEventKind)
			{
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayer:
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayerWithAssists:
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironment:
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironmentWithAssists:
				this.PlayKillAudio(queuedAnnouncerLog);
				break;
			default:
				switch (announcerEventKind)
				{
				case AnnouncerLog.AnnouncerEventKinds.BombPicked:
					this.OnBombPickup(queuedAnnouncerLog.AnnouncerEvent.KillerTeam, false);
					return;
				case AnnouncerLog.AnnouncerEventKinds.BombPickedNearGoal:
					this.OnBombPickup(queuedAnnouncerLog.AnnouncerEvent.KillerTeam, true);
					return;
				case AnnouncerLog.AnnouncerEventKinds.BombShootingNearGoal:
				{
					FMODVoiceOverAsset asset = (queuedAnnouncerLog.AnnouncerEvent.KillerTeam != GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? this.announcerVO.BombShootingEnemyTeam : this.announcerVO.BombShootingAlliedTeam;
					this.PlayAudio(asset, true);
					break;
				}
				default:
					if (announcerEventKind == AnnouncerLog.AnnouncerEventKinds.BluWipe || announcerEventKind == AnnouncerLog.AnnouncerEventKinds.RedWipe)
					{
						if (queuedAnnouncerLog.AnnouncerEvent.KillerTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
						{
							this.PlayAudio(this.announcerVO.TeamWipe, true);
						}
						return;
					}
					if (announcerEventKind != AnnouncerLog.AnnouncerEventKinds.FirstBlood)
					{
						return;
					}
					this.PlayAudio(this.announcerVO.FirstBlood, true);
					return;
				}
				break;
			case AnnouncerLog.AnnouncerEventKinds.KillNemesisRevenge:
				flag = true;
				break;
			}
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(queuedAnnouncerLog.AnnouncerEvent.Killer);
			VoiceOverController voiceOverController = (!(playerOrBotsByObjectId == null) && !(playerOrBotsByObjectId.CharacterInstance == null)) ? playerOrBotsByObjectId.CharacterInstance.GetBitComponent<VoiceOverController>() : null;
			if (voiceOverController)
			{
				if (flag)
				{
					voiceOverController.PlayRevengeAudio(queuedAnnouncerLog);
				}
				else
				{
					voiceOverController.PlayKillAudio(queuedAnnouncerLog);
				}
			}
		}

		private void ClearDeathSnapshot()
		{
			if (this.deathSnapshotAudio != null && !this.deathSnapshotAudio.IsInvalidated())
			{
				this.deathSnapshotAudio.KeyOff();
			}
		}

		public void PlayAudio(FMODVoiceOverAsset asset, bool replayRestriction = true)
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			if (asset != null)
			{
				if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay && replayRestriction)
				{
					return;
				}
				base.Enqueue(asset, base.transform);
			}
		}

		private void PlayCrowdAudio(FMODAsset asset)
		{
			if (asset && !GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				FMODAudioManager.PlayOneShotAt(asset, base.transform.position, 0);
			}
		}

		public void PlayPauseAudio()
		{
			this.PlayAudio(this.announcerVO.Pause, true);
		}

		public void PlayUnpauseAudio()
		{
			this.PlayAudio(this.announcerVO.Unpause, true);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AnnouncerAudio));

		private FMODAudioManager.FMODAudio bombDeliveredSnapshot;

		private FMODAudioManager.FMODAudio matchendSnapshot;

		private FMODAudioManager.FMODAudio deathSnapshotAudio;

		private TeamKind lastBombOwnerTeam;

		public FMODAudioManager.FMODAudio crowdLoopInstance;

		private CrowdConfiguration crowdConfiguration;

		private AnnouncerVoiceOver announcerVO;
	}
}
