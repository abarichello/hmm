using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Input.UiNavigation.ScrollSelector;
using ModelViewer;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnitySkinSelectorView : MonoBehaviour, ISkinSelectorView
	{
		public IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		public IButton ConfirmSkinButton
		{
			get
			{
				return this._confirmSkinButton;
			}
		}

		public IActivatable SkinSelectedActivatable
		{
			get
			{
				return this._skinSelectedActivatable;
			}
		}

		public IAssetPreview Asset3DPreview
		{
			get
			{
				return this._modelViewer;
			}
		}

		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		public IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public IUiNavigationItemContextScroller UiNavigationItemContextScroller
		{
			get
			{
				return this._uiNavigationItemContextScroller;
			}
		}

		public ISkinSelectorListItemView CreateItemView()
		{
			UnitySkinSelectorListItemView unitySkinSelectorListItemView = this._diContainer.InstantiatePrefabForComponent<UnitySkinSelectorListItemView>(this._skinItemPrefab, this._skinListTransform);
			unitySkinSelectorListItemView.SelectionToggle.group = this._selectionToggleGroup;
			return unitySkinSelectorListItemView;
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
			this._viewProvider.Bind<ISkinSelectorView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ISkinSelectorView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private DiContainer _diContainer;

		[SerializeField]
		private UnitySkinSelectorListItemView _skinItemPrefab;

		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		[SerializeField]
		private UnityButton _confirmSkinButton;

		[SerializeField]
		private GameObjectActivatable _skinSelectedActivatable;

		[SerializeField]
		private ToggleGroup _selectionToggleGroup;

		[SerializeField]
		private Transform _skinListTransform;

		[SerializeField]
		private BaseModelViewer _modelViewer;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private UiNavigationItemContextScroller _uiNavigationItemContextScroller;
	}
}
