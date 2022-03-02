using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.UnityUI;
using Hoplon.Input.UiNavigation;
using UnityEngine;

namespace HeavyMetalMachines.Training.View
{
	public class UnityTrainingSelectionScreenView : MonoBehaviour, ITrainingSelectionScreenView
	{
		public ICanvas Canvas
		{
			get
			{
				return this._canvas;
			}
		}

		public ITitle Title
		{
			get
			{
				return this._titleInfo;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IAnimation ShowAnimation
		{
			get
			{
				return this._showAnimation;
			}
		}

		public IAnimation HideAnimation
		{
			get
			{
				return this._hideAnimation;
			}
		}

		public ITrainingSelectionView[] SelectionViews
		{
			get
			{
				return this._selectionViews;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<ITrainingSelectionScreenView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ITrainingSelectionScreenView>(null);
		}

		[InjectOnClient]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _canvas;

		[SerializeField]
		private UnityUiTitleInfo _titleInfo;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityTrainingSelectionView[] _selectionViews;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
