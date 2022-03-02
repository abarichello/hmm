using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.Profile.Presenting;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.Input.UiNavigation;
using Hoplon.ToggleableFeatures;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileMatchesView : MonoBehaviour, IProfileMatchesView
	{
		public IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		public IActivatable Group
		{
			get
			{
				return this._group;
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

		public IActivatable LoadingSpinner
		{
			get
			{
				return this._loadingSpinner;
			}
		}

		public ILabel LoadingSpinnerLabel
		{
			get
			{
				return this._loadingSpinnerLabel;
			}
		}

		private void Awake()
		{
			if (!this._isFeatureToggled.Check(Features.ProfileRefactor))
			{
				Object.Destroy(this);
				return;
			}
			this.SetupPrefab();
			this.CreateViews();
			this._viewProvider.Bind<IProfileMatchesView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IProfileMatchesView>(null);
		}

		private void SetupPrefab()
		{
			Object.Destroy(this._matchViewObsoletePrefab.gameObject);
			this._matchViewPrefab.ResultIndicatorImage.Sprite = this._matchViewNeutralIndicatorSprite;
			this._matchViewPrefab.Deactivate();
		}

		private void CreateViews()
		{
			this._matchViews.Add(this._matchViewPrefab);
			for (int i = 0; i < 39; i++)
			{
				IProfileMatchView item = Object.Instantiate<NguiProfileMatchView>(this._matchViewPrefab, this._matchViewParent);
				this._matchViews.Add(item);
			}
			this._matchViewParentGrid.Reposition();
		}

		public List<IProfileMatchView> GetMatchViews()
		{
			return this._matchViews;
		}

		private const int MatchViewCount = 40;

		[Header("Group")]
		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private GameObjectActivatable _loadingSpinner;

		[SerializeField]
		private NGuiLabel _loadingSpinnerLabel;

		[Header("Matches")]
		[SerializeField]
		private Transform _matchViewParent;

		[SerializeField]
		private NguiGrid _matchViewParentGrid;

		[SerializeField]
		private NguiProfileMatchView _matchViewPrefab;

		[SerializeField]
		private UnitySprite _matchViewNeutralIndicatorSprite;

		[SerializeField]
		private GameObject _matchViewObsoletePrefab;

		private readonly List<IProfileMatchView> _matchViews = new List<IProfileMatchView>();

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;
	}
}
