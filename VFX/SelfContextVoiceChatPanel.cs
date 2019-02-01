using System;
using System.Collections;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using UnityEngine;

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
			SingletonMonoBehaviour<VoiceChatController>.Instance.IsMicEnabled = !SingletonMonoBehaviour<VoiceChatController>.Instance.IsMicEnabled;
			this.EnableDisableVoiceChat();
		}

		private void EnableDisableVoiceChat()
		{
			this._micIcon_ActivatedSprite.enabled = SingletonMonoBehaviour<VoiceChatController>.Instance.IsMicEnabled;
			this._micIcon_DesactivatedSprite.enabled = !SingletonMonoBehaviour<VoiceChatController>.Instance.IsMicEnabled;
			this._mic_Activated_Label.enabled = !SingletonMonoBehaviour<VoiceChatController>.Instance.IsMicEnabled;
			this._mic_Desactivated_Label.enabled = SingletonMonoBehaviour<VoiceChatController>.Instance.IsMicEnabled;
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
	}
}
