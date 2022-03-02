using System;
using System.Collections.Generic;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.News.Business;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon;
using Hoplon.Input;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Logging;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.News.Presenting
{
	public class NewsPresenter : INewsPresenter
	{
		public NewsPresenter(IViewLoader viewLoader, IViewProvider viewProvider, IOptionsPresenter optionsPresenter, INewsGetRecommendation getRecommendation, ILogger<NewsPresenter> logger, IRandom random, IInputGetActiveDevicePoller inputGetActiveDevicePoller, INewsLinkSelector linkSelector, IClientButtonBILogger clientButtonBiLogger)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._optionsPresenter = optionsPresenter;
			this._getRecommendation = getRecommendation;
			this._logger = logger;
			this._random = random;
			this._inputGetActiveDevicePoller = inputGetActiveDevicePoller;
			this._linkSelector = linkSelector;
			this._clientButtonBiLogger = clientButtonBiLogger;
			this._compositeDisposable = new CompositeDisposable();
			this._cacheTextures = new Dictionary<string, ITexture>();
			this._windowVisible = false;
			this._currentSelectorIndex = 0;
			this._showSubject = new Subject<Unit>();
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_News"), delegate(Unit _)
			{
				this.InitializeView();
			}), delegate(Unit _)
			{
				this.InitializeAutomaticRecommendationRotation();
			});
		}

		private void InitializeAutomaticRecommendationRotation()
		{
			float automaticRecommendationRotationIntervalInSec = this._view.AutomaticRecommendationRotationIntervalInSec;
			this._shouldAutomaticallyRotateRecommendations = (automaticRecommendationRotationIntervalInSec > 0f);
			this._automaticRecommendationRotationInterval = TimeSpan.FromSeconds((double)automaticRecommendationRotationIntervalInSec);
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.MainCanvas.Enable();
				return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(this.LoadRecommendation(), delegate(Unit _)
				{
					this.ActivateSelectorToggles();
				}), delegate(Unit _)
				{
					this.UpdateSelectorNavigationButtons();
				}), delegate(Unit _)
				{
					this.UpdateItemButtonsAfterLoadRecommendations();
				}), (Unit _) => Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this.ShowWindow(),
					this.TryToLoadItem(this._firstRecommendationItem, this._view.FirstItem),
					this.TryToLoadItem(this._secondRecommendationItem, this._view.SecondItem)
				}));
			});
		}

		private void ActivateSelectorToggles()
		{
			for (int i = 0; i < this._view.Selectors.Count; i++)
			{
				if (i < this._toggleRecommendationItems.Count)
				{
					ActivatableExtensions.Activate(this._view.Selectors[i].Activatable);
				}
				else
				{
					ActivatableExtensions.Deactivate(this._view.Selectors[i].Activatable);
				}
			}
		}

		private void UpdateSelectorNavigationButtons()
		{
			bool isInteractable = this.HasRecommendationItems() && this._toggleRecommendationItems.Count > 1;
			this._view.SelectorLeftButton.IsInteractable = isInteractable;
			this._view.SelectorRightButton.IsInteractable = isInteractable;
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.DisableItemButtons();
				if (!this._windowVisible)
				{
					return Observable.ContinueWith<long, Unit>(Observable.First<long>(Observable.Where<long>(Observable.EveryUpdate(), (long _) => !this._view.HideWindowAnimation.IsPlaying)), (long _) => this.HideMainButton());
				}
				return Observable.ContinueWith<Unit, Unit>(this.HideWindow(), (Unit _) => this.HideMainButton());
			});
		}

		private IObservable<Unit> HideMainButton()
		{
			return Observable.DoOnCancel<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._view.MainCanvas.Disable();
			}), delegate()
			{
				this._view.MainCanvas.Disable();
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._compositeDisposable.Dispose();
				this._cacheTextures.Clear();
				this.TryToDisposeLoadTexture();
				this.TryToDisposeAutomaticRecommendationRotation();
				return Observable.ReturnUnit();
			});
		}

		public IObservable<Unit> ObserveShow()
		{
			return this._showSubject;
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<INewsView>(null);
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<int>(Observable.Do<int>(this._view.UiNavigationAxisSelector.ObserveNavigationSelectionId(), new Action<int>(this.UpdateSelectorFocus))));
			this.DisableItemButtons();
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(this._view.FirstItem.Button.OnClick(), delegate(Unit _)
			{
				this.TryToOpenLink(this._firstRecommendationItem, NewsCardBiPosition.LowerLeftCard);
			}));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(this._view.SecondItem.Button.OnClick(), delegate(Unit _)
			{
				this.TryToOpenLink(this._secondRecommendationItem, NewsCardBiPosition.LowerCenterCard);
			}));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(this._view.SelectorItem.Button.OnClick(), delegate(Unit _)
			{
				this.OpenSelectorRecommendationLink();
			}));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.SelectorLeftButton.OnClick(), delegate(Unit _)
			{
				this._clientButtonBiLogger.LogButtonClick(ButtonName.NewsToggleNavigationLeft);
			}), delegate(Unit _)
			{
				this.LoadAndShowPreviousRecommendation();
			}));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.SelectorRightButton.OnClick(), delegate(Unit _)
			{
				this._clientButtonBiLogger.LogButtonClick(ButtonName.NewsToggleNavigationRight);
			}), delegate(Unit _)
			{
				this.LoadAndShowNextRecommendation();
			}));
			for (int i = 0; i < this._view.Selectors.Count; i++)
			{
				int index = i;
				IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.Selectors[i].Toggle.OnToggleOn(), delegate(Unit _)
				{
					this.LoadAndShowRecommendationAtIndex(index);
				});
				this._compositeDisposable.Add(disposable);
			}
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(this._view.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				this.TryToShowOptionsWindow();
			}));
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<AxisSelectorEdge>(Observable.Do<AxisSelectorEdge>(Observable.Where<AxisSelectorEdge>(this._view.UiNavigationEdgeNotifier.ObserveOnEdgeReached(), (AxisSelectorEdge axisSelectorEdge) => axisSelectorEdge == 2), delegate(AxisSelectorEdge axisSelectorEdge)
			{
				this.ReleaseUiNavigationFocus();
			})));
		}

		private void UpdateSelectorFocus(int id)
		{
			if (id == this._view.SelectorItem.Button.GetId())
			{
				this._view.SelectorUiNavigationSubGroupHolder.SubGroupFocusGet();
			}
			else
			{
				this._view.SelectorUiNavigationSubGroupHolder.SubGroupFocusRelease();
			}
		}

		private void TryToShowOptionsWindow()
		{
			if (this._inputGetActiveDevicePoller.GetActiveDevice() == 1)
			{
				this._optionsPresenter.Show();
			}
		}

		private void LoadAndShowRecommendationAtIndex(int index)
		{
			this._currentSelectorIndex = index;
			this.TryToDisposeLoadTexture();
			this._loadTextureDisposable = ObservableExtensions.Subscribe<Unit>(this.TryToLoadItem(this._toggleRecommendationItems[index], this._view.SelectorItem));
		}

		private IObservable<Unit> TryToLoadItem(NewsRecommendationItem newsRecommendationItem, INewsItemView newsItemView)
		{
			if (newsRecommendationItem == null)
			{
				return null;
			}
			ActivatableExtensions.Activate(newsItemView.SpinnerActivatable);
			ActivatableExtensions.Deactivate(newsItemView.ErrorActivatable);
			return Observable.AsUnitObservable<ITexture>(Observable.DoOnTerminate<ITexture>(Observable.Catch<ITexture, Exception>(Observable.Do<ITexture>(this.LoadTexture(newsRecommendationItem.ImageUrl), delegate(ITexture texture)
			{
				newsItemView.TextureImage.Texture = texture;
			}), (Exception exception) => this.ShowItemErrorFeedback(newsRecommendationItem.ImageUrl, exception, newsItemView)), delegate()
			{
				ActivatableExtensions.Deactivate(newsItemView.SpinnerActivatable);
			}));
		}

		private IObservable<ITexture> ShowItemErrorFeedback(string imageUrl, Exception exception, INewsItemView newsItemView)
		{
			this._logger.WarnFormat("Error on load texture: Url[{0}] Ex:{1}", new object[]
			{
				imageUrl,
				exception.ToString()
			});
			ActivatableExtensions.Activate(newsItemView.ErrorActivatable);
			return Observable.Empty<ITexture>();
		}

		private IObservable<ITexture> LoadTexture(string imageUrl)
		{
			ITexture texture;
			if (this._cacheTextures.TryGetValue(imageUrl, out texture))
			{
				return Observable.Return<ITexture>(texture);
			}
			string text = string.Format("{0}?{1}", imageUrl, this._random.Range(1, 9999));
			return Observable.Select<WWW, ITexture>(ObservableWWW.GetWWW(text, null, null), delegate(WWW www)
			{
				ITexture texture2 = new UnityTexture(www.texture);
				this._cacheTextures[imageUrl] = texture2;
				return texture2;
			});
		}

		private IObservable<Unit> LoadRecommendation()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (this._toggleRecommendationItems != null)
				{
					return Observable.ReturnUnit();
				}
				return Observable.AsUnitObservable<List<NewsRecommendationItem>>(Observable.DoOnError<List<NewsRecommendationItem>>(Observable.Do<List<NewsRecommendationItem>>(this._getRecommendation.GetRecommendationItems(), new Action<List<NewsRecommendationItem>>(this.UpdateRecommendationItems)), delegate(Exception ex)
				{
					this._logger.Error("LoadRecommendation error: " + ex);
				}));
			});
		}

		private void UpdateRecommendationItems(List<NewsRecommendationItem> recommendationItems)
		{
			this._toggleRecommendationItems = new List<NewsRecommendationItem>(recommendationItems.Count);
			this._firstRecommendationItem = null;
			this._secondRecommendationItem = null;
			if (recommendationItems.Count == 0)
			{
				return;
			}
			this._firstRecommendationItem = recommendationItems[0];
			if (recommendationItems.Count > 1)
			{
				this._secondRecommendationItem = recommendationItems[1];
			}
			for (int i = 2; i < recommendationItems.Count; i++)
			{
				this._toggleRecommendationItems.Add(recommendationItems[i]);
			}
		}

		private void OpenSelectorRecommendationLink()
		{
			this.TryToOpenLink(this._toggleRecommendationItems[this._currentSelectorIndex], NewsCardBiPosition.LowerRightToggleCard);
		}

		private void TryToOpenLink(NewsRecommendationItem newsRecommendationItem, NewsCardBiPosition biPosition)
		{
			if (newsRecommendationItem != null)
			{
				this._linkSelector.Execute(newsRecommendationItem.LinkUrl, biPosition);
			}
		}

		private IObservable<Unit> ShowWindow()
		{
			this._windowVisible = true;
			this._showSubject.OnNext(Unit.Default);
			return Observable.Do<Unit>(Observable.Do<Unit>(this._view.ShowWindowAnimation.Play(), delegate(Unit _)
			{
				this.TryToSelectToggleIfNoneSelected();
			}), delegate(Unit _)
			{
				this.TryToStartAutomaticRecommendationRotation();
			});
		}

		public void GetUiNavigationFocus()
		{
			this._view.UiNavigationGroupHolder.AddGroup();
		}

		private void ReleaseUiNavigationFocus()
		{
			this._view.UiNavigationGroupHolder.RemoveGroup();
		}

		public IObservable<Unit> HideWindow()
		{
			if (!this._windowVisible)
			{
				return Observable.ReturnUnit();
			}
			this._windowVisible = false;
			this.TryToDisposeAutomaticRecommendationRotation();
			return Observable.Do<Unit>(Observable.Do<Unit>(this._view.HideWindowAnimation.Play(), delegate(Unit _)
			{
				this.ReleaseUiNavigationFocus();
			}), delegate(Unit _)
			{
				this._view.UiNavigationAxisSelector.ClearSelection();
			});
		}

		private void TryToSelectToggleIfNoneSelected()
		{
			if (!this.HasRecommendationItems())
			{
				return;
			}
			for (int i = 0; i < this._view.Selectors.Count; i++)
			{
				INewsSelectorView newsSelectorView = this._view.Selectors[i];
				if (newsSelectorView.Toggle.IsOn)
				{
					return;
				}
			}
			this._view.Selectors[0].Toggle.IsOn = true;
		}

		private void LoadAndShowPreviousRecommendation()
		{
			if (!this.HasRecommendationItems())
			{
				return;
			}
			this._currentSelectorIndex--;
			if (this._currentSelectorIndex < 0)
			{
				this._currentSelectorIndex = this._toggleRecommendationItems.Count - 1;
			}
			this._view.Selectors[this._currentSelectorIndex].Toggle.IsOn = true;
		}

		private void LoadAndShowNextRecommendation()
		{
			if (!this.HasRecommendationItems())
			{
				return;
			}
			this._currentSelectorIndex++;
			if (this._currentSelectorIndex > this._toggleRecommendationItems.Count - 1)
			{
				this._currentSelectorIndex = 0;
			}
			this._view.Selectors[this._currentSelectorIndex].Toggle.IsOn = true;
		}

		private bool HasRecommendationItems()
		{
			return this._toggleRecommendationItems != null && this._toggleRecommendationItems.Count > 0;
		}

		private void TryToStartAutomaticRecommendationRotation()
		{
			if (!this._shouldAutomaticallyRotateRecommendations)
			{
				return;
			}
			List<IObservable<Unit>> list = new List<IObservable<Unit>>();
			foreach (INewsSelectorView newsSelectorView in this._view.Selectors)
			{
				list.Add(newsSelectorView.Toggle.OnToggleOn());
			}
			IObservable<bool>[] array = new IObservable<bool>[2];
			array[0] = Observable.Select<long, bool>(Observable.Timer(this._automaticRecommendationRotationInterval), (long _) => true);
			array[1] = Observable.Select<Unit, bool>(Observable.Merge<Unit>(list), (Unit _) => false);
			this._automaticRecommendationRotationDisposable = ObservableExtensions.Subscribe<bool>(Observable.RepeatSafe<bool>(Observable.Do<bool>(Observable.Where<bool>(Observable.Do<bool>(Observable.First<bool>(Observable.Merge<bool>(array)), delegate(bool keepAutomaticRotation)
			{
				this._shouldAutomaticallyRotateRecommendations = keepAutomaticRotation;
			}), (bool _) => this._shouldAutomaticallyRotateRecommendations), delegate(bool _)
			{
				this.LoadAndShowNextRecommendation();
			})));
		}

		private void UpdateItemButtonsAfterLoadRecommendations()
		{
			this._view.FirstItem.Button.IsInteractable = (this._firstRecommendationItem != null);
			this._view.SecondItem.Button.IsInteractable = (this._secondRecommendationItem != null);
			this._view.SelectorItem.Button.IsInteractable = (this._toggleRecommendationItems.Count > 0);
		}

		private void DisableItemButtons()
		{
			this._view.FirstItem.Button.IsInteractable = false;
			this._view.SecondItem.Button.IsInteractable = false;
			this._view.SelectorItem.Button.IsInteractable = false;
		}

		private void TryToDisposeLoadTexture()
		{
			NewsPresenter.TryDispose(ref this._loadTextureDisposable);
		}

		private void TryToDisposeAutomaticRecommendationRotation()
		{
			NewsPresenter.TryDispose(ref this._automaticRecommendationRotationDisposable);
		}

		private static void TryDispose(ref IDisposable disposable)
		{
			if (disposable == null)
			{
				return;
			}
			disposable.Dispose();
			disposable = null;
		}

		private const string SceneName = "UI_ADD_News";

		private INewsView _view;

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly IOptionsPresenter _optionsPresenter;

		private readonly INewsGetRecommendation _getRecommendation;

		private readonly ILogger<NewsPresenter> _logger;

		private readonly IRandom _random;

		private readonly IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		private readonly INewsLinkSelector _linkSelector;

		private readonly IClientButtonBILogger _clientButtonBiLogger;

		private readonly CompositeDisposable _compositeDisposable;

		private readonly Dictionary<string, ITexture> _cacheTextures;

		private NewsRecommendationItem _firstRecommendationItem;

		private NewsRecommendationItem _secondRecommendationItem;

		private List<NewsRecommendationItem> _toggleRecommendationItems;

		private IDisposable _loadTextureDisposable;

		private IDisposable _automaticRecommendationRotationDisposable;

		private bool _windowVisible;

		private int _currentSelectorIndex;

		private bool _shouldAutomaticallyRotateRecommendations = true;

		private TimeSpan _automaticRecommendationRotationInterval;

		private readonly Subject<Unit> _showSubject;
	}
}
