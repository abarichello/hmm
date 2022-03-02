using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
using Hoplon.Input.Business;
using Pocketverse;
using UniRx;
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
				Object.Destroy(base.gameObject);
				return;
			}
			this._isDisabledByConfig = this._gameArenaConfigProvider.GameArenaConfig.GetCurrentArena().DisableUINearBombFeedback;
			this._farDistanceFeedback = new TimedUpdater(this.DelayCheckerMillis, true, false);
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate += this.BombManager_ListenToMatchUpdate;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.PlayersOnListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.PlayersOnListenToObjectSpawn;
			this._inputBindNotifierDisposable = ObservableExtensions.Subscribe<int>(Observable.Do<int>(this._inputBindNotifier.ObserveBind(), delegate(int actionId)
			{
				this.OptionsOnKeyChangedCallback(actionId);
			}));
			this._inputBindResetDefaultNotifierDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._inputBindNotifier.ObserveResetDefault(), delegate(Unit _)
			{
				this.UpdateKeyData();
			}));
			this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice activeDevice)
			{
				this.OnActiveDeviceChange(activeDevice);
			}));
			this.UpdateKeyData();
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate -= this.BombManager_ListenToMatchUpdate;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.PlayersOnListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.PlayersOnListenToObjectSpawn;
			if (this._inputBindNotifierDisposable != null)
			{
				this._inputBindNotifierDisposable.Dispose();
				this._inputBindNotifierDisposable = null;
			}
			if (this._inputBindResetDefaultNotifierDisposable != null)
			{
				this._inputBindResetDefaultNotifierDisposable.Dispose();
				this._inputBindResetDefaultNotifierDisposable = null;
			}
			if (this._inputActiveDeviceChangeNotifierDisposable != null)
			{
				this._inputActiveDeviceChangeNotifierDisposable.Dispose();
				this._inputActiveDeviceChangeNotifierDisposable = null;
			}
		}

		private void Update()
		{
			if (this._isDisabledByConfig)
			{
				return;
			}
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			if (currentPlayerData == null || currentPlayerData.CharacterInstance == null || this._farDistanceFeedback.ShouldHalt())
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery || GameHubBehaviour.Hub.BombManager.ActiveBomb.TeamOwner != TeamKind.Zero)
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

		private void OptionsOnKeyChangedCallback(ControllerInputActions controlaction)
		{
			if (controlaction == 9)
			{
				this.UpdateKeyData();
			}
		}

		private void OnActiveDeviceChange(InputDevice activeDevice)
		{
			if (this.CurrentAnimationKind != BombTipWindow.AnimationKind.None)
			{
				this.UpdateToolTipLabel(this.CurrentAnimationKind);
			}
		}

		public void Show(BombTipWindow.AnimationKind kind)
		{
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial() && !GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible() && GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.BombDelivery)
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
					this.HintDescription.text = Language.Get(this.HintDraftPassDescription, TranslationContext.GUI);
				}
			}
			else
			{
				this.HintDescription.text = Language.Get(this.HintDraftGrabDescription, TranslationContext.GUI);
			}
			this.UpdateKeyData();
		}

		private void UpdateKeyData()
		{
			ISprite sprite;
			string text;
			if (this._inputTranslation.TryToGetInputActionActiveDeviceAssetOrFallbackToTranslation(9, ref sprite, ref text))
			{
				this.JoystickGroup.SetActive(true);
				this.KeyBoardGroup.SetActive(false);
				this.JoystickButtonIcon.sprite2D = (sprite as UnitySprite).GetSprite();
			}
			else
			{
				this.JoystickGroup.SetActive(false);
				this.KeyBoardGroup.SetActive(true);
				this.HintKey.text = text;
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

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private IInputBindNotifier _inputBindNotifier;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[InjectOnClient]
		private IGameArenaConfigProvider _gameArenaConfigProvider;

		private BombTipWindow.AnimationKind _nextFeedback;

		public bool IsAnimationInStarted;

		private bool _isDisabledByConfig;

		private IDisposable _inputBindNotifierDisposable;

		private IDisposable _inputBindResetDefaultNotifierDisposable;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;

		public enum AnimationKind
		{
			None,
			PassBomb,
			GrabBomb
		}
	}
}
