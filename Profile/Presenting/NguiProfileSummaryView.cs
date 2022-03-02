using System;
using System.Collections.Generic;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Profile.Models;
using HeavyMetalMachines.Social.Profile.Models;
using HeavyMetalMachines.Social.Profile.Presenting;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.Input.UiNavigation;
using Hoplon.ToggleableFeatures;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileSummaryView : MonoBehaviour, IProfileSummaryView
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

		public IProfileCharacterRoleProgressView CarrierProgressView
		{
			get
			{
				return this._carrierProgressView;
			}
		}

		public IProfileCharacterRoleProgressView TacklerProgressView
		{
			get
			{
				return this._tacklerProgressView;
			}
		}

		public IProfileCharacterRoleProgressView SupportProgressView
		{
			get
			{
				return this._supportProgressView;
			}
		}

		private void Awake()
		{
			if (!this._isFeatureToggled.Check(Features.ProfileRefactor))
			{
				Object.Destroy(this);
				return;
			}
			this.PrepareStatisticViews();
			this.PrepareThreeCharacterViews();
			this._viewProvider.Bind<IProfileSummaryView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IProfileSummaryView>(null);
		}

		private void PrepareThreeCharacterViews()
		{
			NguiProfileCharacterView characterViewPrefab = this._characterViewPrefab;
			NguiProfileCharacterView nguiProfileCharacterView = Object.Instantiate<NguiProfileCharacterView>(this._characterViewPrefab, this._characterViewParent);
			NguiProfileCharacterView nguiProfileCharacterView2 = Object.Instantiate<NguiProfileCharacterView>(this._characterViewPrefab, this._characterViewParent);
			this._characterViewParentGrid.Reposition();
			NguiProfileSummaryView.PrepareCharacterView(characterViewPrefab);
			NguiProfileSummaryView.PrepareCharacterView(nguiProfileCharacterView);
			NguiProfileSummaryView.PrepareCharacterView(nguiProfileCharacterView2);
			this._characterViews.Add(characterViewPrefab);
			this._characterViews.Add(nguiProfileCharacterView);
			this._characterViews.Add(nguiProfileCharacterView2);
		}

		private void PrepareStatisticViews()
		{
			foreach (ProfileStatisticViewModel profileStatisticViewModel in this._statistics.ViewModels)
			{
				IProfileStatisticView profileStatisticView = Object.Instantiate<NguiProfileStatisticView>(this._statisticViewPrefab, this._statisticViewParent);
				profileStatisticView.IconImage.Sprite = profileStatisticViewModel.Icon;
				this._statisticViewsDictionary.Add(profileStatisticViewModel.Statistic, profileStatisticView);
			}
			this._statisticViewParentGrid.Reposition();
			this._statisticViewPrefab.gameObject.SetActive(false);
		}

		public List<IProfileCharacterView> GetCharacterViews()
		{
			return this._characterViews;
		}

		public IProfileStatisticView GetStatisticView(ProfileStatistic statistic)
		{
			return this._statisticViewsDictionary[statistic];
		}

		private static void PrepareCharacterView(NguiProfileCharacterView characterView)
		{
			characterView.GetComponentInChildren<GUIEventListener>().enabled = false;
			characterView.gameObject.SetActive(true);
		}

		[Header("Group")]
		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[Header("Most played machines")]
		[SerializeField]
		private Transform _characterViewParent;

		[SerializeField]
		private NguiGrid _characterViewParentGrid;

		[SerializeField]
		private NguiProfileCharacterView _characterViewPrefab;

		[Header("Statistics")]
		[SerializeField]
		private Transform _statisticViewParent;

		[SerializeField]
		private NguiGrid _statisticViewParentGrid;

		[SerializeField]
		private NguiProfileStatisticView _statisticViewPrefab;

		[SerializeField]
		private ProfileStatistics _statistics;

		[Header("Role progress")]
		[SerializeField]
		private NguiProfileCharacterRoleProgressView _carrierProgressView;

		[SerializeField]
		private NguiProfileCharacterRoleProgressView _tacklerProgressView;

		[SerializeField]
		private NguiProfileCharacterRoleProgressView _supportProgressView;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		private readonly List<IProfileCharacterView> _characterViews = new List<IProfileCharacterView>();

		private readonly Dictionary<ProfileStatistic, IProfileStatisticView> _statisticViewsDictionary = new Dictionary<ProfileStatistic, IProfileStatisticView>();
	}
}
