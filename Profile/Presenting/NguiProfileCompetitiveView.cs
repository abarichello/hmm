using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.Profile.Presenting;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileCompetitiveView : MonoBehaviour, IProfileCompetitiveView
	{
		public IActivatable Group
		{
			get
			{
				return this._group;
			}
		}

		public IDynamicImage CurrentRankMedalImage
		{
			get
			{
				return this._currentRankMedalImage;
			}
		}

		public ILabel CurrentRankTitleLabel
		{
			get
			{
				return this._currentRankTitleLabel;
			}
		}

		public ILabel CurrentRankScoreLabel
		{
			get
			{
				return this._currentRankScoreLabel;
			}
		}

		public IActivatable CurrentRankPositionGroup
		{
			get
			{
				return this._currentRankPositionGroup;
			}
		}

		public IGrid CurrentRankPositionGrid
		{
			get
			{
				return this._currentRankPositionGrid;
			}
		}

		public ILabel CurrentRankPositionLabel
		{
			get
			{
				return this._currentRankPositionLabel;
			}
		}

		public IDynamicImage HighestRankMedalImage
		{
			get
			{
				return this._highestRankMedalImage;
			}
		}

		public ILabel HighestRankTitleLabel
		{
			get
			{
				return this._highestRankTitleLabel;
			}
		}

		private void Awake()
		{
			if (!this._isFeatureToggled.Check(Features.ProfileRefactor))
			{
				Object.Destroy(this);
				return;
			}
			this._viewProvider.Bind<IProfileCompetitiveView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IProfileCompetitiveView>(null);
		}

		[Header("Group")]
		[SerializeField]
		private GameObjectActivatable _group;

		[Header("Current rank")]
		[SerializeField]
		private NGuiDynamicImage _currentRankMedalImage;

		[SerializeField]
		private NGuiLabel _currentRankTitleLabel;

		[SerializeField]
		private NGuiLabel _currentRankScoreLabel;

		[Header("Current rank position group")]
		[SerializeField]
		private GameObjectActivatable _currentRankPositionGroup;

		[SerializeField]
		private NguiGrid _currentRankPositionGrid;

		[SerializeField]
		private NGuiLabel _currentRankPositionLabel;

		[Header("Highest rank")]
		[SerializeField]
		private NGuiDynamicImage _highestRankMedalImage;

		[SerializeField]
		private NGuiLabel _highestRankTitleLabel;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;
	}
}
