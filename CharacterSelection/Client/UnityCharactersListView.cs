using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Input.UiNavigation.ContextInputNotifier;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityCharactersListView : MonoBehaviour, ICharactersListView
	{
		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroup;
			}
		}

		public IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		public IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public IUiNavigationContextInputNotifier UiNavigationContextInputNotifier
		{
			get
			{
				return this._uiNavigationContextInputNotifier;
			}
		}

		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		public ICharactersListItemView CreateCharacterView()
		{
			UnityCharactersListItemView unityCharactersListItemView = this._diContainer.InstantiatePrefabForComponent<UnityCharactersListItemView>(this._characterItemPrefab, base.transform);
			unityCharactersListItemView.SelectionToggle.group = this._selectionToggleGroup;
			return unityCharactersListItemView;
		}

		public void SetUiNavigationSelection(IToggle toggle)
		{
			UnityToggle unityToggle = (UnityToggle)toggle;
			this._uiNavigationAxisSelector.SetDoClickOnNavigationOff();
			this._uiNavigationAxisSelector.TryForceSelection(unityToggle.Toggle.transform);
			this._uiNavigationAxisSelector.SetDoClickOnNavigationOn();
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICharactersListView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICharactersListView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private DiContainer _diContainer;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private UiNavigationContextInputNotifier _uiNavigationContextInputNotifier;

		[SerializeField]
		private UnityCharactersListItemView _characterItemPrefab;

		[SerializeField]
		private ToggleGroup _selectionToggleGroup;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;
	}
}
