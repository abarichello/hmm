using System;
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
	public class NguiProfileView : MonoBehaviour, IProfileView
	{
		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
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

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IToggle SummaryTabToggle
		{
			get
			{
				return this._summaryTabToggle;
			}
		}

		public IToggle MachinesTabToggle
		{
			get
			{
				return this._machinesTabToggle;
			}
		}

		public IToggle MatchesTabToggle
		{
			get
			{
				return this._matchesTabToggle;
			}
		}

		public ILabel PlayerNameLabel
		{
			get
			{
				return this._playerNameLabel;
			}
		}

		public ILabel PlayerTagLabel
		{
			get
			{
				return this._playerTagLabel;
			}
		}

		public ILabel TotalLevelLabel
		{
			get
			{
				return this._totalLevelLabel;
			}
		}

		public ILabel BattlepassLevelLabel
		{
			get
			{
				return this._battlepassLevelLabel;
			}
		}

		public IDynamicImage AvatarImage
		{
			get
			{
				return this._avatarImage;
			}
		}

		public IDynamicImage AvatarPortraitImage
		{
			get
			{
				return this._avatarPortraitImage;
			}
		}

		public IActivatable AvatarLoadingSpinner
		{
			get
			{
				return this._avatarLoadingSpinner;
			}
		}

		public IButton AvatarChangeButton
		{
			get
			{
				return this._avatarChangeButton;
			}
		}

		public ILabel VictoryNumberLabel
		{
			get
			{
				return this._victoryNumberLabel;
			}
		}

		public ILabel DefeatNumberLabel
		{
			get
			{
				return this._defeatNumberLabel;
			}
		}

		private void Awake()
		{
			if (!this._isFeatureToggled.Check(Features.ProfileRefactor))
			{
				Object.Destroy(this);
				return;
			}
			this._viewProvider.Bind<IProfileView>(this, null);
			this.PreventSummaryTabToggleToStartActive();
		}

		private void PreventSummaryTabToggleToStartActive()
		{
			this._summaryTabUIToggle.startsActive = false;
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IProfileView>(null);
		}

		[Header("Group")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private NGuiButton _backButton;

		[Header("Tabs")]
		[SerializeField]
		private UIToggle _summaryTabUIToggle;

		[SerializeField]
		private NguiToggle _summaryTabToggle;

		[SerializeField]
		private NguiToggle _machinesTabToggle;

		[SerializeField]
		private NguiToggle _matchesTabToggle;

		[Header("Player information")]
		[SerializeField]
		private NGuiLabel _playerNameLabel;

		[SerializeField]
		private NGuiLabel _playerTagLabel;

		[SerializeField]
		private NGuiLabel _totalLevelLabel;

		[SerializeField]
		private NGuiLabel _battlepassLevelLabel;

		[SerializeField]
		private NGuiDynamicTextureOrSprite _avatarImage;

		[SerializeField]
		private NGuiDynamicTextureOrSprite _avatarPortraitImage;

		[SerializeField]
		private GameObjectActivatable _avatarLoadingSpinner;

		[SerializeField]
		private NGuiButton _avatarChangeButton;

		[Header("Victory/defeat statistics")]
		[SerializeField]
		private NGuiLabel _victoryNumberLabel;

		[SerializeField]
		private NGuiLabel _defeatNumberLabel;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;
	}
}
