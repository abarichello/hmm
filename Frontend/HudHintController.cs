using System;
using System.Collections;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Counselor;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudHintController : GameHubBehaviour
	{
		private void ShowWindow()
		{
			this._mainCanvasGroup.alpha = 1f;
		}

		private void HideWindow()
		{
			if (this._showingCursor)
			{
				this._showingCursor = false;
				GameHubBehaviour.Hub.CursorManager.Pop();
			}
			this._mainCanvasGroup.alpha = 0f;
			this._toggleEventTrigger.enabled = false;
		}

		private bool IsWindowVisible()
		{
			return this._mainCanvasGroup.alpha > 0.001f;
		}

		protected void Start()
		{
			this._messageToggle.interactable = false;
			this._messageToggle.isOn = false;
			this._toggleEventTrigger.enabled = false;
			this.HideWindow();
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback += this.OnCurrentPlayerCreated;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
		}

		protected void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback -= this.OnCurrentPlayerCreated;
			GameHubBehaviour.Hub.ClientCounselorController.OnAudioPlayingChanged -= this.ClientCounselorControllerOnAudioPlayingChanged;
			if (this._currentPlayerCombatObject != null)
			{
				this._currentPlayerCombatObject.ListenToObjectUnspawn -= this.CombatObjectOnObjectUnspawn;
			}
			this._currentPlayerCombatObject = null;
			GameHubBehaviour.Hub.Options.Game.OnCounselorHudHintChanged -= this.OnCounselorHudHintChanged;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnListenToPhaseChange;
		}

		private void OnCurrentPlayerCreated(PlayerEvent obj)
		{
			this._currentPlayerCombatObject = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
			this._currentPlayerCombatObject.ListenToObjectUnspawn += this.CombatObjectOnObjectUnspawn;
			this._showDisableCounselorMessage = (!GameHubBehaviour.Hub.Players.CurrentPlayerData.IsRookie && this.IsHudHintActive());
			this.SetupHudHintOptionsListener();
			GameHubBehaviour.Hub.Options.Game.OnCounselorHudHintChanged += this.OnCounselorHudHintChanged;
		}

		private void SetupHudHintOptionsListener()
		{
			ClientCounselorController clientCounselorController = GameHubBehaviour.Hub.ClientCounselorController;
			if (this._showDisableCounselorMessage && GameHubBehaviour.Hub.Options.Game.CounselorHudHint)
			{
				clientCounselorController.OnAudioPlayingChanged += this.ClientCounselorControllerOnAudioPlayingChanged;
			}
			else
			{
				clientCounselorController.OnAudioPlayingChanged -= this.ClientCounselorControllerOnAudioPlayingChanged;
			}
		}

		private void OnCounselorHudHintChanged()
		{
			this.SetupHudHintOptionsListener();
		}

		private void CombatObjectOnObjectUnspawn(CombatObject obj, UnspawnEvent msg)
		{
			bool flag = GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.BombDelivery && !this._currentPlayerCombatObject.Player.IsBotControlled;
			if (flag && this.IsHudHintActive())
			{
				this.ShowMessage(HudHintController.HudMessageType.Warning, Language.Get("COUNSELOR_HINT_DISABLE", TranslationSheets.Hud), this._counselorMessageToggleDelayInSec, this._counselorMessageToggleTimeoutInSec, false, null);
			}
		}

		private void BombManagerOnListenToPhaseChange(BombScoreBoard.State state)
		{
			if (state != BombScoreBoard.State.PreReplay && state != BombScoreBoard.State.Replay)
			{
				return;
			}
			if (this._playAnimationCoroutine != null)
			{
				base.StopCoroutine(this._playAnimationCoroutine);
				this._playAnimationCoroutine = null;
				if (this._messageAnimation.isPlaying)
				{
					this._messageAnimation.Rewind();
				}
				this.HideWindow();
			}
		}

		private void ClientCounselorControllerOnAudioPlayingChanged()
		{
			bool flag = !GameHubBehaviour.Hub.ClientCounselorController.IsPlaying;
			if (this._showDisableCounselorMessage && flag)
			{
				this._showDisableCounselorMessage = false;
				this.ShowMessage(HudHintController.HudMessageType.Warning, Language.Get("COUNSELOR_HINT_DISABLE", TranslationSheets.Hud), this._counselorMessageDelayInSec, this._counselorMessageTimeoutInSec, false, null);
			}
		}

		private bool IsHudHintActive()
		{
			return GameHubBehaviour.Hub.Options.Game.CounselorActive && GameHubBehaviour.Hub.Options.Game.CounselorHudHint;
		}

		public void ShowMessage(HudHintController.HudMessageType messageType, string translatedText, float animationDelayInSec, float animationTimeInSec, bool showToggle = false, string translatedToggleText = null)
		{
			this._messageImage.sprite = null;
			for (int i = 0; i < this._messageDatas.Length; i++)
			{
				HudHintController.MessageData messageData = this._messageDatas[i];
				if (messageData.MessageType == messageType)
				{
					this._messageImage.sprite = messageData.ImageSprite;
					break;
				}
			}
			this._messageText.text = translatedText;
			if (showToggle)
			{
				this._toggleEventTrigger.enabled = true;
				this._messageToggleText.text = translatedToggleText;
			}
			if (this._playAnimationCoroutine == null)
			{
				this._playAnimationCoroutine = base.StartCoroutine(this.PlayAnimation(animationDelayInSec, animationTimeInSec, showToggle));
			}
		}

		private IEnumerator PlayAnimation(float animationDelayInSec, float animationTimeInSec, bool showToggle = false)
		{
			yield return new WaitForSeconds(animationDelayInSec);
			this.ShowWindow();
			this._messageToggle.interactable = showToggle;
			this._messageToggle.isOn = false;
			string inAnimation = (!showToggle) ? "HintHUDInAnimation" : "HintHUDInRespawnAnimation";
			GUIUtils.PlayAnimation(this._messageAnimation, false, 1f, inAnimation);
			yield return new WaitForSeconds(animationTimeInSec);
			string outAnimation = (!showToggle) ? "HintHUDOutAnimation" : "HintHUDOutRespawnAnimation";
			GUIUtils.PlayAnimation(this._messageAnimation, false, 1f, outAnimation);
			while (this._messageAnimation.isPlaying)
			{
				yield return null;
			}
			this._messageToggle.interactable = false;
			this._playAnimationCoroutine = null;
			this.HideWindow();
			yield break;
		}

		public void GuiToggleOnValueChanged(bool toggleValue)
		{
			if (this._messageToggle.isOn)
			{
				GameHubBehaviour.Hub.Options.Game.CounselorHudHint = false;
				this._messageToggle.interactable = false;
				base.StopCoroutine(this._playAnimationCoroutine);
				this._playAnimationCoroutine = null;
				base.StartCoroutine(this.PlayAnimationOnToggleClicked(this._onToggleClickedTimeoutInSec));
			}
		}

		public void GuiToggleOnPointerEnter(bool value)
		{
			if (this.IsWindowVisible())
			{
				this._showingCursor = true;
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
			}
		}

		public void GuiToggleOnPointerExit(bool value)
		{
			if (this._showingCursor)
			{
				this._showingCursor = false;
				GameHubBehaviour.Hub.CursorManager.Pop();
			}
		}

		private IEnumerator PlayAnimationOnToggleClicked(float animationTimeInSec)
		{
			yield return new WaitForSeconds(animationTimeInSec);
			GUIUtils.PlayAnimation(this._messageAnimation, false, 1f, "HintHUDOutRespawnAnimation");
			while (this._messageAnimation.isPlaying)
			{
				yield return null;
			}
			this.HideWindow();
			yield break;
		}

		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[SerializeField]
		private Image _messageImage;

		[SerializeField]
		private Text _messageText;

		[SerializeField]
		private Toggle _messageToggle;

		[SerializeField]
		private Text _messageToggleText;

		[SerializeField]
		private EventTrigger _toggleEventTrigger;

		[SerializeField]
		private Animation _messageAnimation;

		[Header("[Content]")]
		[SerializeField]
		private HudHintController.MessageData[] _messageDatas;

		[SerializeField]
		private float _counselorMessageDelayInSec = 2f;

		[SerializeField]
		private float _counselorMessageTimeoutInSec = 3.5f;

		[SerializeField]
		private float _counselorMessageToggleDelayInSec = 2f;

		[SerializeField]
		private float _counselorMessageToggleTimeoutInSec = 10f;

		[SerializeField]
		private float _onToggleClickedTimeoutInSec = 0.3f;

		private bool _showDisableCounselorMessage;

		private Coroutine _playAnimationCoroutine;

		private CombatObject _currentPlayerCombatObject;

		private bool _showingCursor;

		public enum HudMessageType
		{
			Warning
		}

		[Serializable]
		private struct MessageData
		{
			public HudHintController.HudMessageType MessageType;

			public Sprite ImageSprite;
		}
	}
}
