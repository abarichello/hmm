using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	public class UnityUiTutorialBottomPanel : MonoBehaviour, ITutorialPanelView
	{
		protected void Awake()
		{
			TutorialUIController tutorialUIController = UnityEngine.Object.FindObjectOfType<TutorialUIController>();
			this._tutorialBottomComponent = tutorialUIController.BottomPanelComponent;
		}

		protected void Start()
		{
			this._tutorialBottomComponent.RegisterView(this);
		}

		public void SetVisibility(bool visibility)
		{
			if (visibility)
			{
				base.GetComponent<Canvas>().enabled = true;
				this._animation.Play(this._animationInName);
				this.TrySetLabel(this._tutorialBottomComponent.TitleLabelDraft, this._titleLabel);
				this.TrySetLabel(this._tutorialBottomComponent.DescriptionLabelDraft, this._descriptionLabel);
			}
			else
			{
				this._animation.Play(this._animationOutName);
			}
		}

		private void TrySetLabel(string textTranslated, Text uiLabel)
		{
			if (string.IsNullOrEmpty(textTranslated))
			{
				uiLabel.enabled = false;
			}
			else
			{
				uiLabel.enabled = true;
				uiLabel.text = textTranslated;
			}
		}

		private ITutorialBottomComponent _tutorialBottomComponent;

		[SerializeField]
		private HmmUiImage _titleIcon;

		[SerializeField]
		private Text _titleLabel;

		[SerializeField]
		private Text _descriptionLabel;

		[SerializeField]
		private Animation _animation;

		[SerializeField]
		private string _animationInName;

		[SerializeField]
		private string _animationOutName;
	}
}
