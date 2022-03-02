using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;

namespace HeavyMetalMachines.Training.View
{
	public class UnityTrainingPopUpView : MonoBehaviour, ITrainingPopUpView
	{
		public ICanvas Canvas
		{
			get
			{
				return this._canvas;
			}
		}

		public IButton PlayCasualButton
		{
			get
			{
				return this._playCasualButton;
			}
		}

		public IButton PlayTrainingButton
		{
			get
			{
				return this._playTrainingButton;
			}
		}

		public IButton CustomMatchButton
		{
			get
			{
				return this._customMatchButton;
			}
		}

		public IButton CloseButton
		{
			get
			{
				return this._closeButton;
			}
		}

		public IToggle CheckBox
		{
			get
			{
				return this._checkBox;
			}
		}

		public ILabel MatchCountLabel
		{
			get
			{
				return this._matchCountLabel;
			}
		}

		public ILabel DescriptionLabel
		{
			get
			{
				return this._descriptionLabel;
			}
		}

		public IAnimation ScreenInAnimation
		{
			get
			{
				return this._screenInAnimation;
			}
		}

		public IAnimation ScreenOutAnimation
		{
			get
			{
				return this._screenOutAnimation;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ITrainingPopUpView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ITrainingPopUpView>(null);
		}

		[InjectOnClient]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _canvas;

		[SerializeField]
		private UnityButton _playCasualButton;

		[SerializeField]
		private UnityButton _playTrainingButton;

		[SerializeField]
		private UnityButton _customMatchButton;

		[SerializeField]
		private UnityButton _closeButton;

		[SerializeField]
		private UnityToggle _checkBox;

		[SerializeField]
		private UnityAnimation _screenInAnimation;

		[SerializeField]
		private UnityAnimation _screenOutAnimation;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UnityLabel _matchCountLabel;

		[SerializeField]
		private UnityLabel _descriptionLabel;
	}
}
