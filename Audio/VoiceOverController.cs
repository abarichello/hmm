using System;
using FMod;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class VoiceOverController : AudioQueue
	{
		private VoiceOver VoiceOver
		{
			get
			{
				return this._carHub.Player.Character.voiceOver;
			}
		}

		private int ObjId
		{
			get
			{
				return this._carHub.combatObject.Id.ObjId;
			}
		}

		public override void Initialize(CarComponentHub carHub)
		{
			base.Initialize(carHub);
			this._carHub = carHub;
			this.ActivateListeners();
			base.SetVolumeByIdentifiable(this._carHub.combatObject.Id);
			this.SetDamageSFXToDefault();
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			if (this._carHub.combatObject.Id.IsOwner)
			{
				this.VoiceOver.PreloadPlayer();
			}
			if (this._carHub.combatObject.Team == currentPlayerData.Team)
			{
				this.VoiceOver.PreloadTeamMember();
			}
			this.VoiceOver.Preload();
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

		private void ListenToPhaseChange(BombScoreBoard.State state)
		{
			if (state == BombScoreBoard.State.PreReplay)
			{
				this.damageReceived = 0f;
				this.damageReceivedLastCount = -1f;
				return;
			}
			if (state == BombScoreBoard.State.Replay)
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
				this.InternalPlayAudio(this.VoiceOver.Bomb_Almost_Delivered, VoiceOverEventGroup.BombAlmostDelivered, this.ObjId, -1);
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
			this.InternalPlayAudio(this.VoiceOver.Bomb_Pick, VoiceOverEventGroup.BombPickUp, this.ObjId, -1);
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
					this.InternalPlayAudio(this.VoiceOver.Bomb_Drop_Purposeful, VoiceOverEventGroup.BombPurposefulDrop, this.ObjId, -1);
					return;
				}
				if (reason == SpawnReason.Death)
				{
					this.InternalPlayAudio(this.VoiceOver.Bomb_Drop_Death, VoiceOverEventGroup.BombDeathDrop, this.ObjId, -1);
					return;
				}
				if (reason != SpawnReason.BrokenLink)
				{
					return;
				}
			}
			this.InternalPlayAudio(this.VoiceOver.Bomb_Drop_Yellow, VoiceOverEventGroup.BombYellowDrop, this.ObjId, -1);
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
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_Available_G00, VoiceOverEventGroup.GadgetAvailable);
				break;
			case GadgetSlot.CustomGadget1:
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_Available_G01, VoiceOverEventGroup.GadgetAvailable);
				break;
			case GadgetSlot.CustomGadget2:
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_Available_Ult, VoiceOverEventGroup.GadgetAvailableUltimate);
				break;
			case GadgetSlot.BoostGadget:
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_Available_Nitro, VoiceOverEventGroup.GadgetAvailable);
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
						this.PlayGadgetAudio(this.VoiceOver.Gadgets_Miss_Ult, VoiceOverEventGroup.GadgetHitOrMissUltimate);
					}
				}
				else
				{
					this.PlayGadgetAudio(this.VoiceOver.Gadgets_Miss_G01, VoiceOverEventGroup.GadgetHitOrMiss);
				}
			}
			else
			{
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_Miss_G00, VoiceOverEventGroup.GadgetHitOrMiss);
			}
		}

		public void PlayGadgetUse(GadgetSlot slot)
		{
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_Use_G00, VoiceOverEventGroup.GadgetUse);
				return;
			case GadgetSlot.CustomGadget1:
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_Use_G01, VoiceOverEventGroup.GadgetUse);
				return;
			case GadgetSlot.CustomGadget2:
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_Use_Ult, VoiceOverEventGroup.GadgetUseUltimate);
				return;
			default:
				if (slot != GadgetSlot.BombGadget)
				{
					return;
				}
				this.PlayGadgetAudio(this.VoiceOver.QuickChat_GiveMe_Bomb, VoiceOverEventGroup.BombGadgetUse);
				return;
			}
		}

		public void PlayGadgetCooldown(GadgetSlot slot)
		{
			if (slot == GadgetSlot.CustomGadget0)
			{
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_CD_G00, VoiceOverEventGroup.GadgetCooldown);
				return;
			}
			if (slot == GadgetSlot.CustomGadget1)
			{
				this.PlayGadgetAudio(this.VoiceOver.Gadgets_CD_G01, VoiceOverEventGroup.GadgetCooldown);
				return;
			}
			if (slot != GadgetSlot.CustomGadget2)
			{
				return;
			}
			this.PlayGadgetAudio(this.VoiceOver.Gadgets_CD_Ult, VoiceOverEventGroup.GadgetCooldown);
		}

		public void PlayDisarmedVoiceOver()
		{
			this.PlayGadgetAudio(this.VoiceOver.Disarmed, VoiceOverEventGroup.Disarmed);
		}

		private void PlayGadgetAudio(VoiceOverLine asset, VoiceOverEventGroup group)
		{
			if (asset.VoiceLine == null)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.Global.LockAllPlayers)
			{
				BombScoreBoard.State currentBombGameState = GameHubBehaviour.Hub.BombManager.CurrentBombGameState;
				if (currentBombGameState == BombScoreBoard.State.BombDelivery || currentBombGameState == BombScoreBoard.State.PreReplay)
				{
					this.InternalPlayAudio(asset, group, this.ObjId, -1);
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
					if (this.VoiceOver.Bomb_Near_Death.VoiceLine != null && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(combatObject))
					{
						this.InternalPlayAudio(this.VoiceOver.Bomb_Near_Death, VoiceOverEventGroup.NearDeath, otherId, this.ObjId);
						return;
					}
					this.InternalPlayAudio(this.VoiceOver.Almost_Dying, VoiceOverEventGroup.NearDeath, otherId, this.ObjId);
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
				asset = this.VoiceOver.Damage_Massive;
				voGroup = VoiceOverEventGroup.MassiveDamage;
			}
			else
			{
				if (ammount <= GameHubBehaviour.Hub.AudioSettings.IgnoreAudioDamageThreshold)
				{
					return;
				}
				this.damageReceivedLastCount = Time.timeSinceLevelLoad + GameHubBehaviour.Hub.AudioSettings.MassiveDamageCheckInterval;
				asset = this.VoiceOver.Damage_Regular;
				voGroup = VoiceOverEventGroup.Damage;
			}
			if (this.InternalPlayAudio(asset, voGroup, otherId, this.ObjId) && otherId != -1)
			{
				FMODAudioManager.PlayOneShotAt(this.currentDamageSFXAsset, base.transform.position, 0);
			}
		}

		public void ChangeDamageSFX(FMODAsset damageSFX)
		{
			this.currentDamageSFXAsset = damageSFX;
		}

		public void SetDamageSFXToDefault()
		{
			this.currentDamageSFXAsset = ((!this._carHub.combatObject.Id.IsOwner) ? this._carHub.Player.Character.carAudio.Hitted_Others : this._carHub.Player.Character.carAudio.Hitted_Player);
		}

		private void ListenToObjectSpawn(CombatObject obj, SpawnEvent msg)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery)
			{
				this.InternalPlayAudio(this.VoiceOver.Respawn, VoiceOverEventGroup.Respawn, this.ObjId, -1);
			}
		}

		private void ListenToObjectUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this.InternalPlayAudio((!this._carHub.combatObject.Id.IsOwner) ? this.VoiceOver.Dying_Others : this.VoiceOver.Dying_Client, VoiceOverEventGroup.Death, msg.Causer, this.ObjId);
		}

		public void Play(VoiceOverEventGroup voiceOverEventGroup)
		{
			switch (voiceOverEventGroup)
			{
			case VoiceOverEventGroup.BombDelivered:
				this.InternalPlayAudio(this.VoiceOver.Bomb_Delivered, voiceOverEventGroup, this.ObjId, -1);
				return;
			default:
				switch (voiceOverEventGroup)
				{
				case VoiceOverEventGroup.MatchWin:
					if (!this._carHub.combatObject.Id.IsOwner)
					{
						this.InternalPlayAudio(this.VoiceOver.Match_Win, voiceOverEventGroup, this.ObjId, -1);
					}
					return;
				case VoiceOverEventGroup.MatchLose:
					this.InternalPlayAudio(this.VoiceOver.Match_Lose, voiceOverEventGroup, this.ObjId, -1);
					return;
				case VoiceOverEventGroup.ActivateNitro:
					this.InternalPlayAudio(this.VoiceOver.Movement_Nitro, voiceOverEventGroup, this.ObjId, -1);
					break;
				default:
					if (voiceOverEventGroup == VoiceOverEventGroup.Respawn)
					{
						this.InternalPlayAudio(this.VoiceOver.Respawn, voiceOverEventGroup, this.ObjId, -1);
						return;
					}
					HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("Not implemented VoiceOverEventGroup by play: {0}", voiceOverEventGroup), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
					break;
				}
				return;
			case VoiceOverEventGroup.OpenShop:
				this.InternalPlayAudio(this.VoiceOver.Upgrade_Open_Shop, voiceOverEventGroup, this.ObjId, -1);
				return;
			case VoiceOverEventGroup.BuyUpgrades:
				this.InternalPlayAudio(this.VoiceOver.Upgrade_Buy, voiceOverEventGroup, this.ObjId, -1);
				return;
			case VoiceOverEventGroup.BuyUltimateUpgrade:
				this.InternalPlayAudio(this.VoiceOver.Upgrade_Ult_Buy, voiceOverEventGroup, this.ObjId, -1);
				return;
			}
		}

		public void PlayPing(PlayerPing.PlayerPingKind pingKind)
		{
			switch (pingKind)
			{
			case PlayerPing.PlayerPingKind.ProtectTheBomb:
				this.InternalPlayAudio(this.VoiceOver.QuickChat_Protect_Bomb, VoiceOverEventGroup.ChatProtectBomb, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.GoodGame:
				this.InternalPlayAudio(this.VoiceOver.Talk_GoodGame, VoiceOverEventGroup.ChatGG, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.OnMyWay:
				this.InternalPlayAudio(this.VoiceOver.QuickChat_OnMyWay, VoiceOverEventGroup.ChatOMW, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.Thanks:
				this.InternalPlayAudio(this.VoiceOver.Talk_Thanks, VoiceOverEventGroup.ChatThanks, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.CountMeOut:
				this.InternalPlayAudio(this.VoiceOver.QuickChat_ImOut, VoiceOverEventGroup.ChatCountMeOut, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.GoodLuckHaveFun:
				this.InternalPlayAudio(this.VoiceOver.Talk_GoodLuck, VoiceOverEventGroup.ChatGL, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.LetMeGetThebomb:
				this.InternalPlayAudio(this.VoiceOver.QuickChat_GiveMe_Bomb, VoiceOverEventGroup.ChatLetMeGetThebomb, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.Sorry:
				this.InternalPlayAudio(this.VoiceOver.Talk_Sorry, VoiceOverEventGroup.ChatSorry, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.GetTheBomb:
				this.InternalPlayAudio(this.VoiceOver.QuickChat_Get_Bomb, VoiceOverEventGroup.ChatGetBomb, this.ObjId, -1);
				break;
			case PlayerPing.PlayerPingKind.IWillDropTheBomb:
				this.InternalPlayAudio(this.VoiceOver.QuickChat_Dropping_Bomb, VoiceOverEventGroup.ChatDropBomb, this.ObjId, -1);
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
				this.InternalPlayAudio(this.VoiceOver.Match_Win, VoiceOverEventGroup.MatchWin, this.ObjId, -1);
			}
			else
			{
				this.InternalPlayAudio(this.VoiceOver.Match_Lose, VoiceOverEventGroup.MatchLose, this.ObjId, -1);
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
			if (group > (VoiceOverEventGroup)this._audioSettings.VoiceOverEventsConfig.Length)
			{
				return (FMODAudioManager.SourceTypes)0;
			}
			return this._audioSettings.VoiceOverEventsConfig[(int)group];
		}

		public void PlayKillAudio(AnnouncerManager.QueuedAnnouncerLog queuedAnnouncerLog)
		{
			if (this.VoiceOver.Kill_Ultimate.VoiceLine != null && this._carHub.combatObject.GadgetStates.G2StateObject.EffectState == EffectState.Running)
			{
				this.InternalPlayAudio(this.VoiceOver.Kill_Ultimate, VoiceOverEventGroup.KillEnemyPlayer, this.ObjId, queuedAnnouncerLog.AnnouncerEvent.Victim);
				return;
			}
			this.InternalPlayAudio(this.VoiceOver.Kill_Enemy, VoiceOverEventGroup.KillEnemyPlayer, this.ObjId, queuedAnnouncerLog.AnnouncerEvent.Victim);
		}

		public void PlayKillAssistAudio(AnnouncerManager.QueuedAnnouncerLog queuedAnnouncerLog)
		{
			this.InternalPlayAudio(this.VoiceOver.Kill_Enemy, VoiceOverEventGroup.KillEnemyPlayer, this.ObjId, queuedAnnouncerLog.AnnouncerEvent.Victim);
		}

		public void PlayRevengeAudio(AnnouncerManager.QueuedAnnouncerLog queuedAnnouncerLog)
		{
			this.InternalPlayAudio(this.VoiceOver.Kill_Revenge, VoiceOverEventGroup.KillNemesisRevenge, this.ObjId, queuedAnnouncerLog.AnnouncerEvent.Victim);
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
			if (FMODAudioManager.CheckForbiddenSources(GameHubBehaviour.Hub, voiceOverEventType, this.ObjId, causerID, targetId))
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
				{
				}
				return false;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
			{
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

		private FMODAsset currentDamageSFXAsset;
	}
}
