using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class BombTipWindow : GameHubBehaviour
	{
		private BombTipWindow.AnimationKind CurrentAnimationKind
		{
			get
			{
				return this._currentAnimationKind;
			}
			set
			{
				if (this._currentAnimationKind == value)
				{
					return;
				}
				this._currentAnimationKind = value;
			}
		}

		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			this._farDistanceFeedback = new TimedUpdater(this.DelayCheckerMillis, true, false);
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback += this.OptionsOnKeyChangedCallback;
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate += this.BombManager_ListenToMatchUpdate;
			GameHubBehaviour.Hub.GuiScripts.Esc.OnControlModeChangedCallback += this.OnControlModeChangedCallback;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.PlayersOnListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.PlayersOnListenToObjectSpawn;
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback -= this.OptionsOnKeyChangedCallback;
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate -= this.BombManager_ListenToMatchUpdate;
			GameHubBehaviour.Hub.GuiScripts.Esc.OnControlModeChangedCallback -= this.OnControlModeChangedCallback;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.PlayersOnListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.PlayersOnListenToObjectSpawn;
		}

		private void Update()
		{
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			if (currentPlayerData == null || currentPlayerData.CharacterInstance == null || this._farDistanceFeedback.ShouldHalt())
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.BombDelivery || GameHubBehaviour.Hub.BombManager.ActiveBomb.TeamOwner != TeamKind.Zero)
			{
				if (this.CurrentAnimationKind == BombTipWindow.AnimationKind.GrabBomb)
				{
					this.Hide();
				}
				return;
			}
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			Identifiable characterInstance = currentPlayerData.CharacterInstance;
			if (!characterInstance.gameObject.activeInHierarchy)
			{
				if (this.CurrentAnimationKind != BombTipWindow.AnimationKind.None)
				{
					this.Hide();
				}
				return;
			}
			float sqrMagnitude = (GameHubBehaviour.Hub.BombManager.BombMovement.transform.position - characterInstance.transform.position).sqrMagnitude;
			if (sqrMagnitude > (float)(GameHubBehaviour.Hub.BombManager.Rules.GrabFeedbackMaxBombDistance * GameHubBehaviour.Hub.BombManager.Rules.GrabFeedbackMaxBombDistance))
			{
				if (this.CurrentAnimationKind == BombTipWindow.AnimationKind.GrabBomb)
				{
					this.Hide();
				}
				return;
			}
			if (this.CurrentAnimationKind == BombTipWindow.AnimationKind.PassBomb)
			{
				this.Hide();
				return;
			}
			if (this.CurrentAnimationKind == BombTipWindow.AnimationKind.GrabBomb)
			{
				return;
			}
			if (!this.CanShow())
			{
				return;
			}
			this.Show(BombTipWindow.AnimationKind.GrabBomb);
		}

		public void OnAnimationInStarted()
		{
			this.IsAnimationInStarted = true;
		}

		public void OnAnimationOutEnd()
		{
			this.IsAnimationInStarted = false;
			this.CurrentAnimationKind = BombTipWindow.AnimationKind.None;
			if (this._nextFeedback == BombTipWindow.AnimationKind.None)
			{
				return;
			}
			if (!this.CanShow())
			{
				return;
			}
			this.Show(this._nextFeedback);
			this._nextFeedback = BombTipWindow.AnimationKind.None;
		}

		private void BombManager_ListenToMatchUpdate()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			if ((this.CurrentAnimationKind == BombTipWindow.AnimationKind.GrabBomb && GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb()) || (this.CurrentAnimationKind == BombTipWindow.AnimationKind.PassBomb && !GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb()))
			{
				this.Hide();
			}
			if (this.CurrentAnimationKind == BombTipWindow.AnimationKind.PassBomb)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.BombManager.IsCarryingBomb(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.Id.ObjId))
			{
				return;
			}
			this.Hide();
			this._nextFeedback = BombTipWindow.AnimationKind.PassBomb;
		}

		private void OptionsOnKeyChangedCallback(ControlAction controlaction)
		{
			if (controlaction == ControlAction.GadgetDropBomb)
			{
				this.UpdateKeyData();
			}
		}

		private void OnControlModeChangedCallback(CarInput.DrivingStyleKind drivingStyleKind)
		{
			if (this.CurrentAnimationKind != BombTipWindow.AnimationKind.None)
			{
				this.UpdateToolTipLabel(this.CurrentAnimationKind);
			}
		}

		public void Show(BombTipWindow.AnimationKind kind)
		{
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial() && !GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible() && GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.BombDelivery)
			{
				this.CurrentAnimationKind = kind;
				this.UpdateToolTipLabel(kind);
				this.Animator.SetBool("active", true);
			}
		}

		private void PlayersOnListenToObjectSpawn(PlayerEvent data)
		{
			if (data.TargetId == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				this._playerRespawning = false;
			}
		}

		private void PlayersOnListenToObjectUnspawn(PlayerEvent data)
		{
			if (data.TargetId == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				if (this.CurrentAnimationKind != BombTipWindow.AnimationKind.None)
				{
					this.Hide();
				}
				this._playerRespawning = true;
			}
		}

		private bool CanShow()
		{
			return !this._playerRespawning;
		}

		public void Hide()
		{
			if (!this.IsAnimationInStarted)
			{
				this.CurrentAnimationKind = BombTipWindow.AnimationKind.None;
				this._nextFeedback = BombTipWindow.AnimationKind.None;
			}
			this.Animator.SetBool("active", false);
		}

		private void UpdateToolTipLabel(BombTipWindow.AnimationKind kind)
		{
			if (kind != BombTipWindow.AnimationKind.GrabBomb)
			{
				if (kind == BombTipWindow.AnimationKind.PassBomb)
				{
					this.HintDescription.text = Language.Get(this.HintDraftPassDescription, TranslationSheets.GUI);
				}
			}
			else
			{
				this.HintDescription.text = Language.Get(this.HintDraftGrabDescription, TranslationSheets.GUI);
			}
			this.UpdateKeyData();
		}

		private void UpdateKeyData()
		{
			bool flag = ControlOptions.IsUsingControllerJoystick(GameHubBehaviour.Hub);
			if (flag)
			{
				this.JoystickGroup.SetActive(true);
				this.KeyBoardGroup.SetActive(false);
				if (!ControlOptions.IsActionScanning(ControlAction.GadgetDropBomb, ControlOptions.ControlActionInputType.Secondary))
				{
					this.JoystickButtonIcon.sprite2D = GameHubBehaviour.Hub.GuiScripts.JoystickShortcutIcons.GetJoystickShortcutIcon(ControlOptions.GetText(ControlAction.GadgetDropBomb, ControlOptions.ControlActionInputType.Secondary));
				}
			}
			else
			{
				this.JoystickGroup.SetActive(false);
				this.KeyBoardGroup.SetActive(true);
				if (!ControlOptions.IsActionScanning(ControlAction.GadgetDropBomb, ControlOptions.ControlActionInputType.Primary))
				{
					this.HintKey.text = ControlOptions.GetTextlocalized(ControlAction.GadgetDropBomb, ControlOptions.ControlActionInputType.Primary);
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombTipWindow));

		public Animator Animator;

		public UILabel HintDescription;

		public string HintDraftPassDescription;

		public string HintDraftGrabDescription;

		public UILabel HintKey;

		public GameObject KeyBoardGroup;

		public GameObject JoystickGroup;

		public UI2DSprite JoystickButtonIcon;

		private bool _playerRespawning;

		public int DelayCheckerMillis = 250;

		private TimedUpdater _farDistanceFeedback;

		private BombTipWindow.AnimationKind _currentAnimationKind;

		private BombTipWindow.AnimationKind _nextFeedback;

		public bool IsAnimationInStarted;

		public enum AnimationKind
		{
			None,
			PassBomb,
			GrabBomb
		}
	}
}
