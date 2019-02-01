using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX.PlotKids.VoiceChat
{
	public class VoiceChatStatusChangerGUIButton : MonoBehaviour
	{
		public void Setup(string universalID, bool isBot, bool isEnemy)
		{
			this._isBot = isBot;
			this._isEnemy = isEnemy;
			this._universalID = universalID;
			this.RefreshCanDisplayHover();
			if (this._isDisplayingInteractableTooltip)
			{
				this.SetDisplayState(false);
			}
			if (!this._canDisplayHover || SpectatorController.IsSpectating)
			{
				return;
			}
			this.RefreshGroupChatAllowedState();
			this.RefreshFriendVoiceMutedStatus();
		}

		private void RefreshCanDisplayHover()
		{
			this._canDisplayHover = (!this._isBot && !string.IsNullOrEmpty(this._universalID) && !GameHubBehaviour.Hub.User.IsUniversalIdLocalPlayer(this._universalID));
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
			this.SetDisplayState(this._collider.Raycast(ray, out raycastHit, 999999f));
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
			SingletonMonoBehaviour<VoiceChatController>.Instance.ToggleMuteUser(this._universalID);
			this.RefreshFriendVoiceMutedStatus();
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
			bool flag = SingletonMonoBehaviour<VoiceChatController>.Instance.IsUserMuted(this._universalID);
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
		}

		public void onButtonClick_EnableDesableVoiceChat()
		{
			ManagerController.Get<ChatManager>().ToggleIgnoreUserGroupChat(this._universalID);
			this.RefreshGroupChatAllowedState();
		}

		private void RefreshGroupChatAllowedState()
		{
			bool flag = !ManagerController.Get<ChatManager>().IsUserIgnored(this._universalID);
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
			if (this._isBot || this._isEnemy || SingletonMonoBehaviour<VoiceChatController>.Instance == null)
			{
				return;
			}
			bool flag = SingletonMonoBehaviour<VoiceChatController>.Instance.IsUserSpeaking(this._universalID);
			if (flag == this._voiceAlertObject.activeSelf)
			{
				return;
			}
			this._voiceAlertObject.SetActive(flag);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(VoiceChatStatusChangerGUIButton));

		[SerializeField]
		private GameObject ObjectToActivated;

		[SerializeField]
		private Collider _collider;

		[SerializeField]
		private bool _isHoverButton = true;

		private string _universalID;

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
