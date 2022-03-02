using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.News.Presenting
{
	public class NewsView : MonoBehaviour, INewsView
	{
		public float AutomaticRecommendationRotationIntervalInSec
		{
			get
			{
				return (float)this._automaticRecommendationRotationIntervalInSec;
			}
		}

		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public IAnimation ShowWindowAnimation
		{
			get
			{
				return this._showWindowAnimation;
			}
		}

		public IAnimation HideWindowAnimation
		{
			get
			{
				return this._hideWindowAnimation;
			}
		}

		public INewsItemView FirstItem
		{
			get
			{
				return this._firstNewsItemView;
			}
		}

		public INewsItemView SecondItem
		{
			get
			{
				return this._secondNewsItemView;
			}
		}

		public INewsItemView SelectorItem
		{
			get
			{
				return this._selectorNewsItemView;
			}
		}

		public List<INewsSelectorView> Selectors
		{
			get
			{
				return this._selectors;
			}
		}

		public IButton SelectorLeftButton
		{
			get
			{
				return this.selectorLeftButton;
			}
		}

		public IButton SelectorRightButton
		{
			get
			{
				return this.selectorRightButton;
			}
		}

		public ICanvasGroup SelectorLeftCanvasGroup
		{
			get
			{
				return this.selectorLeftCanvasGroup;
			}
		}

		public ICanvasGroup SelectorRightCanvasGroup
		{
			get
			{
				return this.selectorRightCanvasGroup;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IUiNavigationEdgeNotifier UiNavigationEdgeNotifier
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public IUiNavigationSubGroupHolder SelectorUiNavigationSubGroupHolder
		{
			get
			{
				return this._selectorUiNavigationSubGroupHolder;
			}
		}

		private void Awake()
		{
			ActivatableExtensions.Deactivate(this.newsSelectorToggleViewReference.Activatable);
			this.CreateSelectorsTogglePool();
			this._viewProvider.Bind<INewsView>(this, null);
		}

		private void CreateSelectorsTogglePool()
		{
			UnityToggle unityToggle = (UnityToggle)this.newsSelectorToggleViewReference.Toggle;
			this._selectors.Add(this.newsSelectorToggleViewReference);
			for (int i = 1; i < 10; i++)
			{
				NewsSelectorToggleView newsSelectorToggleView = Object.Instantiate<NewsSelectorToggleView>(this.newsSelectorToggleViewReference, this.newsSelectorToggleViewReference.transform.parent);
				UnityToggle unityToggle2 = (UnityToggle)newsSelectorToggleView.Toggle;
				unityToggle2.Toggle.group = unityToggle.Toggle.group;
				this._selectors.Add(newsSelectorToggleView);
			}
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<INewsView>(null);
		}

		private const int SelectorsPoolMaxQuantity = 10;

		[SerializeField]
		private int _automaticRecommendationRotationIntervalInSec = 5;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityAnimation _showWindowAnimation;

		[SerializeField]
		private UnityAnimation _hideWindowAnimation;

		[SerializeField]
		private NewsItemView _firstNewsItemView;

		[SerializeField]
		private NewsItemView _secondNewsItemView;

		[SerializeField]
		private NewsItemView _selectorNewsItemView;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private UiNavigationSubGroupHolder _selectorUiNavigationSubGroupHolder;

		[SerializeField]
		private NewsSelectorToggleView newsSelectorToggleViewReference;

		[SerializeField]
		private UnityButton selectorLeftButton;

		[SerializeField]
		private UnityButton selectorRightButton;

		[SerializeField]
		private UnityCanvasGroup selectorLeftCanvasGroup;

		[SerializeField]
		private UnityCanvasGroup selectorRightCanvasGroup;

		[Inject]
		private IViewProvider _viewProvider;

		private readonly List<INewsSelectorView> _selectors = new List<INewsSelectorView>(10);
	}
}
