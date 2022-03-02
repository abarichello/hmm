using System;
using System.Collections.Generic;
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
	public class NguiProfileMachineDetailView : MonoBehaviour, IProfileMachineDetailView
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

		public ILabel CharacterNameLabel
		{
			get
			{
				return this._characterNameLabel;
			}
		}

		public ILabel CharacterLevelLabel
		{
			get
			{
				return this._characterLevelLabel;
			}
		}

		public IProgressBar CharacterExperienceProgressBar
		{
			get
			{
				return this._characterExperienceProgressBar;
			}
		}

		public ILabel CharacterExperienceVictoriesRatioLabel
		{
			get
			{
				return this._characterExperienceVictoriesRatioLabel;
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
			this._viewProvider.Bind<IProfileMachineDetailView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IProfileMachineDetailView>(null);
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

		public IProfileStatisticView GetStatisticView(ProfileStatistic statistic)
		{
			return this._statisticViewsDictionary[statistic];
		}

		public IEnumerable<IProfileCharacterUnlockableView> GetCharacterUnlockableViews()
		{
			return this._preSpawnedCharacterUnlockableViews;
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

		[Header("Character")]
		[SerializeField]
		private NGuiLabel _characterNameLabel;

		[SerializeField]
		private NGuiLabel _characterLevelLabel;

		[SerializeField]
		private NGuiProgressBar _characterExperienceProgressBar;

		[SerializeField]
		private NGuiLabel _characterExperienceVictoriesRatioLabel;

		[Header("Character Rewards")]
		[SerializeField]
		private NguiProfileCharacterUnlockableView[] _preSpawnedCharacterUnlockableViews;

		[Header("Statistics")]
		[SerializeField]
		private Transform _statisticViewParent;

		[SerializeField]
		private NguiGrid _statisticViewParentGrid;

		[SerializeField]
		private NguiProfileStatisticView _statisticViewPrefab;

		[SerializeField]
		private ProfileStatistics _statistics;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		private readonly Dictionary<ProfileStatistic, IProfileStatisticView> _statisticViewsDictionary = new Dictionary<ProfileStatistic, IProfileStatisticView>();
	}
}
