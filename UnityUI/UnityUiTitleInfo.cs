using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiTitleInfo : MonoBehaviour, ITitle
	{
		public string Title
		{
			get
			{
				return this._titleText.text;
			}
			set
			{
				this._titleText.text = value;
				this._titleShadowText.text = value;
			}
		}

		public string Subtitle
		{
			get
			{
				return this._subtitleText.text;
			}
			set
			{
				this._subtitleText.text = value;
			}
		}

		public HmmUiText.TextStyles SubtitleTextStyle
		{
			get
			{
				return this._subtitleText.TextStyle;
			}
			set
			{
				this._subtitleText.TextStyle = value;
			}
		}

		public string Description
		{
			get
			{
				return this._descriptionText.text;
			}
			set
			{
				this._descriptionText.text = value;
			}
		}

		public HmmUiText.TextStyles DescriptionTextStyle
		{
			get
			{
				return this._subtitleText.TextStyle;
			}
			set
			{
				this._subtitleText.TextStyle = value;
			}
		}

		public IActivatable SubtitleActivatable
		{
			get
			{
				return new FunctionActivatable(delegate()
				{
					this._subtitleText.gameObject.SetActive(true);
				}, delegate()
				{
					this._subtitleText.gameObject.SetActive(false);
				});
			}
		}

		public IActivatable DescriptionActivatable
		{
			get
			{
				return new FunctionActivatable(delegate()
				{
					this._descriptionText.gameObject.SetActive(true);
				}, delegate()
				{
					this._descriptionText.gameObject.SetActive(false);
				});
			}
		}

		public IButton InfoButton
		{
			get
			{
				return this._infoButton;
			}
		}

		public HmmUiText.TextStyles TitleTextStyle
		{
			get
			{
				return this._titleText.TextStyle;
			}
			set
			{
				this._titleText.TextStyle = value;
				this._titleShadowText.TextStyle = value;
			}
		}

		public void Setup(string titleText, HmmUiText.TextStyles titleTextStyle, string subtitleText, HmmUiText.TextStyles subtitleTextStyle, string descriptionText, HmmUiText.TextStyles descriptionStyle, bool enableInfoButton = false)
		{
			this.Title = titleText;
			this.TitleTextStyle = titleTextStyle;
			this.SetSubtitle(subtitleText, subtitleTextStyle);
			this.SetDescription(descriptionText, descriptionStyle);
			if (enableInfoButton)
			{
				ActivatableExtensions.Activate(this._infoButton);
			}
			else
			{
				ActivatableExtensions.Deactivate(this._infoButton);
			}
		}

		private void SetSubtitle(string subtitleText, HmmUiText.TextStyles subtitleTextStyle)
		{
			this._subtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitleText));
			this.Subtitle = subtitleText;
			this.SubtitleTextStyle = subtitleTextStyle;
		}

		private void SetDescription(string descriptionText, HmmUiText.TextStyles descriptionStyle)
		{
			this._descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(descriptionText));
			this.Description = descriptionText;
			this.DescriptionTextStyle = descriptionStyle;
		}

		public IObservable<Unit> ObserveInfoButtonClick()
		{
			return this._infoButton.OnClick();
		}

		[SerializeField]
		private HmmUiText _titleText;

		[SerializeField]
		private HmmUiText _titleShadowText;

		[SerializeField]
		private HmmUiText _subtitleText;

		[SerializeField]
		private HmmUiText _descriptionText;

		[SerializeField]
		private UnityButton _infoButton;
	}
}
