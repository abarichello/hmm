using System;
using FMod;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class VoiceOverController : AudioQueue
	{
		public override void Initialize(CarComponentHub carHub)
		{
			base.Initialize(carHub);
			this._carHub = carHub;
			this._objId = this._carHub.combatObject.Id.ObjId;
			this.CacheVoiceOver();
			this.ActivateListeners();
			base.SetVolumeByIdentifiable(this._carHub.combatObject.Id);
			this.SetDamageSFXToDefault();
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			if (this._carHub.combatObject.Id.IsOwner)
			{
				this._voiceOver.PreloadPlayer();
			}
			if (this._carHub.combatObject.Team == currentPlayerData.Team)
			{
				this._voiceOver.PreloadTeamMember();
			}
			this._voiceOver.Preload();
		}

		private void CacheVoiceOver()
		{
			this._voiceOver = this._carHub.Player.GetCharacterVoiceOver();
		}

		private void ActivateListeners()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombAlmostDeliveredTriggerEnter += this.OnBombAlmostDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged += this.OnBombChangeCarrier;
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop += this.OnBombDrop;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.ListenToPhaseChange;
			this._carHub.combatObject.OnRepairReceived += this.OnRepairReceived;
			this._carHub.combatObject.OnDamageReceived += this.OnDamageReceived;
			this._carHub.combatObject.ListenToObjectSpawn += this.ListenToObjectSpawn;
			this._carHub.combatObject.ListenToObjectUnspawn += this.ListenToObjectUnspawn;
			PauseController.OnInGamePauseStateChanged += this.OnInGamePauseStateChange;
		}

		private void DeactivateListeners()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombAlmostDeliveredTriggerEnter -= this.OnBombAlmostDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged -= this.OnBombChangeCarrier;
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop -= this.OnBombDrop;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.ListenToPhaseChange;
			PauseController.OnInGamePauseStateChanged -= this.OnInGamePauseStateChange;
			if (this._carHub == null)
			{
				return;
			}
			this._carHub.combatObject.OnRepairReceived -= this.OnRepairReceived;
			this._carHub.combatObject.OnDamageReceived -= this.OnDamageReceived;
			this._carHub.combatObject.ListenToObjectSpawn -= this.ListenToObjectSpawn;
			this._carHub.combatObject.ListenToObjectUnspawn -= this.ListenToObjectUnspawn;
		}

		protected override void Cleanup()
		{
			base.Cleanup();
			this.DeactivateListeners();
		}

		private void ListenToPhaseChange(BombScoreboardState state)
		{
			if (state == BombScoreboardState.PreReplay)
			{
				this.damageReceived = 0f;
				this.damageReceivedLastCount = -1f;
				return;
			}
			if (state == BombScoreboardState.Replay)
			{
				this.damageReceived = 0f;
				this.damageReceivedLastCount = -1f;
				base.StopAll();
			}
		}

		private void OnBombAlmostDelivery(TeamKind trackTeamKind)
		{
			if (GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this._carHub.combatObject) && trackTeamKind != this._carHub.combatObject.Team)
			{
				this.InternalPlayAudio(this._voiceOver.Bomb_Almost_Delivered, VoiceOverEventGroup.BombAlmostDelivered, this._objId, -1);
			}
		}

		private void OnBombChangeCarrier(CombatObject carrier)
		{
			if (carrier == null)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this._carHub.combatObject.Id.ObjId))
			{
				return;
			}
			this.InternalPlayAudio(this._voiceOver.Bomb_Pick, VoiceOverEventGroup.BombPickUp, this._objId, -1);
		}

		private void OnBombDrop(BombInstance bombInstance, SpawnReason reason, int causer)
		{
			if (causer != this._carHub.combatObject.Id.ObjId)
			{
				return;
			}
			if (reason != SpawnReason.TriggerDrop)
			{
				if (reason == SpawnReason.InputDrop)
				{
					this.InternalPlayAudio(this._voiceOver.Bomb_Drop_Purposeful, VoiceOverEventGroup.BombPurposefulDrop, this._objId, -1);
					return;
				}
				if (reason == SpawnReason.Death)
				{
					this.InternalPlayAudio(this._voiceOver.Bomb_Drop_Death, VoiceOverEventGroup.BombDeathDrop, this._objId, -1);
					return;
				}
				if (reason != SpawnReason.BrokenLink)
				{
					return;
				}
			}
			this.InternalPlayAudio(this._voiceOver.Bomb_Drop_Yellow, VoiceOverEventGroup.BombYellowDrop, this._objId, -1);
		}

		private void OnInGamePauseStateChange(PauseController.PauseState oldState, PauseController.PauseState newState, PlayerData playerData)
		{
			bool pause = newState == PauseController.PauseState.Paused || newState == PauseController.PauseState.UnpauseCountDown;
			base.PauseQueue(pause);
		}

		public void PlayGadgetAvailableVoiceOver(GadgetSlot slot)
		{
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				this.PlayGadgetAudio(this._voiceOver.Gadgets_Available_G00, VoiceOverEventGroup.GadgetAvailable);
				break;
			case GadgetSlot.CustomGadget1:
				this.PlayGadgetAudio(this._voiceOver.Gadgets_Available_G01, VoiceOverEventGroup.GadgetAvailable);
				break;
			case GadgetSlot.CustomGadget2:
				this.PlayGadgetAudio(this._voiceOver.Gadgets_Available_Ult, VoiceOverEventGroup.GadgetAvailableUltimate);
				break;
			case GadgetSlot.BoostGadget:
				this.PlayGadgetAudio(this._voiceOver.Gadgets_Available_Nitro, VoiceOverEventGroup.GadgetAvailable);
				break;
			}
		}

		public void PlayGadgetMissVoiceOver(GadgetSlot slot)
		{
			if (slot != GadgetSlot.CustomGadget0)
			{
				if (slot != GadgetSlot.CustomGadget1)
				{
					if (slot == GadgetSlot.CustomGadget2)
					{
						this.PlayGadgetAudio(this._voiceOver.Gadgets_Miss_Ult, VoiceOverEventGroup.GadgetHitOrMissUltimate);
					}
				}
				else
				{
					this.PlayGadgetAudio(this._voiceOver.Gadgets_Miss_G01, VoiceOverEventGroup.GadgetHitOrMiss);
				}
			}
			else
			{
				this.PlayGadgetAudio(this._voiceOver.Gadgets_Miss_G00, VoiceOverEventGroup.GadgetHitOrMiss);
			}
		}

		public void PlayGadgetUse(GadgetSlot slot)
		{
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				this.PlayGadgetAudio(this._voiceOver.Gadgets_Use_G00, VoiceOverEventGroup.GadgetUse);
				return;
			case GadgetSlot.CustomGadget1:
				this.PlayGadgetAudio(this._voiceOver.Gadgets_Use_G01, VoiceOverEventGroup.GadgetUse);
				return;
			case GadgetSlot.CustomGadget2:
				this.PlayGadgetAudio(this._voiceOver.Gadgets_Use_Ult, VoiceOverEventGroup.GadgetUseUltimate);
				return;
			default:
				if (slot != GadgetSlot.BombGadget)
				{
					return;
				}
				this.PlayGadgetAudio(this._voiceOver.QuickChat_GiveMe_Bomb, VoiceOverEventGroup.BombGadgetUse);
				return;
			}
		}

		public void PlayGadgetCooldown(GadgetSlot slot)
		{
			if (slot == GadgetSlot.CustomGadget0)
			{
				this.PlayGadgetAudio(this._voiceOver.Gadgets_CD_G00, VoiceOverEventGroup.GadgetCooldown);
				return;
			}
			if (slot == GadgetSlot.CustomGadget1)
			{
				this.PlayGadgetAudio(this._voiceOver.Gadgets_CD_G01, VoiceOverEventGroup.GadgetCooldown);
				return;
			}
			if (slot != GadgetSlot.CustomGadget2)
			{
				return;
			}
			this.PlayGadgetAudio(this._voiceOver.Gadgets_CD_Ult, VoiceOverEventGroup.GadgetCooldown);
		}

		public void PlayDisarmedVoiceOver()
		{
			this.PlayGadgetAudio(this._voiceOver.Disarmed, VoiceOverEventGroup.Disarmed);
		}

		private void PlayGadgetAudio(VoiceOverLine asset, VoiceOverEventGroup group)
		{
			if (asset.VoiceLine == null)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.Global.LockAllPlayers)
			{
				BombScoreboardState currentBombGameState = GameHubBehaviour.Hub.BombManager.CurrentBombGameState;
				if (currentBombGameState == BombScoreboardState.BombDelivery || currentBombGameState == BombScoreboardState.PreReplay)
				{
					this.InternalPlayAudio(asset, group, this._objId, -1);
				}
			}
		}

		private void OnRepairReceived(float amount, int otherId)
		{
			if (this._carHub.combatObject.Data.HP >= (float)this._carHub.combatObject.Data.HPMax * GameHubBehaviour.Hub.AudioSettings.NearDeathThreshold)
			{
				this.lowHealthCanTrigger = true;
			}
		}

		private void OnDamageReceived(float ammount, int otherId)
		{
			CombatObject combatObject = this._carHub.combatObject;
			bool flag = combatObject.Id.ObjId == otherId;
			if (!flag)
			{
				if (this.damageReceivedLastCount < Time.timeSinceLevelLoad)
				{
					this.damageReceived = 0f;
					this.damageReceivedLastCount = Time.timeSinceLevelLoad + GameHubBehaviour.Hub.AudioSettings.MassiveDamageCheckInterval;
				}
				this.damageReceived += ammount;
			}
			if (combatObject.Data.HP < (float)combatObject.Data.HPMax * GameHubBehaviour.Hub.AudioSettings.NearDeathThreshold)
			{
				if (this.lowHealthCanTrigger)
				{
					this.lowHealthCanTrigger = false;
					if (this._voiceOver.Bomb_Near_Death.VoiceLine != null && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(combatObject))
					{
						this.InternalPlayAudio(this._voiceOver.Bomb_Near_Death, VoiceOverEventGroup.NearDeath, otherId, this._objId);
						return;
					}
					this.InternalPlayAudio(this._voiceOver.Almost_Dying, VoiceOverEventGroup.NearDeath, otherId, this._objId);
					return;
				}
			}
			else
			{
				this.lowHealthCanTrigger = true;
			}
			if (flag)
			{
				return;
			}
			VoiceOverLine asset;
			VoiceOverEventGroup voGroup;
			if (this.damageReceived > (float)combatObject.Data.HPMax * GameHubBehaviour.Hub.AudioSettings.MassiveDamageThreshold)
			{
				this.damageReceived = 0f;
				this.damageReceivedLastCount = -1f;
				asset = this._voiceOver.Damage_Massive;
				voGroup = VoiceOverEventGroup.MassiveDamage;
			}
			else
			{
				if (ammount <= GameHubBehaviour.Hub.AudioSettings.IgnoreAudioDamageThreshold)
				{
					return;
				}
				this.damageReceivedLastCount = Time.timeSinceLevelLoad + GameHubBehaviour.Hub.AudioSettings.MassiveDamageCheckInterval;
				asset = this._voiceOver.Damage_Regular;
				voGroup = VoiceOverEventGroup.Damage;
			}
			if (this.InternalPlayAudio(asset, voGroup, otherId, this._objId) && otherId != -1)
			{
				FMODAudioManager.PlayOneShotAt(this.currentDamageSFXAsset, base.transform.position, 0);
			}
		}

		public void ChangeDamageSFX(AudioEventAsset damageSFX)
		{
			this.currentDamageSFXAsset = damageSFX;
		}

		public void SetDamageSFXToDefault()
		{
			CarAudioData characterCarAudioData = this._carHub.Player.GetCharacterCarAudioData();
			this.currentDamageSFXAsset = ((!this._carHub.combatObject.Id.IsOwner) ? characterCarAudioData.Hitted_Others : characterCarAudioData.Hitted_Player);
		}

		private void ListenToObjectSpawn(CombatObject obj, SpawnEvent msg)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery)
			{
				this.InternalPlayAudio(this._voiceOver.Respawn, VoiceOverEventGroup.Respawn, this._objId, -1);
			}
		}

		private void ListenToObjectUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this.InternalPlayAudio((!this._carHub.combatObject.Id.IsOwner) ? this._voiceOver.Dying_Others : this._voiceOver.Dying_Client, VoiceOverEventGroup.Death, msg.Causer, this._objId);
		}

		public void Play(VoiceOverEventGroup voiceOverEventGroup)
		{
			switch (voiceOverEventGroup)
			{
			case VoiceOverEventGroup.QuickChatAttackInterceptors:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Attack_Interceptors, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatAttackSupporters:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Attack_Supporters, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatAttackTransporters:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Attack_Transporters, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatGroupUp:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Group_Up, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatNeedRepair:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Need_Repair, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatNeedRepairIntense:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Need_Repair_Intense, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatOk:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Ok, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatSpecialAlmostReady:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Special_Almost_Ready, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatSpecialNotReady:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Special_Not_Ready, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatSpecialReady:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Special_Ready, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatGiveMeBomb:
				this.InternalPlayAudio(this._voiceOver.QuickChat_GiveMe_Bomb, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatDroppingBomb:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Dropping_Bomb, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatProtectBomb:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Protect_Bomb, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatGetBomb:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Get_Bomb, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatOnMyWay:
				this.InternalPlayAudio(this._voiceOver.QuickChat_OnMyWay, voiceOverEventGroup, this._objId, -1);
				return;
			case VoiceOverEventGroup.QuickChatImOut:
				this.InternalPlayAudio(this._voiceOver.QuickChat_ImOut, voiceOverEventGroup, this._objId, -1);
				return;
			default:
				switch (voiceOverEventGroup)
				{
				case VoiceOverEventGroup.BombDelivered:
					this.InternalPlayAudio(this._voiceOver.Bomb_Delivered, voiceOverEventGroup, this._objId, -1);
					return;
				default:
					switch (voiceOverEventGroup)
					{
					case VoiceOverEventGroup.MatchWin:
						if (!this._carHub.combatObject.Id.IsOwner)
						{
							this.InternalPlayAudio(this._voiceOver.Match_Win, voiceOverEventGroup, this._objId, -1);
						}
						return;
					case VoiceOverEventGroup.MatchLose:
						this.InternalPlayAudio(this._voiceOver.Match_Lose, voiceOverEventGroup, this._objId, -1);
						return;
					case VoiceOverEventGroup.ActivateNitro:
						this.InternalPlayAudio(this._voiceOver.Movement_Nitro, voiceOverEventGroup, this._objId, -1);
						break;
					default:
						if (voiceOverEventGroup == VoiceOverEventGroup.Respawn)
						{
							this.InternalPlayAudio(this._voiceOver.Respawn, voiceOverEventGroup, this._objId, -1);
							return;
						}
						Debug.Assert(false, string.Format("Not implemented VoiceOverEventGroup by play: {0}", voiceOverEventGroup), Debug.TargetTeam.All);
						break;
					}
					return;
				case VoiceOverEventGroup.OpenShop:
					this.InternalPlayAudio(this._voiceOver.Upgrade_Open_Shop, voiceOverEventGroup, this._objId, -1);
					return;
				case VoiceOverEventGroup.BuyUpgrades:
					this.InternalPlayAudio(this._voiceOver.Upgrade_Buy, voiceOverEventGroup, this._objId, -1);
					return;
				case VoiceOverEventGroup.BuyUltimateUpgrade:
					this.InternalPlayAudio(this._voiceOver.Upgrade_Ult_Buy, voiceOverEventGroup, this._objId, -1);
					return;
				}
				break;
			}
		}

		public void PlayPing(PlayerPing.PlayerPingKind pingKind)
		{
			switch (pingKind)
			{
			case PlayerPing.PlayerPingKind.ProtectTheBomb:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Protect_Bomb, VoiceOverEventGroup.ChatProtectBomb, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.GoodGame:
				this.InternalPlayAudio(this._voiceOver.Talk_GoodGame, VoiceOverEventGroup.ChatGG, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.OnMyWay:
				this.InternalPlayAudio(this._voiceOver.QuickChat_OnMyWay, VoiceOverEventGroup.ChatOMW, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.Thanks:
				this.InternalPlayAudio(this._voiceOver.Talk_Thanks, VoiceOverEventGroup.ChatThanks, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.CountMeOut:
				this.InternalPlayAudio(this._voiceOver.QuickChat_ImOut, VoiceOverEventGroup.ChatCountMeOut, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.GoodLuckHaveFun:
				this.InternalPlayAudio(this._voiceOver.Talk_GoodLuck, VoiceOverEventGroup.ChatGL, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.LetMeGetThebomb:
				this.InternalPlayAudio(this._voiceOver.QuickChat_GiveMe_Bomb, VoiceOverEventGroup.ChatLetMeGetThebomb, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.Sorry:
				this.InternalPlayAudio(this._voiceOver.Talk_Sorry, VoiceOverEventGroup.ChatSorry, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.GetTheBomb:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Get_Bomb, VoiceOverEventGroup.ChatGetBomb, this._objId, -1);
				break;
			case PlayerPing.PlayerPingKind.IWillDropTheBomb:
				this.InternalPlayAudio(this._voiceOver.QuickChat_Dropping_Bomb, VoiceOverEventGroup.ChatDropBomb, this._objId, -1);
				break;
			}
		}

		public void TriggerMatchEndAudio()
		{
			if (!this._carHub.combatObject.IsLocalPlayer)
			{
				return;
			}
			TeamKind teamKind = TeamKind.Zero;
			if (GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchOverBluWins)
			{
				teamKind = TeamKind.Blue;
			}
			else if (GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchOverRedWins)
			{
				teamKind = TeamKind.Red;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind)
			{
				this.InternalPlayAudio(this._voiceOver.Match_Win, VoiceOverEventGroup.MatchWin, this._objId, -1);
			}
			else
			{
				this.InternalPlayAudio(this._voiceOver.Match_Lose, VoiceOverEventGroup.MatchLose, this._objId, -1);
			}
		}

		private bool IsVoGroupPlayDeadException(VoiceOverEventGroup voGroup)
		{
			switch (voGroup)
			{
			case VoiceOverEventGroup.BombDeathDrop:
			case VoiceOverEventGroup.BombDelivered:
			case VoiceOverEventGroup.OpenShop:
			case VoiceOverEventGroup.BuyUpgrades:
			case VoiceOverEventGroup.BuyUltimateUpgrade:
				break;
			default:
				switch (voGroup)
				{
				case VoiceOverEventGroup.MassiveDamage:
				case VoiceOverEventGroup.Death:
				case VoiceOverEventGroup.Respawn:
					return true;
				}
				return false;
			}
			return true;
		}

		private FMODAudioManager.SourceTypes GetVoiceOverEventType(VoiceOverEventGroup group)
		{
			if (group >= (VoiceOverEventGroup)this._audioSettings.VoiceOverEventsConfig.Length)
			{
				return (FMODAudioManager.SourceTypes)0;
			}
			return this._audioSettings.VoiceOverEventsConfig[(int)group];
		}

		public void PlayKillAudio(QueuedAnnouncerLog queuedAnnouncerLog)
		{
			if (this._voiceOver.Kill_Ultimate.VoiceLine != null && this._carHub.combatObject.GadgetStates.G2StateObject.EffectState == EffectState.Running)
			{
				this.InternalPlayAudio(this._voiceOver.Kill_Ultimate, VoiceOverEventGroup.KillEnemyPlayer, this._objId, queuedAnnouncerLog.AnnouncerEvent.Victim);
				return;
			}
			this.InternalPlayAudio(this._voiceOver.Kill_Enemy, VoiceOverEventGroup.KillEnemyPlayer, this._objId, queuedAnnouncerLog.AnnouncerEvent.Victim);
		}

		public void PlayKillAssistAudio(QueuedAnnouncerLog queuedAnnouncerLog)
		{
			this.InternalPlayAudio(this._voiceOver.Kill_Enemy, VoiceOverEventGroup.KillEnemyPlayer, this._objId, queuedAnnouncerLog.AnnouncerEvent.Victim);
		}

		public void PlayRevengeAudio(QueuedAnnouncerLog queuedAnnouncerLog)
		{
			this.InternalPlayAudio(this._voiceOver.Kill_Revenge, VoiceOverEventGroup.KillNemesisRevenge, this._objId, queuedAnnouncerLog.AnnouncerEvent.Victim);
		}

		private bool InternalPlayAudio(VoiceOverLine asset, VoiceOverEventGroup voGroup, int causerID, int targetId = -1)
		{
			if (this._carHub.combatObject.IsLocalPlayer && this._carHub.combatObject.Player.IsBotControlled)
			{
				return false;
			}
			if (asset.VoiceLine == null)
			{
				return false;
			}
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial() && !asset.PlayOnTutorial)
			{
				return false;
			}
			if (!this._carHub.combatObject.IsAlive() && !this.IsVoGroupPlayDeadException(voGroup))
			{
				return false;
			}
			FMODAudioManager.SourceTypes voiceOverEventType = this.GetVoiceOverEventType(voGroup);
			if (FMODAudioManager.CheckForbiddenSources(GameHubBehaviour.Hub.Players, voiceOverEventType, this._objId, causerID, targetId))
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
				{
					VoiceOverController.Log.DebugFormat("[AUDIO] [CAR] {0} tried to call {1}, but he is forbidden by config {2}", new object[]
					{
						base.name,
						asset.VoiceLine.name,
						voiceOverEventType
					});
				}
				return false;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
			{
				VoiceOverController.Log.DebugFormat("[AUDIO] [CAR] {0} calling {1}", new object[]
				{
					base.name,
					asset.VoiceLine.name
				});
			}
			base.Enqueue(asset.VoiceLine, this._carHub.RenderTransform);
			return true;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(VoiceOverController));

		private CarComponentHub _carHub;

		private float damageReceived;

		private float damageReceivedLastCount = -1f;

		private bool lowHealthCanTrigger = true;

		private GadgetState LastGadget0State = GadgetState.None;

		private GadgetState LastGadget1State = GadgetState.None;

		private GadgetState LastGadget2State = GadgetState.None;

		private GadgetState LastBombGadgetState = GadgetState.None;

		private AudioEventAsset currentDamageSFXAsset;

		private VoiceOver _voiceOver;

		private int _objId;
	}
}
