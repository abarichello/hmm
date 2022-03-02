using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavymetalMachines.ReportSystem
{
	public class UnityFeedbackWindowPresenterView : MonoBehaviour, IFeedbackWindowPresenterView
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

		public IAnimation InAnimation
		{
			get
			{
				return this._inAnimation;
			}
		}

		public IAnimation OutAnimation
		{
			get
			{
				return this._outAnimation;
			}
		}

		public IButton OkButton
		{
			get
			{
				return this._okButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public ILabel TitleLabel
		{
			get
			{
				return this._titleLabel;
			}
		}

		public ILabel DescriptionLabel
		{
			get
			{
				return this._descriptionLabel;
			}
		}

		public IActivatable Description2TextActivatable
		{
			get
			{
				return this._description2TextActivatable;
			}
		}

		public IActivatable InfringementsTitleActivatable
		{
			get
			{
				return this._infringementsTitleActivatable;
			}
		}

		public ILabel InfringementsListLabel
		{
			get
			{
				return this._infringementsListLabel;
			}
		}

		public IActivatable PunishmentsTitleActivatable
		{
			get
			{
				return this._punishmentsTitleActivatable;
			}
		}

		public ILabel PunishmentsListLabel
		{
			get
			{
				return this._punishmentsListLabel;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IFeedbackWindowPresenterView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IFeedbackWindowPresenterView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UnityAnimation _inAnimation;

		[SerializeField]
		private UnityAnimation _outAnimation;

		[SerializeField]
		private UnityButton _okButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UnityLabel _titleLabel;

		[SerializeField]
		private UnityLabel _descriptionLabel;

		[SerializeField]
		private GameObjectActivatable _description2TextActivatable;

		[SerializeField]
		private GameObjectActivatable _infringementsTitleActivatable;

		[SerializeField]
		private UnityLabel _infringementsListLabel;

		[SerializeField]
		private GameObjectActivatable _punishmentsTitleActivatable;

		[SerializeField]
		private UnityLabel _punishmentsListLabel;
	}
}
