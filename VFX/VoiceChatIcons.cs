using System;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using UnityEngine;

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
				this.UpdateFriendVoiceMutedStatus();
			}
		}

		public void onButtonClick_FriendVoiceHandle()
		{
			SingletonMonoBehaviour<VoiceChatController>.Instance.ToggleMuteUser(this._userUniversalId);
			this.UpdateFriendVoiceMutedStatus();
			Debug.Log("pressed onButtonClick_FriendVoiceHandle");
		}

		private void UpdateFriendVoiceMutedStatus()
		{
			bool flag = SingletonMonoBehaviour<VoiceChatController>.Instance.IsUserMuted(this._userUniversalId);
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

		[SerializeField]
		private UI2DSprite _dialogIcon_ActivatedSprite;

		[SerializeField]
		private UI2DSprite _dialogIcon_DesactivatedSprite;
	}
}
