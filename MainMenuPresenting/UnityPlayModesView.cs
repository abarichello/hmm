using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class UnityPlayModesView : MonoBehaviour, IPlayModesView
	{
		public IActivatable Group
		{
			get
			{
				return this._group;
			}
		}

		public IActivatable CompetitiveGroup
		{
			get
			{
				return this._competitiveGroup;
			}
		}

		public IButton OpenCompetitiveModeInfoButton
		{
			get
			{
				return this._openCompetitiveModeInfoButton;
			}
		}

		public IButton OpenTrainingModeButton
		{
			get
			{
				return this._opentrainingModesButton;
			}
		}

		public IActivatable OpenCompetitiveModeInfoActivatable
		{
			get
			{
				return this._openCompetitiveModeInfoActivatable;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IAlpha[] ModesArtsAlphas
		{
			get
			{
				return this._modesArtsAlphas;
			}
		}

		public IAlpha RootPanel
		{
			get
			{
				return this._rootPanel;
			}
		}

		public IActivatable RootObject
		{
			get
			{
				return this._rootObject;
			}
		}

		public IAnimation ModesAnimationIn
		{
			get
			{
				return this._modesAnimationIn;
			}
		}

		public IAnimation BackGroundAnimatinIn
		{
			get
			{
				return this._backGroundAnimatinIn;
			}
		}

		public IAnimation ModesAnimationOut
		{
			get
			{
				return this._modesAnimationOut;
			}
		}

		public IAnimation BackGroundAnimatinOut
		{
			get
			{
				return this._backGroundAnimatinOut;
			}
		}

		public IButton OpenNormalModeButton
		{
			get
			{
				return this._openNormalModeButton;
			}
		}

		public IButton OpenCustomModeButton
		{
			get
			{
				return this._openCustomModeButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public IButton RegionButton
		{
			get
			{
				return this._regionButton;
			}
		}

		public ILabel CrossplayActivatedLabel
		{
			get
			{
				return this._crossplayActivatedLabel;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IPlayModesView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IPlayModesView>(null);
		}

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private GameObjectActivatable _competitiveGroup;

		[SerializeField]
		private NGuiButton _openCompetitiveModeInfoButton;

		[SerializeField]
		private NGuiButton _opentrainingModesButton;

		[SerializeField]
		private NGuiButton _openNormalModeButton;

		[SerializeField]
		private NGuiButton _openCustomModeButton;

		[SerializeField]
		private GameObjectActivatable _openCompetitiveModeInfoActivatable;

		[SerializeField]
		private NGuiButton _backButton;

		[SerializeField]
		private NGUIWidgetAlpha[] _modesArtsAlphas;

		[SerializeField]
		private NGUIPanelAlpha _rootPanel;

		[SerializeField]
		private GameObjectActivatable _rootObject;

		[SerializeField]
		private UnityAnimation _modesAnimationIn;

		[SerializeField]
		private UnityAnimation _backGroundAnimatinIn;

		[SerializeField]
		private UnityAnimation _modesAnimationOut;

		[SerializeField]
		private UnityAnimation _backGroundAnimatinOut;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private NGuiButton _regionButton;

		[SerializeField]
		private NGuiLabel _crossplayActivatedLabel;

		[Inject]
		[UsedImplicitly]
		private IViewProvider _viewProvider;
	}
}
