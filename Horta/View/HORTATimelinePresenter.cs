using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Horta.View
{
	public class HORTATimelinePresenter : IHORTATimelinePresenter, IPresenter
	{
		public HORTATimelinePresenter(IViewLoader viewLoader, IViewProvider viewProvider, ILocalizeKey translation, IGameTime gameTime)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._translation = translation;
			this._gameTime = gameTime;
			this._hideSubject = new Subject<Unit>();
		}

		public IObservable<Unit> Initialize(ITimelineController timelineController)
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._timelineController = timelineController;
			}), (Unit _) => this.Initialize());
		}

		public IObservable<Unit> Initialize()
		{
			if (this._timelineController == null)
			{
				throw new InvalidOperationException("TimelineController is null. Call Initialize(ITimelineController) instead.");
			}
			return Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_HORTA_Timeline"), delegate(Unit _)
			{
				this._isVisible = false;
				this._isDraggingTimeSlider = false;
				this._view = this._viewProvider.Provide<IHORTATimelineView>(null);
				this.InitializeView();
				this.InitializePlaybackTimerObservable();
			});
		}

		private void InitializeView()
		{
			this._availableSpeeds = this._timelineController.AvailableSpeeds;
			this.InitializePlayPauseButton();
			this.InitializeSlider();
			this.InitializeSpeedButtons();
			this.InitializeTimeLabels();
		}

		private void InitializePlaybackTimerObservable()
		{
			this._playbackTimerObservableDisposable = ObservableExtensions.Subscribe<long>(Observable.Do<long>(Observable.Where<long>(Observable.EveryUpdate(), (long _) => !this._isDraggingTimeSlider), delegate(long _)
			{
				int synchTime = this._gameTime.GetSynchTime();
				this.UpdateCurrentTimeLabelText(synchTime);
				this.UpdateTimeSlider(synchTime);
			}));
		}

		private void InitializePlayPauseButton()
		{
			if (this._timelineController.IsPlaying)
			{
				this._view.SetAsPauseButton();
			}
			else
			{
				this._view.SetAsPlayButton();
			}
			this._playButtonOnClickDisposable = ObservableExtensions.Subscribe<Unit>(this._view.PlayPauseButton.OnClick(), delegate(Unit _)
			{
				if (this._timelineController.IsPlaying)
				{
					this._timelineController.Pause();
					this._view.SetAsPlayButton();
				}
				else
				{
					this._timelineController.Play();
					this._view.SetAsPauseButton();
				}
			});
		}

		private void InitializeSlider()
		{
			this._timeSliderNormalizedValue = 0f;
			this.UpdateTimeSlider(0);
			ObservableExtensions.Subscribe<float>(this._view.TimeSlider.OnValueChanged(), delegate(float normalizedValue)
			{
				this._timeSliderNormalizedValue = normalizedValue;
				if (this._isDraggingTimeSlider)
				{
					int timeMillisFromSliderNormalizedValue = this.GetTimeMillisFromSliderNormalizedValue();
					this.UpdateCurrentTimeLabelText(timeMillisFromSliderNormalizedValue);
				}
			});
			this._timeSliderOnPointerUpDisposable = ObservableExtensions.Subscribe<Unit>(this._view.TimeSlider.OnPointerUp(), delegate(Unit _)
			{
				int timeMillisFromSliderNormalizedValue = this.GetTimeMillisFromSliderNormalizedValue();
				this._timelineController.SetTime(timeMillisFromSliderNormalizedValue);
				this.UpdateCurrentTimeLabelText(timeMillisFromSliderNormalizedValue);
				this._isDraggingTimeSlider = false;
			});
			this._timeSliderOnPointerDownDisposable = ObservableExtensions.Subscribe<Unit>(this._view.TimeSlider.OnPointerDown(), delegate(Unit _)
			{
				this._isDraggingTimeSlider = true;
			});
		}

		private int GetTimeMillisFromSliderNormalizedValue()
		{
			return (int)Mathf.Lerp(0f, (float)this._timelineController.TimelineSizeMillis, this._timeSliderNormalizedValue);
		}

		private void InitializeSpeedButtons()
		{
			this._slowButtonOnClickDisposable = ObservableExtensions.Subscribe<Unit>(this._view.SlowButton.OnClick(), delegate(Unit _)
			{
				this._timelineController.DecreaseSpeed();
			});
			this._accelerateButtonOnClickDisposable = ObservableExtensions.Subscribe<Unit>(this._view.AccelerateButton.OnClick(), delegate(Unit _)
			{
				this._timelineController.IncreaseSpeed();
			});
			this._speedChangeDisposable = ObservableExtensions.Subscribe<int>(this._timelineController.ObserveSpeedChange(), delegate(int speedIndex)
			{
				this.SetMultiplierLabelText(speedIndex);
				this.UpdateSpeedButtons(speedIndex);
			});
			this._availabilityDisposable = ObservableExtensions.Subscribe<bool>(this._timelineController.ObserveAvailability(), delegate(bool available)
			{
				this._view.MainCanvasGroup.Interactable = available;
			});
		}

		private void InitializeTimeLabels()
		{
			int currentSpeedIndex = this._timelineController.CurrentSpeedIndex;
			this.SetMultiplierLabelText(currentSpeedIndex);
			this.UpdateSpeedButtons(currentSpeedIndex);
			TimeSpan timeSpan = TimeSpan.FromMilliseconds((double)this._timelineController.TimelineSizeMillis);
			this._view.MaxTimeLabel.Text = string.Format("/ {0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
			this.UpdateCurrentTimeLabelText(0);
		}

		public IObservable<Unit> Show()
		{
			return this.ShowView();
		}

		private IObservable<Unit> ShowView()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._isVisible = true;
				this._view.MainCanvas.Enable();
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._isVisible = false;
				this._view.MainCanvas.Disable();
				this._hideSubject.OnNext(Unit.Default);
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.TryToDispose(this._playButtonOnClickDisposable);
				this.TryToDispose(this._slowButtonOnClickDisposable);
				this.TryToDispose(this._accelerateButtonOnClickDisposable);
				this.TryToDispose(this._playbackTimerObservableDisposable);
				this.TryToDispose(this._timeSliderOnPointerUpDisposable);
				this.TryToDispose(this._timeSliderOnPointerDownDisposable);
				this.TryToDispose(this._speedChangeDisposable);
				this.TryToDispose(this._availabilityDisposable);
			});
		}

		private void TryToDispose(IDisposable disposable)
		{
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		public void ToggleVisibility()
		{
			if (!this._isVisible)
			{
				ObservableExtensions.Subscribe<Unit>(this.Show());
			}
			else
			{
				ObservableExtensions.Subscribe<Unit>(this.Hide());
			}
		}

		private void UpdateTimeSlider(int timeMillis)
		{
			this._view.TimeSlider.FillPercent = (float)timeMillis / (float)this._timelineController.TimelineSizeMillis;
		}

		private void UpdateSpeedButtons(int speedIndex)
		{
			if (speedIndex == 0)
			{
				this._view.SlowButton.IsInteractable = false;
				this._view.AccelerateButton.IsInteractable = true;
				this._view.SetSpeedButtonsContext(HORTATimelineViewSpeedContext.MinimalSpeed);
			}
			else if (speedIndex == this._availableSpeeds.Length - 1)
			{
				this._view.SlowButton.IsInteractable = true;
				this._view.AccelerateButton.IsInteractable = false;
				this._view.SetSpeedButtonsContext(HORTATimelineViewSpeedContext.TopSpeed);
			}
			else
			{
				this._view.SlowButton.IsInteractable = true;
				this._view.AccelerateButton.IsInteractable = true;
				this._view.SetSpeedButtonsContext(HORTATimelineViewSpeedContext.BetweenMinimalAndTopSpeed);
			}
		}

		private void SetMultiplierLabelText(int speedIndex)
		{
			this._view.MultiplierLabel.Text = string.Format("x{0:#.##}", this._availableSpeeds[speedIndex]);
		}

		private void UpdateCurrentTimeLabelText(int timeInMillis)
		{
			TimeSpan timeSpan = TimeSpan.FromMilliseconds((double)timeInMillis);
			this._view.CurrentTimeLabel.Text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
		}

		private const string SceneName = "UI_ADD_HORTA_Timeline";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly ILocalizeKey _translation;

		private readonly IGameTime _gameTime;

		private ITimelineController _timelineController;

		private readonly ISubject<Unit> _hideSubject;

		private IHORTATimelineView _view;

		private IDisposable _playButtonOnClickDisposable;

		private IDisposable _slowButtonOnClickDisposable;

		private IDisposable _accelerateButtonOnClickDisposable;

		private IDisposable _playbackTimerObservableDisposable;

		private IDisposable _timeSliderOnPointerUpDisposable;

		private IDisposable _timeSliderOnPointerDownDisposable;

		private IDisposable _speedChangeDisposable;

		private IDisposable _availabilityDisposable;

		private bool _isVisible;

		private float _timeSliderNormalizedValue;

		private float[] _availableSpeeds;

		private bool _isDraggingTimeSlider;
	}
}
