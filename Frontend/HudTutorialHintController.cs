using System;
using System.Collections;
using Pocketverse;
using Swordfish.Common.exceptions;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudTutorialHintController : GameHubBehaviour
	{
		public static HudTutorialHintController Instance { get; private set; }

		private void Awake()
		{
			if (HudTutorialHintController.Instance != null)
			{
				Debug.LogError("Multiple instances of HudTutorialHintController created. It should have only one.", this);
				return;
			}
			HudTutorialHintController.Instance = this;
		}

		protected void Start()
		{
			this.HideWindow();
		}

		protected void OnDestroy()
		{
		}

		public void ShowMessage(HudTutorialHintIconType iconType, string translatedText)
		{
			this.ShowMessage(iconType, 0, 0, translatedText);
		}

		public void ShowMessage(HudTutorialHintIconType iconType, int currStep, int maxStep, string translatedText)
		{
			if (this.IsWindowVisible())
			{
				base.StartCoroutine(this.PlayHideAnimation(delegate
				{
					this.ShowMessage(iconType, currStep, maxStep, translatedText);
				}));
				return;
			}
			this.ConfigureMessage(iconType, currStep, maxStep, translatedText);
			this.PlayShowAnimation();
		}

		public void HideMessage()
		{
			if (this.IsWindowVisible())
			{
				base.StartCoroutine(this.PlayHideAnimation(null));
			}
		}

		private void HideWindow()
		{
			this._mainCanvasGroup.alpha = 0f;
		}

		private void ShowWindow()
		{
			this._mainCanvasGroup.alpha = 1f;
		}

		private bool IsWindowVisible()
		{
			return this._mainCanvasGroup.alpha > 0.001f;
		}

		private IEnumerator PlayHideAnimation(Action onHideAnimationFinished = null)
		{
			this._messageAnimation.Play("HintTutorialOutAnimation");
			while (this._messageAnimation.IsPlaying("HintTutorialOutAnimation"))
			{
				yield return null;
			}
			this.HideWindow();
			if (onHideAnimationFinished != null)
			{
				onHideAnimationFinished();
			}
			yield break;
		}

		private void PlayShowAnimation()
		{
			this.ShowWindow();
			this._messageAnimation.Play("HintTutorialInAnimation");
		}

		private void ConfigureMessage(HudTutorialHintIconType iconType, int currStep, int maxStep, string translatedText)
		{
			this.SetMessageIconByType(iconType);
			this.SetMessageStepText(currStep, maxStep);
			this.SetMessageText(translatedText);
		}

		private void SetMessageIconByType(HudTutorialHintIconType targetIconType)
		{
			for (int i = 0; i < this.iconsData.Length; i++)
			{
				HudTutorialHintController.TutorialHintIconData tutorialHintIconData = this.iconsData[i];
				if (tutorialHintIconData.IconType == targetIconType)
				{
					this._messageImage.sprite = tutorialHintIconData.IconSprite;
					return;
				}
			}
			throw new InvalidArgumentException(string.Format("Icon type {0} is not registered!", targetIconType));
		}

		private void SetMessageStepText(int currStep, int maxStep)
		{
			if (maxStep <= 0)
			{
				this._messageStepText.text = string.Empty;
				return;
			}
			this._messageStepText.text = string.Format("{0}/{1}", currStep, maxStep);
		}

		private void SetMessageText(string translatedText)
		{
			this._messageDescriptionText.text = translatedText;
		}

		private const string HintTutorialInAnimationName = "HintTutorialInAnimation";

		private const string HintTutorialOutAnimationName = "HintTutorialOutAnimation";

		[Header("[Hud Elements]")]
		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[SerializeField]
		private Image _messageImage;

		[SerializeField]
		private Text _messageStepText;

		[SerializeField]
		private Text _messageDescriptionText;

		[SerializeField]
		private Animation _messageAnimation;

		[Header("[Data]")]
		[SerializeField]
		private HudTutorialHintController.TutorialHintIconData[] iconsData;

		[Serializable]
		private struct TutorialHintIconData
		{
			public HudTutorialHintIconType IconType;

			public Sprite IconSprite;
		}
	}
}
