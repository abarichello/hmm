using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.Counselor;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
using Pocketverse;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Counselor
{
	public class HudCounselorCursorController : GameHubBehaviour
	{
		private void Reset()
		{
			this.state = HudCounselorCursorController.State.Off;
			this._anim.Stop();
			this._anim.Sample();
			this._anim.Play(this.animationOffName);
		}

		private void OnEnable()
		{
			this.Reset();
			if (!SpectatorController.IsSpectating)
			{
				GameHubBehaviour.Hub.ClientCounselorController.OnAudioPlayingChanged += this.OnAudioPlayingChanged;
				GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned += this.ListenToAllPlayersSpawned;
				GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned += this.ListenToAllPlayersSpawned;
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
				this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice activeDevice)
				{
					this.OnActiveDeviceChange();
				}));
			}
		}

		private void OnPhaseChange(BombScoreboardState bombScoreBoardState)
		{
			if (this.state != HudCounselorCursorController.State.Off && bombScoreBoardState != BombScoreboardState.BombDelivery)
			{
				this.GoToOffState(true);
			}
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.ClientCounselorController.OnAudioPlayingChanged -= this.OnAudioPlayingChanged;
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned -= this.ListenToAllPlayersSpawned;
			GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned -= this.ListenToAllPlayersSpawned;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			if (this._combat != null)
			{
				this._combat.SpawnController.OnStateChanged -= this.SpawnControllerOnStateChanged;
			}
			if (this._inputActiveDeviceChangeNotifierDisposable != null)
			{
				this._inputActiveDeviceChangeNotifierDisposable.Dispose();
				this._inputActiveDeviceChangeNotifierDisposable = null;
			}
		}

		private void ListenToAllPlayersSpawned()
		{
			if (!GameHubBehaviour.Hub.Events.Players.CarCreationFinished || !GameHubBehaviour.Hub.Events.Bots.CarCreationFinished)
			{
				return;
			}
			this._combat = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetComponent<CombatObject>();
			this._combat.SpawnController.OnStateChanged += this.SpawnControllerOnStateChanged;
		}

		private void SpawnControllerOnStateChanged(SpawnStateKind stateType)
		{
			if (this.state != HudCounselorCursorController.State.Off && stateType != SpawnStateKind.PreSpawned)
			{
				this.GoToOffState(false);
			}
		}

		private void GoToOffState(bool immediate)
		{
			this.StopCurrentAnim();
			if (immediate)
			{
				this._anim.Play(this.animationOffName);
			}
			else
			{
				this._anim.Play(this.animationKeyOutName);
			}
			this.state = HudCounselorCursorController.State.Off;
		}

		private void OnAudioPlayingChanged()
		{
			CounselorConfig.AdvicesConfig currentAdviceConfig = GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig;
			if (!this.IsValidToShowCursor(currentAdviceConfig))
			{
				if (this.state != HudCounselorCursorController.State.Off)
				{
					this.GoToOffState(false);
				}
				return;
			}
			if (!GameHubBehaviour.Hub.ClientCounselorController.IsPlaying)
			{
				return;
			}
			this.UpdateCursorInfo(currentAdviceConfig);
			if (this.state == HudCounselorCursorController.State.Off)
			{
				this.state = HudCounselorCursorController.State.In;
				this._anim.Play(this.animationKeyInName);
				if (this.updateInStateCoroutine != null)
				{
					base.StopCoroutine(this.updateInStateCoroutine);
				}
				this.updateInStateCoroutine = base.StartCoroutine(this.UpdateInState());
			}
		}

		private void UpdateCursorInfo(CounselorConfig.AdvicesConfig advicesConfig)
		{
			if (!this.IsValidToShowCursor(advicesConfig))
			{
				return;
			}
			if (this._inputGetActiveDevicePoller.GetActiveDevice() == 3)
			{
				this._messageLabel.gameObject.SetActive(false);
				this._joystickGroupGameObject.SetActive(true);
				ISprite sprite;
				string text;
				this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(advicesConfig.InputAction, ref sprite, ref text);
				this._joystickIconImage.sprite = (sprite as UnitySprite).GetSprite();
			}
			else
			{
				this._messageLabel.gameObject.SetActive(true);
				this._joystickGroupGameObject.SetActive(false);
				this._messageLabel.text = Language.Get(advicesConfig.CursorText, TranslationContext.Advisor);
			}
		}

		private bool IsValidToShowCursor(CounselorConfig.AdvicesConfig advicesConfig)
		{
			return !string.IsNullOrEmpty(advicesConfig.CursorText);
		}

		private void StopCurrentAnim()
		{
			if (this._anim.clip.wrapMode != 2)
			{
				return;
			}
			this._anim.Stop(this._anim.clip.name);
			this._anim.Rewind(this._anim.clip.name);
			this._anim.Sample();
		}

		private IEnumerator UpdateInState()
		{
			yield return UnityUtils.WaitForOneSecond;
			if (!this._anim.isPlaying)
			{
				yield return UnityUtils.WaitForEndOfFrame;
			}
			this.StopCurrentAnim();
			this._anim.Play(this.animationKeyIdleName);
			this.state = HudCounselorCursorController.State.Showing;
			yield break;
		}

		private void OnActiveDeviceChange()
		{
			if (this.state == HudCounselorCursorController.State.Showing)
			{
				this.UpdateCursorInfo(GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig);
			}
		}

		private HudCounselorCursorController.State state = HudCounselorCursorController.State.Off;

		[SerializeField]
		private Text _messageLabel;

		[SerializeField]
		private GameObject _joystickGroupGameObject;

		[SerializeField]
		private Text _joystickMessageLabel;

		[SerializeField]
		private Image _joystickIconImage;

		[SerializeField]
		private Animation _anim;

		[SerializeField]
		private string animationOffName;

		[SerializeField]
		private string animationKeyInName;

		[SerializeField]
		private string animationKeyIdleName;

		[SerializeField]
		private string animationKeyOutName;

		private Coroutine updateInStateCoroutine;

		private CombatObject _combat;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;

		public enum State
		{
			Off = 1,
			In,
			Showing,
			ShowingTransition
		}
	}
}
