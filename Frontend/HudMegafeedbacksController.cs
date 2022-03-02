using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using GameScriptAnimation;
using HeavyMetalMachines.Achievements;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback;
using HeavyMetalMachines.PostProcessing;
using HeavyMetalMachines.Render;
using Hoplon.Logging;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudMegafeedbacksController : GameHubBehaviour, ICleanupListener
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

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event HudMegafeedbacksController.AnimationPlayingDelegate EvtAnimationStart;

		public void Awake()
		{
			this.MegafeedbackRoundWinGameObject.SetActive(false);
			this.MegafeedbackRoundLostGameObject.SetActive(false);
			this.MegafeedbackRaceStartAnimator.gameObject.SetActive(false);
			this.MegafeedbackDefeatGameObject.SetActive(false);
			this.MegafeedbackVictoryGameObject.SetActive(false);
			this.MegafeedbackLowHpGlassShatterGameObject.SetActive(false);
			this.MegafeedbackStrongCollisionGlassShatterGameObject.SetActive(false);
			this.MegafeedbackPlayerDeathObject.SetActive(false);
			this.MegafeedbackUltimateActivatedGameObject.SetActive(false);
			this.MegafeedbackBombAcquiredGameObject.SetActive(false);
			this.MegafeedbackBombLostGameObject.SetActive(false);
			this.BombWinEvent.OnExitEvent += this.BombWinOnExitEvent;
			this.BombLostEvent.OnExitEvent += this.BombLostOnExitEvent;
			this.RaceStartEvent.OnExitEvent += this.RaceStartOnExitEvent;
			this.DefeatEvent.OnExitEvent += this.DefeatEventOnExitEvent;
			this.VictoryEvent.OnExitEvent += this.VictoryEventOnExitEvent;
			this.LowHpGlassShatterEvent.OnExitEvent += this.LowHpGlassShatterEventOnExitEvent;
			this.StrongCollisionHpGlassShatterEvent.OnExitEvent += this.StrongCollisionHpGlassShatterEventOnExitEvent;
			this.PlayerDeathEvent.OnExitEvent += this.PlayerDeathGlassShatterEventOnExitEvent;
			this.UltimateActivatedGameObjectEvent.OnExitEvent += this.UltimateActivatedGameObjectEventOnExitEvent;
			this.BombAquiredGameObjectEvent.OnExitEvent += this.BombAquiredGameObjectEventOnExitEvent;
			this.BombLostGameObjectEvent.OnExitEvent += this.BombLostGameObjectEventOnExitEvent;
			this.TryAddCombatFeedbackEventListeners();
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn += this.OnStartingPlayerSpawnAnimationEvent;
			this.m_poDeathAnimation = new DeathAnimation(this.m_DeathAnimationStates, false, "DeathAnimation", this._postProcessing, this._gameCamera);
			this.m_poSpawnAnimation = new DeathAnimation(this.m_SpawnAnimationStates, true, "SpawnAnimation", this._postProcessing, this._gameCamera);
			this.m_poCameraAnimation = new CameraAnimation(this.m_DeathCameraAnimationParameters, this._postProcessing, this._gameCamera, this._gameCameraEngine);
			this.m_poDeathAnimation.OnDeathAnimationEndedEvent += this.OnDeathAnimationEnded;
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.BombManager.GridController.OnGridGamePlayersCreated += this.OnGridGamePlayersCreated;
				GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.OnPlayerSpawn;
				GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.OnPlayerUnspawn;
				GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate += this.OnBombMatchUpdate;
				GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivered;
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChanged;
			}
		}

		private void TryAddCombatFeedbackEventListeners()
		{
			if (this.WeakenExitEvent)
			{
				this.WeakenExitEvent.OnExitEvent += this.WeakenOnExitEvent;
			}
			if (this.StrongExitEvent)
			{
				this.StrongExitEvent.OnExitEvent += this.StrongOnExitEvent;
			}
			if (this.OverheatExitEvent)
			{
				this.OverheatExitEvent.OnExitEvent += this.OverheatOnExitEvent;
			}
			if (this.SilenceExitEvent)
			{
				this.SilenceExitEvent.OnExitEvent += this.SilenceOnExitEvent;
			}
		}

		private void TryRemoveCombatFeedbackEventListeners()
		{
			if (this.WeakenExitEvent)
			{
				this.WeakenExitEvent.OnExitEvent -= this.WeakenOnExitEvent;
			}
			if (this.StrongExitEvent)
			{
				this.StrongExitEvent.OnExitEvent -= this.StrongOnExitEvent;
			}
			if (this.OverheatExitEvent)
			{
				this.OverheatExitEvent.OnExitEvent -= this.OverheatOnExitEvent;
			}
			if (this.SilenceExitEvent)
			{
				this.SilenceExitEvent.OnExitEvent -= this.SilenceOnExitEvent;
			}
		}

		private void BombWinOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.DisableBombTriggerMegaFeedBack(this.MegafeedbackRoundWinGameObject);
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
		}

		private void BombLostOnExitEvent(HudFeedbackExitEvent exitEvent)
		{
			this.DisableBombTriggerMegaFeedBack(this.MegafeedbackRoundLostGameObject);
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
		}

		private void RaceStartOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.MegafeedbackRaceStartAnimator.gameObject.SetActive(false);
		}

		private void DefeatEventOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.DisableBombTriggerMegaFeedBack(this.MegafeedbackDefeatGameObject);
		}

		private void VictoryEventOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.DisableBombTriggerMegaFeedBack(this.MegafeedbackVictoryGameObject);
		}

		private void DisableBombTriggerMegaFeedBack(GameObject megaFeedbackGameObject)
		{
			megaFeedbackGameObject.SetActive(false);
			if (HudMegafeedbacksController.EvtAnimationStart != null)
			{
				HudMegafeedbacksController.EvtAnimationStart(false);
			}
		}

		private void LowHpGlassShatterEventOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.MegafeedbackLowHpGlassShatterGameObject.SetActive(false);
		}

		private void StrongCollisionHpGlassShatterEventOnExitEvent(HudFeedbackExitEvent exitEvent)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreboardState.BombDelivery)
			{
				return;
			}
			this.MegafeedbackStrongCollisionGlassShatterGameObject.SetActive(false);
			this.m_poDeathAnimation.Start();
		}

		private void PlayerDeathGlassShatterEventOnExitEvent(HudFeedbackExitEvent exitEvent)
		{
			this.MegafeedbackPlayerDeathObject.SetActive(false);
		}

		private void UltimateActivatedGameObjectEventOnExitEvent(HudFeedbackExitEvent exitEvent)
		{
			this.MegafeedbackUltimateActivatedGameObject.SetActive(false);
		}

		private void BombAquiredGameObjectEventOnExitEvent(HudFeedbackExitEvent exitEvent)
		{
			this.MegafeedbackBombAcquiredGameObject.SetActive(false);
		}

		private void BombLostGameObjectEventOnExitEvent(HudFeedbackExitEvent exitEvent)
		{
			this.MegafeedbackBombLostGameObject.SetActive(false);
		}

		private void WeakenOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.MegafeedbackWeakenGameObject.SetActive(false);
		}

		private void StrongOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.MegafeedbackStrongGameObject.SetActive(false);
		}

		private void OverheatOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.MegafeedbackOverheatGameObject.SetActive(false);
		}

		private void SilenceOnExitEvent(HudFeedbackExitEvent exitevent)
		{
			this.MegafeedbackSilenceGameObject.SetActive(false);
		}

		private void OnGridGamePlayersCreated()
		{
			HudMegafeedbacksController.Log.DebugFormat("OnGridGamePlayersCreated. GO name: {0}", new object[]
			{
				base.gameObject.name
			});
			if (this._speedometerListenerSetted)
			{
				HudMegafeedbacksController.Log.WarnFormat("Trying to set SpeedometerAnimation listener twice? Houston we have a problem.", new object[0]);
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.GridController.CurrentPlayer != null)
			{
				GameHubBehaviour.Hub.BombManager.GridController.CurrentPlayer.OnValueChanged += this.SetSpeedometerAnimationValue;
			}
			GameHubBehaviour.Hub.BombManager.GridController.ListenToGridGameFinished += this.OnGridGameFinished;
			this._speedometerListenerSetted = true;
		}

		private void OnPhaseChanged(BombScoreboardState currentPhase)
		{
			this.m_poDeathAnimation.Finish();
			this.m_poCameraAnimation.FinishCameraAnimation();
			this.m_poSpawnAnimation.Finish();
			if (currentPhase == BombScoreboardState.PreBomb)
			{
				base.StartCoroutine(this.PlayRaceStartAnimation());
			}
			if (currentPhase != BombScoreboardState.BombDelivery)
			{
				this.DisableCombatFeedbacks();
			}
		}

		private IEnumerator PlayRaceStartAnimation()
		{
			long now = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			long end = GameHubBehaviour.Hub.BombManager.ScoreBoard.Timeout;
			long start = Math.Max(0L, end - (long)Mathf.CeilToInt(1000f * this.AnimationTimeWhenGreenSeconds));
			HudMegafeedbacksController.Log.DebugFormat("PlayRaceStartAnimation: now: {0}, start: {1}, end: {2}", new object[]
			{
				now,
				start,
				end
			});
			if (now < end)
			{
				while (now < start)
				{
					yield return new WaitForSecondsRealtime((float)(start - now) / 1000f);
					now = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				}
				double slope = (double)this.AnimationTimeWhenGreenSeconds / (double)this.RaceStartTotalAnimationTime / (double)(end - start);
				float animatorTime = (float)(slope * (double)(now - start));
				if (animatorTime < 1f)
				{
					int num = (int)(now - start);
					HudMegafeedbacksController.Log.DebugFormat("PlayRaceStartAnimation: animator: {0}, FMOD: {1}", new object[]
					{
						animatorTime,
						num
					});
					this.MegafeedbackRaceStartAnimator.gameObject.SetActive(true);
					this.MegafeedbackRaceStartWidget.UpdateAnchors();
					this.MegafeedbackRaceStartAnimator.Play("active", 0, animatorTime);
					FMODAudioManager.PlayOneShotAt(this.raceStartAudio, this.MegafeedbackRaceStartAnimator.transform.position, num);
				}
				else
				{
					HudMegafeedbacksController.Log.Debug("PlayRaceStartAnimation: fail");
				}
			}
			else
			{
				HudMegafeedbacksController.Log.Debug("PlayRaceStartAnimation: skip");
			}
			HudMegafeedbacksController.Log.Debug("PlayRaceStartAnimation: done");
			yield break;
		}

		private void OnPlayerSpawn(PlayerEvent data)
		{
			if (data.TargetId != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				return;
			}
			if (!this.m_oPlayerCombatObject)
			{
				this.m_oPlayerCombatObject = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
				this.m_oPlayerCombatObject._data.OnHPChanged += this.OnPlayerHpChanged;
				this.m_oPlayerCombatObject.CustomGadget2.ClientListenToEffectStarted += this.OnUltimateActivated;
				if (this.m_oPlayerCombatObject.CustomGadget0.Kind == GadgetKind.Overheat)
				{
					this.m_oPlayerOverheatGadget = this.m_oPlayerCombatObject.CustomGadget0;
				}
				else if (this.m_oPlayerCombatObject.CustomGadget1.Kind == GadgetKind.Overheat)
				{
					this.m_oPlayerOverheatGadget = this.m_oPlayerCombatObject.CustomGadget1;
				}
				else if (this.m_oPlayerCombatObject.BoostGadget.Kind == GadgetKind.Overheat)
				{
					this.m_oPlayerOverheatGadget = this.m_oPlayerCombatObject.BoostGadget;
				}
			}
			this.m_poDeathAnimation.Finish();
			this.m_poCameraAnimation.FinishCameraAnimation();
			this.m_poSpawnAnimation.Finish();
		}

		private void OnStartingPlayerSpawnAnimationEvent(PlayerEvent playerEvent)
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData != null && GameHubBehaviour.Hub.Players.CurrentPlayerData.GetPlayerCarObjectId() == playerEvent.TargetId)
			{
				this.m_poDeathAnimation.Finish();
				this.m_poCameraAnimation.FinishCameraAnimation();
				this.m_poSpawnAnimation.Start();
			}
		}

		private void OnDeathAnimationEnded()
		{
			if (!this._playback.IsRunningReplay)
			{
				this.m_poCameraAnimation.StartCameraAnimation("CameraAnimation", () => this.m_poDeathAnimation.IsRunning() || this.m_poSpawnAnimation.IsRunning());
			}
		}

		private void OnPlayerUnspawn(PlayerEvent data)
		{
			if (data.TargetId != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				return;
			}
			this._gameCamera.Shake(this._CameraShakeStrenghtOnDeath);
			this.MegafeedbackStrongCollisionGlassShatterGameObject.SetActive(true);
			this.DisableCombatFeedbacks();
		}

		private void DisableCombatFeedbacks()
		{
			if (this.MegafeedbackWeakenGameObject)
			{
				this.MegafeedbackWeakenGameObject.SetActive(false);
			}
			if (this.MegafeedbackStrongGameObject)
			{
				this.MegafeedbackStrongGameObject.SetActive(false);
			}
			if (this.MegafeedbackOverheatGameObject)
			{
				this.MegafeedbackOverheatGameObject.SetActive(false);
			}
			if (this.MegafeedbackSilenceGameObject)
			{
				this.MegafeedbackSilenceGameObject.SetActive(false);
			}
		}

		public void OnDestroy()
		{
			this.BombWinEvent.OnExitEvent -= this.BombWinOnExitEvent;
			this.BombLostEvent.OnExitEvent -= this.BombLostOnExitEvent;
			this.RaceStartEvent.OnExitEvent -= this.RaceStartOnExitEvent;
			this.DefeatEvent.OnExitEvent -= this.DefeatEventOnExitEvent;
			this.LowHpGlassShatterEvent.OnExitEvent -= this.LowHpGlassShatterEventOnExitEvent;
			this.StrongCollisionHpGlassShatterEvent.OnExitEvent -= this.StrongCollisionHpGlassShatterEventOnExitEvent;
			this.PlayerDeathEvent.OnExitEvent -= this.PlayerDeathGlassShatterEventOnExitEvent;
			this.UltimateActivatedGameObjectEvent.OnExitEvent -= this.UltimateActivatedGameObjectEventOnExitEvent;
			this.BombAquiredGameObjectEvent.OnExitEvent -= this.BombAquiredGameObjectEventOnExitEvent;
			this.BombLostGameObjectEvent.OnExitEvent -= this.BombLostGameObjectEventOnExitEvent;
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn -= this.OnStartingPlayerSpawnAnimationEvent;
			this.TryRemoveCombatFeedbackEventListeners();
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate -= this.OnBombMatchUpdate;
				GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivered;
				GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.OnPlayerSpawn;
				GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.OnPlayerUnspawn;
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChanged;
				HudMegafeedbacksController.Log.DebugFormat("Remove speedometerListener: name {0}", new object[]
				{
					base.gameObject.name
				});
				GameHubBehaviour.Hub.BombManager.GridController.OnGridGamePlayersCreated -= this.OnGridGamePlayersCreated;
				if (GameHubBehaviour.Hub.BombManager.GridController.CurrentPlayer != null)
				{
					HudMegafeedbacksController.Log.DebugFormat("Remove speedometer animation Listener: name {0}", new object[]
					{
						base.gameObject.name
					});
					GameHubBehaviour.Hub.BombManager.GridController.CurrentPlayer.OnValueChanged -= this.SetSpeedometerAnimationValue;
					GameHubBehaviour.Hub.BombManager.GridController.ListenToGridGameFinished -= this.OnGridGameFinished;
				}
				this._speedometerListenerSetted = false;
			}
			if (this.m_oPlayerCombatObject)
			{
				this.m_oPlayerCombatObject._data.OnHPChanged -= this.OnPlayerHpChanged;
				this.m_oPlayerCombatObject.CustomGadget2.ClientListenToEffectStarted -= this.OnUltimateActivated;
				this.m_oPlayerCombatObject = null;
			}
			this.m_oPlayerOverheatGadget = null;
			this.m_poDeathAnimation.OnDeathAnimationEndedEvent -= this.OnDeathAnimationEnded;
			this.m_poDeathAnimation.Finish();
			this.m_poSpawnAnimation.Finish();
			this.m_poCameraAnimation.FinishCameraAnimation();
			this.m_poDeathAnimation = null;
			this.m_poSpawnAnimation = null;
			this.m_poCameraAnimation = null;
		}

		private void OnBombMatchUpdate()
		{
			if (!this.m_oPlayerCombatObject)
			{
				return;
			}
			if (this.m_boIsCarryingBomb && !GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.m_oPlayerCombatObject.Id.ObjId))
			{
				this.m_boIsCarryingBomb = false;
				this.MegafeedbackBombLostGameObject.SetActive(true);
				return;
			}
			if (!this.m_boIsCarryingBomb && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.m_oPlayerCombatObject.Id.ObjId))
			{
				this.m_boIsCarryingBomb = true;
				this.MegafeedbackBombAcquiredGameObject.SetActive(true);
			}
		}

		private void OnBombDelivered(int causerId, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			this.m_boIsCarryingBomb = false;
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.BombDelivery && !GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				base.StartCoroutine(this.BombTrigger(scoredTeam));
			}
		}

		private IEnumerator BombTrigger(TeamKind teamScorer)
		{
			GameHubBehaviour.Hub.Announcer.IsHudBusy = true;
			yield return new WaitForSecondsRealtime(this.RoundWinOrLostAnimationDelayInSec);
			this.GameGui.ShowGameHud(false);
			if (HudMegafeedbacksController.EvtAnimationStart != null)
			{
				HudMegafeedbacksController.EvtAnimationStart(true);
			}
			BombScoreBoard bombScoreBoard = GameHubBehaviour.Hub.BombManager.ScoreBoard;
			int targetScore = GameHubBehaviour.Hub.BombManager.Rules.BombScoreTarget;
			bool isClientScoreBoardOutdated = Time.unscaledTime - GameHubBehaviour.Hub.BombManager.ScoreBoard.LastScoreUpdateTime > 5f;
			if (isClientScoreBoardOutdated)
			{
				targetScore--;
			}
			bool isPlayerTeamScore = SpectatorController.IsSpectating || GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamScorer;
			int teamScorerScore = (teamScorer != TeamKind.Red) ? bombScoreBoard.BombScoreBlue : bombScoreBoard.BombScoreRed;
			if (teamScorerScore == targetScore)
			{
				HudMegafeedbacksController.Log.DebugFormat("MegaFeedBack Player{0} PlayerTeam: {1}, ScoreTeam: {2}, IsSpectating: {3}, BombsRed: {4}, BombsBlue: {5} ", new object[]
				{
					(!isPlayerTeamScore) ? "LOSE" : "WIN",
					GameHubBehaviour.Hub.Players.CurrentPlayerData.Team,
					teamScorer,
					SpectatorController.IsSpectating,
					bombScoreBoard.BombScoreRed,
					bombScoreBoard.BombScoreBlue
				});
				if (isPlayerTeamScore)
				{
					this.MegafeedbackVictoryGameObject.SetActive(true);
					FMODAudioManager.PlayOneShotAt(this._matchWinSfx, Vector3.zero, 0);
				}
				else
				{
					this.MegafeedbackDefeatGameObject.SetActive(true);
					FMODAudioManager.PlayOneShotAt(this._matchLossSfx, Vector3.zero, 0);
				}
				this.TryToGiveLastGoalAchievements(isPlayerTeamScore);
				yield break;
			}
			this.EnableRoundWingAnimation(isPlayerTeamScore, teamScorerScore);
			yield break;
		}

		private void EnableRoundWingAnimation(bool isPlayerTeamScore, int teamScorerScore)
		{
			int num = (teamScorerScore <= 1) ? 0 : 1;
			if (isPlayerTeamScore)
			{
				this.MegafeedbackRoundWinWingColorAnimator.Color.a = (float)num;
				this.MegafeedbackRoundWinGameObject.SetActive(true);
				FMODAudioManager.PlayOneShotAt(this._roundWinSfx, Vector3.zero, 0);
				return;
			}
			this.MegafeedbackRoundLostWingColorAnimator.Color.a = (float)num;
			this.MegafeedbackRoundLostGameObject.SetActive(true);
			FMODAudioManager.PlayOneShotAt(this._roundLossSfx, Vector3.zero, 0);
		}

		private void OnPlayerHpChanged(float val)
		{
			if (val <= (float)this._LifePercentualForGlassShatter / 100f * (float)this.m_oPlayerCombatObject._data.HPMax)
			{
				if (!this.m_boPlayedGlassShatterAnimation)
				{
					this.MegafeedbackLowHpGlassShatterGameObject.SetActive(true);
					this.m_boPlayedGlassShatterAnimation = true;
				}
			}
			else
			{
				this.m_boPlayedGlassShatterAnimation = false;
			}
		}

		private void OnUltimateActivated(BaseFX baseFx)
		{
			this.MegafeedbackUltimateActivatedGameObject.SetActive(true);
		}

		private void SetSpeedometerAnimationValue(float value)
		{
			GameHubBehaviour.Hub.BombManager.DispatchReliable(new byte[0]).OnPlayerUpdatedGridProgress(GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerAddress, (int)value);
		}

		private void OnGridGameFinished(byte playerAddress, float finalValue)
		{
			if (playerAddress != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerAddress)
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.DispatchReliable(new byte[0]).OnPlayerUpdatedGridProgress(GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerAddress, (int)finalValue);
		}

		private void Update()
		{
			this.m_poDeathAnimation.Update();
			this.m_poCameraAnimation.UpdateCameraAnimation();
			this.m_poSpawnAnimation.Update();
			if (!this.m_oPlayerCombatObject || !this.m_oPlayerCombatObject.IsAlive() || GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreboardState.BombDelivery)
			{
				return;
			}
			this.UpdateCombatFeedbackWeakOrStrong();
			this.UpdateCombatFeedbackSilence();
			this.UpdateCombatFeedbackOverheat();
		}

		private void UpdateCombatFeedbackOverheat()
		{
			if (!this.m_oPlayerOverheatGadget || !this.MegafeedbackOverheatGameObject || !this.MegafeedbackOverheatAnimator)
			{
				return;
			}
			GadgetData.GadgetStateObject gadgetState = this.m_oPlayerCombatObject.GadgetStates.GetGadgetState(this.m_oPlayerOverheatGadget.Slot);
			float heat = gadgetState.Heat;
			bool flag = gadgetState.GadgetState == GadgetState.CoolingAfterOverheat;
			if (!this.MegafeedbackOverheatGameObject.activeSelf)
			{
				if ((double)heat < 0.75)
				{
					return;
				}
				this.MegafeedbackOverheatGameObject.SetActive(true);
			}
			this.MegafeedbackOverheatAnimator.SetBool("cooling", flag);
			this.MegafeedbackOverheatAnimator.SetFloat("currentHeat", heat);
		}

		private void UpdateCombatFeedbackSilence()
		{
			if (!this.MegafeedbackSilenceGameObject)
			{
				return;
			}
			bool flag = this.m_oPlayerCombatObject.Attributes.CurrentStatus.HasFlag(StatusKind.Disarmed);
			if (flag == this.MegafeedbackSilenceGameObject.activeSelf)
			{
				return;
			}
			this.ToggleEffect(this.MegafeedbackSilenceGameObject);
		}

		private void UpdateCombatFeedbackWeakOrStrong()
		{
			float hplightDamagePct = this.m_oPlayerCombatObject.Attributes.HPLightDamagePct;
			if (this.MegafeedbackWeakenGameObject && ((this.MegafeedbackWeakenGameObject.activeSelf && hplightDamagePct >= 0f) || (!this.MegafeedbackWeakenGameObject.activeSelf && hplightDamagePct < 0f)))
			{
				this.ToggleEffect(this.MegafeedbackWeakenGameObject);
			}
			if (this.MegafeedbackStrongGameObject && ((this.MegafeedbackStrongGameObject.activeSelf && hplightDamagePct <= 0f) || (!this.MegafeedbackStrongGameObject.activeSelf && hplightDamagePct > 0f)))
			{
				this.ToggleEffect(this.MegafeedbackStrongGameObject);
			}
		}

		private void ToggleEffect(GameObject effect)
		{
			if (!effect)
			{
				return;
			}
			Animator componentInChildren = effect.GetComponentInChildren<Animator>();
			if (effect.activeSelf)
			{
				componentInChildren.SetBool("active", false);
			}
			else
			{
				effect.SetActive(true);
				componentInChildren = effect.GetComponentInChildren<Animator>();
				componentInChildren.SetBool("active", true);
			}
		}

		private void TryToGiveLastGoalAchievements(bool isVictory)
		{
			if (SpectatorController.IsSpectating || GameHubBehaviour.Hub.Players.CurrentPlayerData.IsLeaver)
			{
				return;
			}
			MatchPlayerStatistics matchPlayerStatistics = new MatchPlayerStatistics();
			matchPlayerStatistics.Convert(this.GetCurrentPlayerStats(), isVictory);
			HudMegafeedbacksController.Log.Info("TryToGiveLastGoalAchievements: " + matchPlayerStatistics);
			ObservableExtensions.Subscribe<Unit>(this._incrementLastGoalAchievements.Increment(matchPlayerStatistics));
		}

		private PlayerStats GetCurrentPlayerStats()
		{
			return GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetComponent<PlayerStats>();
		}

		public void OnCleanup(CleanupMessage msg)
		{
			base.StopAllCoroutines();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudMegafeedbacksController));

		[Inject]
		private IPlayback _playback;

		[Inject]
		private IGamePostProcessing _postProcessing;

		[Inject]
		private IGameCamera _gameCamera;

		[Inject]
		private IGameCameraEngine _gameCameraEngine;

		[Inject]
		private IIncrementLastGoalAchievements _incrementLastGoalAchievements;

		[Inject]
		private ILogger<HudMegafeedbacksController> _logger;

		[Tooltip("The total duration of the animation")]
		public float RaceStartTotalAnimationTime = 8f;

		[Tooltip("When the Green light turn on in the animation")]
		public float AnimationTimeWhenGreenSeconds = 7f;

		public float RoundWinOrLostAnimationDelayInSec = 4f;

		public GameObject MegafeedbackRoundWinGameObject;

		public GameObject MegafeedbackRoundLostGameObject;

		public GameObject MegafeedbackDefeatGameObject;

		public GameObject MegafeedbackVictoryGameObject;

		public GameObject MegafeedbackLowHpGlassShatterGameObject;

		public GameObject MegafeedbackStrongCollisionGlassShatterGameObject;

		public GameObject MegafeedbackPlayerDeathObject;

		public GameObject MegafeedbackUltimateActivatedGameObject;

		public GameObject MegafeedbackBombAcquiredGameObject;

		public GameObject MegafeedbackBombLostGameObject;

		public GameObject MegafeedbackWeakenGameObject;

		public GameObject MegafeedbackStrongGameObject;

		public GameObject MegafeedbackOverheatGameObject;

		public GameObject MegafeedbackSilenceGameObject;

		public Animator MegafeedbackOverheatAnimator;

		public Animator MegafeedbackRaceStartAnimator;

		public UIWidget MegafeedbackRaceStartWidget;

		public HudFeedbackExitEvent BombWinEvent;

		public HudFeedbackExitEvent BombLostEvent;

		public HudFeedbackExitEvent RaceStartEvent;

		public HudFeedbackExitEvent DefeatEvent;

		public HudFeedbackExitEvent VictoryEvent;

		public HudFeedbackExitEvent LowHpGlassShatterEvent;

		public HudFeedbackExitEvent StrongCollisionHpGlassShatterEvent;

		public HudFeedbackExitEvent PlayerDeathEvent;

		public HudFeedbackExitEvent UltimateActivatedGameObjectEvent;

		public HudFeedbackExitEvent BombAquiredGameObjectEvent;

		public HudFeedbackExitEvent BombLostGameObjectEvent;

		public HudFeedbackExitEvent WeakenExitEvent;

		public HudFeedbackExitEvent StrongExitEvent;

		public HudFeedbackExitEvent OverheatExitEvent;

		public HudFeedbackExitEvent SilenceExitEvent;

		public int _LifePercentualForGlassShatter = 15;

		private bool m_boPlayedGlassShatterAnimation;

		private CombatObject m_oPlayerCombatObject;

		private GadgetBehaviour m_oPlayerOverheatGadget;

		public float _CollisionIntensityForGlassShatter = 40f;

		public float _CameraShakeStrenghtOnDeath = 1.5f;

		[Header("[Round Wings colors]")]
		public RendererMaterialColorAnimator MegafeedbackRoundWinWingColorAnimator;

		public RendererMaterialColorAnimator MegafeedbackRoundLostWingColorAnimator;

		[Header("Sound Effects")]
		[SerializeField]
		private AudioEventAsset _roundWinSfx;

		[SerializeField]
		private AudioEventAsset _roundLossSfx;

		[SerializeField]
		private AudioEventAsset _matchWinSfx;

		[SerializeField]
		private AudioEventAsset _matchLossSfx;

		private bool m_boIsCarryingBomb;

		private DeathAnimation m_poDeathAnimation;

		private DeathAnimation m_poSpawnAnimation;

		private CameraAnimation m_poCameraAnimation;

		private GameGui _gameGui;

		public AudioEventAsset raceStartAudio;

		private bool _speedometerListenerSetted;

		public List<DeathAnimation.CDeathAnimationState> m_DeathAnimationStates;

		public List<DeathAnimation.CDeathAnimationState> m_SpawnAnimationStates;

		public CameraAnimation.CCameraAnimationParameters m_DeathCameraAnimationParameters;

		private float _oldValue;

		public delegate void AnimationPlayingDelegate(bool isAnimationPlaying);
	}
}
