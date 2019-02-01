using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
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
		}

		public void Setup(PlayerStats playerScrapData, CombatObject combatObject, SpawnController spawnController, string userId)
		{
			this._playerScrapData = playerScrapData;
			this._combatObject = combatObject;
			PlayerData player = combatObject.Player;
			this._botControlledGameObject.SetActive(!player.IsBot && (player.IsBotControlled || !player.Connected));
			this._combatObjectId = this._combatObject.Id.ObjId;
			this._combatObject.ListenToObjectSpawn += this.OnCombatObjecSpawn;
			this._combatObject.ListenToObjectUnspawn += this.OnCombatObjecUnspawn;
			this.RespawnController.Configure(this._combatObject);
			if (SpectatorController.IsSpectating)
			{
				this._groupIconsFeedback.gameObject.SetActive(true);
			}
			else
			{
				this._groupIconsFeedback.gameObject.SetActive(true);
				this._portraitSpectatorButton.enabled = false;
			}
			this._lastUpgradedLevel = new Dictionary<string, int>[4];
			for (int i = 0; i < 4; i++)
			{
				this._lastUpgradedLevel[i] = new Dictionary<string, int>();
			}
			GameHubBehaviour.Hub.Announcer.ListenToEvent += this.ListenToAnnouncerEvent;
			this._voiceChatStatus.Setup(player.UserId, player.IsBot, player.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team);
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
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private void ThumbConfig(bool isAlive)
		{
			this.ThumbGrey.enabled = !isAlive;
			if (!isAlive)
			{
				this.Thumb.alpha = 0.5f;
			}
			else
			{
				this.Thumb.alpha = 1f;
			}
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
			else if (isSameTeam)
			{
				color = Color.cyan;
			}
			else
			{
				color = Color.red;
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
				this._combatObject.CustomGadget0.ListenToGadgetSetLevel -= this.onGadgetSetLevel;
				this._combatObject.CustomGadget1.ListenToGadgetSetLevel -= this.onGadgetSetLevel;
				this._combatObject.CustomGadget2.ListenToGadgetSetLevel -= this.onGadgetSetLevel;
				this._combatObject.GenericGadget.ListenToGadgetSetLevel -= this.onGadgetSetLevel;
				if (this._lastUpgradedLevel != null)
				{
					for (int i = 0; i < 4; i++)
					{
						this._lastUpgradedLevel[i].Clear();
						this._lastUpgradedLevel[i] = null;
					}
				}
				this._lastUpgradedLevel = null;
			}
			SpectatorController.EvtWatchCar -= this.OnWatchCar_UpdadeFeedbackIcon;
		}

		private void ListenToAnnouncerEvent(AnnouncerManager.QueuedAnnouncerLog queuedAnnouncerLog)
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

		private void onGadgetSetLevel(GadgetBehaviour gadget, string upgradeName, int level)
		{
			if (!gadget.IsUpgradeInShop(upgradeName))
			{
				return;
			}
			int gadgetHashTableIndex = this.GetGadgetHashTableIndex(gadget.Slot);
			if (gadgetHashTableIndex < 0 || gadgetHashTableIndex >= 4)
			{
				return;
			}
			int num;
			if (this._lastUpgradedLevel[gadgetHashTableIndex].TryGetValue(upgradeName, out num))
			{
				this._lastUpgradedLevel[gadgetHashTableIndex][upgradeName] = level;
			}
			else
			{
				this._lastUpgradedLevel[gadgetHashTableIndex].Add(upgradeName, level);
				num = level;
			}
			if (num == level)
			{
				return;
			}
			if (SingletonMonoBehaviour<PanelController>.Instance.IsModalOfTypeOpened<SpectatorModalGUI>())
			{
				return;
			}
			if (base.gameObject.activeInHierarchy && level > num)
			{
				base.StartCoroutine(this.DisplayGadgetFeedback(gadget, upgradeName, level, num));
			}
		}

		private int GetGadgetHashTableIndex(GadgetSlot slot)
		{
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				return 0;
			case GadgetSlot.CustomGadget1:
				return 1;
			case GadgetSlot.CustomGadget2:
				return 2;
			case GadgetSlot.GenericGadget:
				return 3;
			}
			HudPlayersObject.Log.Error("Invalid Gadget Slot");
			return -1;
		}

		private IEnumerator DisplayGadgetFeedback(GadgetBehaviour gadget, string upgradeName, int level, int lastLevel)
		{
			while (GameHubBehaviour.Hub.Match.State != MatchData.MatchState.MatchStarted || this._isAnimationPlaying || this.BombScoredStatus())
			{
				yield return null;
			}
			this._isAnimationPlaying = true;
			this._feedbackEffectGameObject.SetActive(true);
			Animation feedbackEffectAnimation = this._feedbackEffectGameObject.GetComponent<Animation>();
			feedbackEffectAnimation.Play("BaseAnimation");
			base.StartCoroutine(this.DisableShowedFeedback());
			FMODAudioManager.PlayOneShotAt(this._upgradeFeedbackAudio, base.transform.position, 0);
			this._feedbackSellGameObject.SetActive(level < lastLevel);
			bool changeAndDisplayIcon = level > lastLevel || this._feedbackRevertGameObject.activeSelf || this._feedbackSellGameObject.activeSelf;
			if (changeAndDisplayIcon)
			{
				GadgetBehaviour.UpgradeInstance[] upgrades = gadget.Upgrades;
				GadgetBehaviour.UpgradeInstance upgradeInstance = null;
				for (int i = 0; i < upgrades.Length; i++)
				{
					if (upgrades[i].Info.Name.Equals(upgradeName))
					{
						upgradeInstance = upgrades[i];
						break;
					}
				}
				if (upgradeInstance != null)
				{
					string instanceIconName = HudUtils.GetInstanceIconName(gadget.Combat.Player.Character.Asset, upgradeInstance.Info, true);
					for (int j = 0; j < this._instanceIconSprites.Length; j++)
					{
						this._instanceIconSprites[j].SpriteName = instanceIconName;
					}
				}
				this._feedbackUpgradeGameObject.SetActive(true);
				Animation component = this._feedbackUpgradeGameObject.GetComponent<Animation>();
				component.Play("PurshaseAnimation");
			}
			yield break;
		}

		private IEnumerator DisableShowedFeedback()
		{
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(this.DelayToDisableFeedback));
			Animation feedbackEffectAnimation = this._feedbackEffectGameObject.GetComponent<Animation>();
			feedbackEffectAnimation.Play("ExitBaseAnimation");
			Animation feedbackUpgradeAnimation = this._feedbackUpgradeGameObject.GetComponent<Animation>();
			feedbackUpgradeAnimation.Play("ExitFeed");
			while (feedbackEffectAnimation.isPlaying)
			{
				yield return null;
			}
			this._isAnimationPlaying = false;
			this._feedbackSellGameObject.SetActive(false);
			this._feedbackUpgradeGameObject.SetActive(false);
			this._feedbackEffectGameObject.SetActive(false);
			this._feedbackRevertGameObject.SetActive(false);
			yield break;
		}

		private bool BombScoredStatus()
		{
			return GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.PreReplay || GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay;
		}

		private void OnWatchCar_UpdadeFeedbackIcon(CombatObject combatObject)
		{
			if (combatObject != null && combatObject == this._combatObject)
			{
				this._isBeeingWatchedIconIsActive = true;
				this._isBeingWatchedIconAnimator.SetTrigger("Select");
				return;
			}
			if (!this._isBeeingWatchedIconIsActive)
			{
				return;
			}
			this._isBeeingWatchedIconIsActive = false;
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

		public GameObject Collider;

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

		[SerializeField]
		private HMMUI2DDynamicSprite[] _instanceIconSprites;

		private int _combatObjectId = -1;

		private Dictionary<string, int>[] _lastUpgradedLevel;

		[SerializeField]
		private UIButtonToggled _portraitSpectatorButton;

		[SerializeField]
		private GameObject _isBeingWatchedIcon;

		[SerializeField]
		private Animator _isBeingWatchedIconAnimator;

		[Header("FeedBack upgrade itens")]
		private Dictionary<string, int> _gadgetUpgrades;

		[SerializeField]
		private Transform _groupIconsFeedback;

		[SerializeField]
		private GameObject _feedbackEffectGameObject;

		[SerializeField]
		private GameObject _feedbackUpgradeGameObject;

		[SerializeField]
		private GameObject _feedbackRevertGameObject;

		[SerializeField]
		private GameObject _feedbackSellGameObject;

		[SerializeField]
		private FMODAsset _upgradeFeedbackAudio;

		public List<HudTabPlayer.HudTabGadgetInfo> GadgetInfos;

		private bool _isAnimationPlaying;

		private float DelayToDisableFeedback = 10f;

		private bool _isBeeingWatchedIconIsActive;

		private const string AnimatorSelectTrigger = "Select";

		private const string AnimatorExitTrigger = "Exit";
	}
}
