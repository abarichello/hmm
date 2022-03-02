using System;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.VoiceChat.Business;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public class VoiceChatIcons : MonoBehaviour
	{
		public string UserUniversalId
		{
			get
			{
				return this._userUniversalId;
			}
			set
			{
				this._userUniversalId = value;
				this._dummyPlayer.UniversalId = this._userUniversalId;
				this.UpdateFriendVoiceMutedStatus();
			}
		}

		public void onButtonClick_FriendVoiceHandle()
		{
			Debug.Log("pressed onButtonClick_FriendVoiceHandle");
			ObservableExtensions.Subscribe<Unit>(this._muteVoiceChatPlayer.ToggleMute(this._dummyPlayer), delegate(Unit _)
			{
				this.UpdateFriendVoiceMutedStatus();
			});
		}

		private void UpdateFriendVoiceMutedStatus()
		{
			bool flag = this._isVoiceChatPlayerMuted.IsMuted(this._dummyPlayer);
			if (this._headSetIcon_ActivatedSprite != null)
			{
				this._headSetIcon_ActivatedSprite.enabled = !flag;
			}
			if (this._headSetIcon_DesactivatedSprite != null)
			{
				this._headSetIcon_DesactivatedSprite.enabled = flag;
			}
		}

		public void onButtonClick_EnableDesableVoiceChat()
		{
			Debug.LogWarning("[NOT IMPLEMENTED] pressed onButtonClick_EnableDesableVoiceChat");
		}

		[SerializeField]
		private string _userUniversalId;

		[SerializeField]
		private UI2DSprite _headSetIcon_ActivatedSprite;

		[SerializeField]
		private UI2DSprite _headSetIcon_DesactivatedSprite;

		private Player _dummyPlayer = new Player();

		[Inject]
		private IMuteVoiceChatPlayer _muteVoiceChatPlayer;

		[Inject]
		private IIsVoiceChatPlayerMuted _isVoiceChatPlayerMuted;

		[SerializeField]
		private UI2DSprite _dialogIcon_ActivatedSprite;

		[SerializeField]
		private UI2DSprite _dialogIcon_DesactivatedSprite;
	}
}
