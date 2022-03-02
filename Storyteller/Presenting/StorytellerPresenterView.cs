using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public class StorytellerPresenterView : MonoBehaviour, IStorytellerPresenterView
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

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IButton RefreshButton
		{
			get
			{
				return this._refreshButton;
			}
		}

		public IButton SearchButton
		{
			get
			{
				return this._searchButton;
			}
		}

		public IDropdown<string> RegionsDropdown
		{
			get
			{
				return this._regionDropdown;
			}
		}

		public IDropdown<string> QueuesDropdown
		{
			get
			{
				return this._queuesDropdown;
			}
		}

		public IInputField SearchField
		{
			get
			{
				return this._searchField;
			}
		}

		public UnityUiTitleInfo Title
		{
			get
			{
				return this._title;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IUiNavigationSubGroupHolder UiNavigationTopSubGroupHolder
		{
			get
			{
				return this._uiNavigationTopSubGroupHolder;
			}
		}

		public IUiNavigationSubGroupHolder UiNavigationMatchListSubGroupHolder
		{
			get
			{
				return this._uiNavigationMatchListSubGroupHolder;
			}
		}

		public IActivatable NoResultsGroup
		{
			get
			{
				return this._noResultsGroup;
			}
		}

		public ILabel NoResultsLabel
		{
			get
			{
				return this._noResultsLabel;
			}
		}

		public IUiNavigationAxisSelector UiNavigationTopGroupAxisSelector
		{
			get
			{
				return this._uiNavigationTopGroupAxisSelector;
			}
		}

		public IUiNavigationAxisSelector UiNavigationMatchListAxisSelector
		{
			get
			{
				return this._uiNavigationMatchListAxisSelector;
			}
		}

		public IStorytellerGameserverScroller GameServersScroller
		{
			get
			{
				return this._gameserversScroller;
			}
		}

		public IStorytellerGameserverConnection GameServerConnection
		{
			get
			{
				return this._gameserversScroller;
			}
		}

		private IUiNavigationAxisSelectorTransformHandler UiNavigationTopGroupAxisSelectorTransformHandler
		{
			get
			{
				return this._uiNavigationTopGroupAxisSelector;
			}
		}

		public void UiNavigationPrioritySelection()
		{
			this.UiNavigationTopGroupAxisSelectorTransformHandler.TryForceSelection(this._prioritySelectionInTopGroup);
		}

		public IStorytellerMatchInfo CreateMatchInfo()
		{
			return new StorytellerMatchInfo();
		}

		public void PlaySearchFailedByMinCharsAnim()
		{
			this._searchFailFeedback.Play();
		}

		public void ShowLoadingMatches()
		{
			this._loadingMatches.SetActive(true);
		}

		public void HideLoadingMatches()
		{
			this._loadingMatches.SetActive(false);
		}

		public void EnableSearchInteractions()
		{
			this.SearchButton.IsInteractable = true;
			this.RefreshButton.IsInteractable = true;
			this.RegionsDropdown.Interactable = true;
			this.QueuesDropdown.Interactable = true;
			this.SearchField.Interactable = true;
		}

		public void DisableSearchInteractions()
		{
			this.SearchButton.IsInteractable = false;
			this.RefreshButton.IsInteractable = false;
			this.RegionsDropdown.Interactable = false;
			this.QueuesDropdown.Interactable = false;
			this.SearchField.Interactable = false;
		}

		public IObservable<Unit> PlayIntroAnim()
		{
			return Observable.Delay<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				GUIUtils.PlayAnimation(this._introAnim, false, 1f, string.Empty);
			}), TimeSpan.FromSeconds((double)this._introAnim.clip.length));
		}

		public IObservable<Unit> PlayOutroAnim()
		{
			return Observable.Delay<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				GUIUtils.PlayAnimation(this._introAnim, true, 1f, string.Empty);
			}), TimeSpan.FromSeconds((double)this._introAnim.clip.length));
		}

		public void ShowWaitingWindow()
		{
			this._waitingWindow.Show(typeof(StorytellerMatchInfo));
		}

		public void HideWaitingWindow()
		{
			this._waitingWindow.Hide(typeof(StorytellerMatchInfo));
		}

		public IObservable<string> OnVirtualKeyboardClose()
		{
			return this._searchInputField.ObserveVirtualKeyboardClose();
		}

		private void Start()
		{
			this._viewProvider.Bind<IStorytellerPresenterView>(this, null);
			this._gameserversScroller = new UnityGameserverScroller(this._scroller, this._gameserverCellPrefab);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IStorytellerPresenterView>(null);
		}

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityButton _refreshButton;

		[SerializeField]
		private UnityButton _searchButton;

		[SerializeField]
		private StringUnityDropdown _regionDropdown;

		[SerializeField]
		private StringUnityDropdown _queuesDropdown;

		[SerializeField]
		private UnityInputField _searchField;

		[SerializeField]
		private UnityUiTitleInfo _title;

		[SerializeField]
		private GameObjectActivatable _noResultsGroup;

		[SerializeField]
		private UnityLabel _noResultsLabel;

		private UnityGameserverScroller _gameserversScroller;

		[SerializeField]
		private EnhancedScroller _scroller;

		[SerializeField]
		private StorytellerGameserverCellView _gameserverCellPrefab;

		[SerializeField]
		private Animation _searchFailFeedback;

		[SerializeField]
		private Animation _introAnim;

		[SerializeField]
		private GameObject _loadingMatches;

		[SerializeField]
		private GameObject _noResultsWarning;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationTopSubGroupHolder;

		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationMatchListSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationTopGroupAxisSelector;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationMatchListAxisSelector;

		[SerializeField]
		private Transform _prioritySelectionInTopGroup;

		[SerializeField]
		private HmmUiInputField _searchInputField;

		[InjectOnClient]
		private IViewProvider _viewProvider;

		[InjectOnClient]
		private IWaitingWindow _waitingWindow;
	}
}
