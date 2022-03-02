using System;
using System.Collections;
using System.Diagnostics;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class BaseHintGuiItem<T, TContent> : BaseGUIItem<T, TContent>, IHint where T : BaseHintGuiItem<T, TContent> where TContent : BaseHintContent
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseHintGuiItem<T, TContent>.HintDismissed EvtHintDismissed;

		public void ActivateHint()
		{
			if (base.gameObject == null)
			{
				BaseHintGuiItem<T, TContent>.Log.Warn("Trying to active a BaseHintGuiItem when the object is destroyed.");
				return;
			}
			base.gameObject.SetActive(true);
			TContent referenceObject = base.ReferenceObject;
			if (referenceObject.TimeoutSeconds > 0f)
			{
				this._timeoutCoroutine = GameHubBehaviour.Hub.StartCoroutine(this.WaitAndDestroy());
			}
		}

		public void UpdateIndex(long index)
		{
			if (base.gameObject == null)
			{
				BaseHintGuiItem<T, TContent>.Log.Warn("Trying to update the index of a BaseHintGuiItem when the object is destroyed.");
				return;
			}
			base.gameObject.name = string.Format("{0}_HintObject_{1}", index, base.GetType().Name);
		}

		public void DismissQuestionHint()
		{
			this._isFinished = true;
			if (this._timeoutCoroutine != null)
			{
				GameHubBehaviour.Hub.StopCoroutine(this._timeoutCoroutine);
				this._timeoutCoroutine = null;
			}
			if (this.EvtHintDismissed != null)
			{
				this.EvtHintDismissed(this);
			}
			NGUITools.Destroy(base.gameObject);
		}

		protected virtual void SetAsFinished()
		{
			if (this._isFinished)
			{
				return;
			}
			this.DismissQuestionHint();
		}

		protected override void SetPropertiesTasks(TContent referenceObject)
		{
			TContent referenceObject2 = base.ReferenceObject;
			if (referenceObject2.UseDefaultStyle)
			{
				this._contentLabel.gameObject.SetActive(true);
				UILabel contentLabel = this._contentLabel;
				TContent referenceObject3 = base.ReferenceObject;
				contentLabel.text = referenceObject3.TextContent;
				if (this.DefaultGroup)
				{
					this.DefaultGroup.SetActive(true);
					this.VoiceChatGroup.SetActive(false);
				}
			}
			else if (this.VoiceChatGroup)
			{
				this.DefaultGroup.SetActive(false);
				this.VoiceChatGroup.SetActive(true);
				this.VoiceChatFirstLabel.gameObject.SetActive(true);
				UILabel voiceChatFirstLabel = this.VoiceChatFirstLabel;
				TContent referenceObject4 = base.ReferenceObject;
				voiceChatFirstLabel.text = referenceObject4.TextContent;
				TContent referenceObject5 = base.ReferenceObject;
				if (string.IsNullOrEmpty(referenceObject5.NewLineTextContent))
				{
					this.VoiceChatSecondLabel.gameObject.SetActive(false);
				}
				else
				{
					this.VoiceChatSecondLabel.gameObject.SetActive(true);
					UILabel voiceChatSecondLabel = this.VoiceChatSecondLabel;
					TContent referenceObject6 = base.ReferenceObject;
					voiceChatSecondLabel.text = referenceObject6.NewLineTextContent;
				}
				TContent referenceObject7 = base.ReferenceObject;
				if (referenceObject7.Sprite == null)
				{
					this.GridWidget.leftAnchor.absolute = this.hintSprite.width;
					this.hintSprite.enabled = false;
				}
				else
				{
					this.hintSprite.enabled = true;
					UI2DSprite ui2DSprite = this.hintSprite;
					TContent referenceObject8 = base.ReferenceObject;
					ui2DSprite.sprite2D = referenceObject8.Sprite;
				}
			}
			if (this._timeoutCoroutine != null)
			{
				GameHubBehaviour.Hub.StopCoroutine(this._timeoutCoroutine);
			}
		}

		private IEnumerator WaitAndDestroy()
		{
			float initTime = Time.realtimeSinceStartup;
			for (;;)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				TContent referenceObject = base.ReferenceObject;
				if (realtimeSinceStartup >= referenceObject.TimeoutSeconds + initTime)
				{
					break;
				}
				yield return null;
			}
			this.SetAsFinished();
			yield break;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BaseHintGuiItem<T, TContent>));

		[SerializeField]
		private UIWidget _widget;

		[SerializeField]
		private UILabel _contentLabel;

		protected bool _isFinished;

		public UI2DSprite GlowEffect;

		public UIWidget GridWidget;

		public GameObject DefaultGroup;

		public GameObject VoiceChatGroup;

		public UILabel VoiceChatFirstLabel;

		public UILabel VoiceChatSecondLabel;

		public UI2DSprite hintSprite;

		private Coroutine _timeoutCoroutine;

		public delegate void HintDismissed(IHint dismissedHint);
	}
}
