using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudPlayersObject : GameHubBehaviour
	{
		private void Start()
		{
			SpectatorController.EvtWatchCar += this.OnWatchCar_UpdadeFeedbackIcon;
			this._position = this.ThumbGrey.transform.localPosition;
		}

		public void Setup(PlayerStats playerScrapData, CombatObject combatObject, SpawnController spawnController, string userId)
		{
			this._playerScrapData = playerScrapData;
			this._combatObject = combatObject;
			PlayerData player = combatObject.Player;
			this._botControlledGameObject.SetActive(!player.IsBot && (player.IsBotControlled || !player.Connected));
			HudPlayersObject.Log.DebugFormat("Setup players object. Name:{0} IsControlled:{1}, isConnected:{2}", new object[]
			{
				player.Name,
				player.IsBotControlled,
				player.Connected
			});
			this._combatObjectId = this._combatObject.Id.ObjId;
			this._combatObject.ListenToObjectSpawn += this.OnCombatObjecSpawn;
			this._combatObject.ListenToObjectUnspawn += this.OnCombatObjecUnspawn;
			this.RespawnController.Configure(this._combatObject);
			if (!SpectatorController.IsSpectating)
			{
				this._portraitSpectatorButton.enabled = false;
			}
			GameHubBehaviour.Hub.Announcer.ListenToEvent += this.ListenToAnnouncerEvent;
			this._voiceChatStatus.Setup(player.ConvertToPlayer(), player.IsBot, player.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team);
		}

		private void OnCombatObjecSpawn(CombatObject combatObject, SpawnEvent msg)
		{
			this.ThumbConfig(true);
		}

		private void OnCombatObjecUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this.ThumbConfig(false);
			if (this._combatObject.IsBot && GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void ThumbConfig(bool isAlive)
		{
			this.ThumbGrey.transform.localPosition = (isAlive ? (Vector3.up * 10000f) : this._position);
			this.Thumb.alpha = ((!isAlive) ? 0.5f : 1f);
		}

		public void Update()
		{
			if (this._playerScrapData != null)
			{
				this.Disconnected.SetActive(this._playerScrapData.Disconnected);
				this._botControlledGameObject.SetActive(!this._playerScrapData.Combat.Player.IsBot && this._playerScrapData.Combat.Player.IsBotControlled);
			}
			this.BombAnimator.SetBool("active", GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this._combatObjectId));
		}

		public void ShowPlayerNameLabel()
		{
			this.PlayerNameGO.SetActive(true);
		}

		public void HidePlayerNameLabel()
		{
			this.PlayerNameGO.SetActive(false);
		}

		public void SetTeamColor(bool isCurrentPlayer, bool isSameTeam, bool isRightSide)
		{
			Color color;
			if (isCurrentPlayer)
			{
				color = this.CurrentPlayerColor;
			}
			else
			{
				color = ((!isSameTeam) ? Color.red : Color.cyan);
			}
			this.BorderPortraitUi2DSprite.color = color;
			this.BombSprite.color = color;
		}

		public void SetBotControlledTeamIconColor(Color spriteColor)
		{
			this._botControlledSprite.color = spriteColor;
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.Announcer.ListenToEvent -= this.ListenToAnnouncerEvent;
			}
			if (this._combatObject != null)
			{
				this._combatObject.ListenToObjectSpawn -= this.OnCombatObjecSpawn;
				this._combatObject.ListenToObjectUnspawn -= this.OnCombatObjecUnspawn;
			}
			SpectatorController.EvtWatchCar -= this.OnWatchCar_UpdadeFeedbackIcon;
		}

		private void ListenToAnnouncerEvent(QueuedAnnouncerLog queuedAnnouncerLog)
		{
			if (queuedAnnouncerLog.AnnouncerEvent.AnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.BotControllerActivated && queuedAnnouncerLog.AnnouncerEvent.AnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.BotControllerDeactivated)
			{
				return;
			}
			if (queuedAnnouncerLog.AnnouncerEvent.Killer != this._combatObjectId)
			{
				return;
			}
			this._botControlledGameObject.SetActive(queuedAnnouncerLog.AnnouncerEvent.AnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.BotControllerActivated);
		}

		private void OnWatchCar_UpdadeFeedbackIcon(CombatObject combatObject)
		{
			if (combatObject != null && combatObject == this._combatObject)
			{
				this._isBeeingWatchedIconIsActive = true;
				this._isBeingWatchedIconAnimator.ResetTrigger("Exit");
				this._isBeingWatchedIconAnimator.SetTrigger("Select");
				return;
			}
			if (!this._isBeeingWatchedIconIsActive)
			{
				return;
			}
			this._isBeeingWatchedIconIsActive = false;
			this._isBeingWatchedIconAnimator.ResetTrigger("Select");
			this._isBeingWatchedIconAnimator.SetTrigger("Exit");
		}

		public void onButtonClick_Spectator_SelectCar()
		{
			if (!SpectatorController.IsSpectating)
			{
				return;
			}
			SingletonMonoBehaviour<SpectatorController>.Instance.SelectTargetCombatObject(this._combatObject, true, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudPlayersObject));

		private const string BombAnimatorActiveProperty = "active";

		public Color CurrentPlayerColor = default(Color);

		public HudRespawnController RespawnController;

		public HMMUI2DDynamicSprite Thumb;

		public UI2DSprite ThumbGrey;

		public GameObject Disconnected;

		public UILabel PlayerNameLabel;

		public GameObject PlayerNameGO;

		public UI2DSprite BorderPortraitUi2DSprite;

		public UI2DSprite BombSprite;

		public Animator BombAnimator;

		private PlayerStats _playerScrapData;

		private CombatObject _combatObject;

		[SerializeField]
		private VoiceChatStatusChangerGUIButton _voiceChatStatus;

		[SerializeField]
		private GameObject _botControlledGameObject;

		[SerializeField]
		private UI2DSprite _botControlledSprite;

		private int _combatObjectId = -1;

		[SerializeField]
		private UIButtonToggled _portraitSpectatorButton;

		[SerializeField]
		private Animator _isBeingWatchedIconAnimator;

		private Vector3 _position;

		private bool _isAnimationPlaying;

		private float DelayToDisableFeedback = 10f;

		private bool _isBeeingWatchedIconIsActive;

		private const string AnimatorSelectTrigger = "Select";

		private const string AnimatorExitTrigger = "Exit";
	}
}
