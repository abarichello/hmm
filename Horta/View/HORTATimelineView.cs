using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Horta.View
{
	public class HORTATimelineView : MonoBehaviour, IHORTATimelineView
	{
		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroup;
			}
		}

		public IButton PlayPauseButton
		{
			get
			{
				return this._playPauseButton;
			}
		}

		public ISlider TimeSlider
		{
			get
			{
				return this._timeSlider;
			}
		}

		public IButton SlowButton
		{
			get
			{
				return this._slowButton;
			}
		}

		public IButton AccelerateButton
		{
			get
			{
				return this._accelerateButton;
			}
		}

		public ILabel MultiplierLabel
		{
			get
			{
				return this._multiplierLabel;
			}
		}

		public ILabel CurrentTimeLabel
		{
			get
			{
				return this._currentTimeLabel;
			}
		}

		public ILabel MaxTimeLabel
		{
			get
			{
				return this._maxTimeLabel;
			}
		}

		public void SetAsPlayButton()
		{
			this._playPauseButtonImage.sprite = this._playSprite;
		}

		public void SetAsPauseButton()
		{
			this._playPauseButtonImage.sprite = this._pauseSprite;
		}

		public void SetSpeedButtonsContext(HORTATimelineViewSpeedContext timelineViewSpeedContext)
		{
			if (timelineViewSpeedContext != HORTATimelineViewSpeedContext.BetweenMinimalAndTopSpeed)
			{
				if (timelineViewSpeedContext != HORTATimelineViewSpeedContext.TopSpeed)
				{
					if (timelineViewSpeedContext == HORTATimelineViewSpeedContext.MinimalSpeed)
					{
						this._slowButtonImage.sprite = this._slowMaxSprite;
						this._accelerateButtonImage.sprite = this._accelerateNormalSprite;
					}
				}
				else
				{
					this._slowButtonImage.sprite = this._slowNormalSprite;
					this._accelerateButtonImage.sprite = this._accelerateMaxSprite;
				}
			}
			else
			{
				this._slowButtonImage.sprite = this._slowNormalSprite;
				this._accelerateButtonImage.sprite = this._accelerateNormalSprite;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IHORTATimelineView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IHORTATimelineView>(null);
		}

		[InjectOnClient]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UnityButton _playPauseButton;

		[SerializeField]
		private UnitySlider _timeSlider;

		[SerializeField]
		private UnityButton _slowButton;

		[SerializeField]
		private UnityButton _accelerateButton;

		[SerializeField]
		private UnityLabel _multiplierLabel;

		[SerializeField]
		private UnityLabel _currentTimeLabel;

		[SerializeField]
		private UnityLabel _maxTimeLabel;

		[SerializeField]
		private Image _playPauseButtonImage;

		[SerializeField]
		private Sprite _playSprite;

		[SerializeField]
		private Sprite _pauseSprite;

		[SerializeField]
		private Image _slowButtonImage;

		[SerializeField]
		private Sprite _slowNormalSprite;

		[SerializeField]
		private Sprite _slowMaxSprite;

		[SerializeField]
		private Image _accelerateButtonImage;

		[SerializeField]
		private Sprite _accelerateNormalSprite;

		[SerializeField]
		private Sprite _accelerateMaxSprite;
	}
}
