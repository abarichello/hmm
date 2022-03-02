using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Chat.Business;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.VoiceChat.Business;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX.PlotKids.VoiceChat
{
	public class VoiceChatStatusChangerGUIButton : MonoBehaviour
	{
		public void Setup(IPlayer player, bool isBot, bool isEnemy)
		{
			this._isBot = isBot;
			this._isEnemy = isEnemy;
			this._player = (player ?? new Player());
			this.RefreshCanDisplayHover();
			if (this._isDisplayingInteractableTooltip)
			{
				this.SetDisplayState(false);
			}
			if (!this._canDisplayHover || SpectatorController.IsSpectating)
			{
				return;
			}
			VoiceChatStatusChangerGUIButton.Log.DebugFormat("Setup player {0}. isBot: {1}. isEnemy: {2}", new object[]
			{
				this._player.UniversalId,
				this._isBot,
				this._isEnemy
			});
			this.RefreshGroupChatAllowedState();
			this.RefreshFriendVoiceMutedStatus();
		}

		private void RefreshCanDisplayHover()
		{
			this._canDisplayHover = (!this._isBot && !string.IsNullOrEmpty(this._player.UniversalId) && !GameHubBehaviour.Hub.User.IsUniversalIdLocalPlayer(this._player.UniversalId));
		}

		private void Update()
		{
			if (!this._canDisplayHover)
			{
				return;
			}
			this.CheckHoverStatus();
			this.TryAlertMemberSpeaking();
		}

		private void CheckHoverStatus()
		{
			if (!this._isHoverButton)
			{
				return;
			}
			Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycastHit;
			this.SetDisplayState(this._collider.Raycast(ray, ref raycastHit, 999999f));
		}

		private void SetDisplayState(bool targetDisplayState)
		{
			if (this._isDisplayingInteractableTooltip == targetDisplayState)
			{
				return;
			}
			this._isDisplayingInteractableTooltip = targetDisplayState;
			this.ObjectToActivated.SetActive(targetDisplayState);
		}

		public void onButtonClick_FriendVoiceHandle()
		{
			ObservableExtensions.Subscribe<Unit>(this._muteUserVoice.ToggleMute(this._player), delegate(Unit _)
			{
				this.RefreshFriendVoiceMutedStatus();
			});
		}

		private void RefreshFriendVoiceMutedStatus()
		{
			if (this._isEnemy)
			{
				if (this._headSetIcon_ActivatedGameObject != null)
				{
					this._headSetIcon_ActivatedGameObject.SetActive(false);
				}
				if (this._headSetIcon_DesactivatedGameObject != null)
				{
					this._headSetIcon_DesactivatedGameObject.SetActive(false);
				}
				if (this._headSetIcon_ActivatedSprite != null)
				{
					this._headSetIcon_ActivatedSprite.gameObject.SetActive(false);
				}
				if (this._headSetIcon_DesactivatedSprite != null)
				{
					this._headSetIcon_DesactivatedSprite.gameObject.SetActive(false);
				}
				return;
			}
			bool flag = this._isUserMuted.IsMuted(this._player);
			if (this._headSetIcon_ActivatedGameObject != null)
			{
				this._headSetIcon_ActivatedGameObject.SetActive(!flag);
			}
			if (this._headSetIcon_DesactivatedGameObject != null)
			{
				this._headSetIcon_DesactivatedGameObject.SetActive(flag);
			}
			if (this._headSetIcon_ActivatedSprite != null)
			{
				this._headSetIcon_ActivatedSprite.gameObject.SetActive(!flag);
			}
			if (this._headSetIcon_DesactivatedSprite != null)
			{
				this._headSetIcon_DesactivatedSprite.gameObject.SetActive(flag);
			}
			VoiceChatStatusChangerGUIButton.Log.DebugFormat("Player \"{0}\" mute status = {1}", new object[]
			{
				this._player.UniversalId,
				flag
			});
		}

		public void onButtonClick_EnableDesableVoiceChat()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._blockPlayerInGroupChat.ToggleBlock(this._player), delegate(Unit _)
			{
				this.RefreshGroupChatAllowedState();
			}));
		}

		private void RefreshGroupChatAllowedState()
		{
			bool flag = !this._isPlayerBlockedInGroupChat.IsBlocked(this._player);
			if (this._dialogIcon_ActivatedSprite != null)
			{
				this._dialogIcon_ActivatedSprite.gameObject.SetActive(flag);
			}
			if (this._dialogIcon_DesactivatedSprite != null)
			{
				this._dialogIcon_DesactivatedSprite.gameObject.SetActive(!flag);
			}
			if (this._dialogIcon_ActivatedGameObject != null)
			{
				this._dialogIcon_ActivatedGameObject.SetActive(flag);
			}
			if (this._dialogIcon_DesactivatedGameObject != null)
			{
				this._dialogIcon_DesactivatedGameObject.gameObject.SetActive(!flag);
			}
		}

		private void TryAlertMemberSpeaking()
		{
			if (this._isBot || this._isEnemy)
			{
				return;
			}
			bool flag = this._isUserSpeaking.IsSpeaking(this._player);
			if (flag == this._voiceAlertObject.activeSelf)
			{
				return;
			}
			this._voiceAlertObject.SetActive(flag);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(VoiceChatStatusChangerGUIButton));

		[Inject]
		private IIsPlayerSpeakingOnVoiceChat _isUserSpeaking;

		[Inject]
		private IMuteVoiceChatPlayer _muteUserVoice;

		[Inject]
		private IIsVoiceChatPlayerMuted _isUserMuted;

		[Inject]
		private IIsPlayerBlockedInGroupChat _isPlayerBlockedInGroupChat;

		[Inject]
		private IBlockPlayerInGroupChat _blockPlayerInGroupChat;

		[SerializeField]
		private GameObject ObjectToActivated;

		[SerializeField]
		private Collider _collider;

		[SerializeField]
		private bool _isHoverButton = true;

		private IPlayer _player;

		private bool _isBot;

		private bool _isEnemy;

		private bool _canDisplayHover;

		private bool _isDisplayingInteractableTooltip;

		[SerializeField]
		private UI2DSprite _headSetIcon_ActivatedSprite;

		[SerializeField]
		private UI2DSprite _headSetIcon_DesactivatedSprite;

		[SerializeField]
		private GameObject _headSetIcon_ActivatedGameObject;

		[SerializeField]
		private GameObject _headSetIcon_DesactivatedGameObject;

		[SerializeField]
		private GameObject _voiceAlertObject;

		[SerializeField]
		private UI2DSprite _dialogIcon_ActivatedSprite;

		[SerializeField]
		private UI2DSprite _dialogIcon_DesactivatedSprite;

		[SerializeField]
		private GameObject _dialogIcon_ActivatedGameObject;

		[SerializeField]
		private GameObject _dialogIcon_DesactivatedGameObject;
	}
}
