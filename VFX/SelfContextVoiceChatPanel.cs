using System;
using System.Collections;
using HeavyMetalMachines.VoiceChat.Business;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public class SelfContextVoiceChatPanel : ModalGUIController
	{
		protected override void InitDialogTasks()
		{
			base.SetPositionToCurrentContext(this._contextMenuTransform);
			this.EnableDisableVoiceChat();
		}

		public void onButtonClick_EnableDesableVoiceChat()
		{
			this._enableOrDisableLocalPlayerVoiceChatMicrophone.Toggle();
			this.EnableDisableVoiceChat();
		}

		private void EnableDisableVoiceChat()
		{
			bool flag = this._isLocalPlayerMicrophoneEnabled.IsEnabled();
			this._micIcon_ActivatedSprite.enabled = flag;
			this._micIcon_DesactivatedSprite.enabled = !flag;
			this._mic_Activated_Label.enabled = !flag;
			this._mic_Desactivated_Label.enabled = flag;
		}

		public void onButtonClick_Close()
		{
			base.ResolveModalWindow();
		}

		protected override IEnumerator ResolveModalWindowTasks()
		{
			yield break;
		}

		[SerializeField]
		private Transform _contextMenuTransform;

		[Header("Self_ContextVoiceChat")]
		[SerializeField]
		private UI2DSprite _micIcon_ActivatedSprite;

		[SerializeField]
		private UI2DSprite _micIcon_DesactivatedSprite;

		[SerializeField]
		private UILabel _mic_Activated_Label;

		[SerializeField]
		private UILabel _mic_Desactivated_Label;

		[NonSerialized]
		public SocialModalGUI ParentGUI;

		[Inject]
		private IEnableOrDisableLocalPlayerVoiceChatMicrophone _enableOrDisableLocalPlayerVoiceChatMicrophone;

		[Inject]
		private IIsLocalPlayerMicrophoneEnabled _isLocalPlayerMicrophoneEnabled;
	}
}
