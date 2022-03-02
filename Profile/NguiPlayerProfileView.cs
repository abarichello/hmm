using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Profile
{
	public class NguiPlayerProfileView : MonoBehaviour, IPlayerProfileView
	{
		public IActivatable MainGroup
		{
			get
			{
				return this._mainGroup;
			}
		}

		public ILabel CurrentRankLabel
		{
			get
			{
				return this._currentRankLabel;
			}
		}

		public ILabel CurrentScoreLabel
		{
			get
			{
				return this._currentScoreLabel;
			}
		}

		public IActivatable CurrentTopPlacementPositionGroup
		{
			get
			{
				return this._currentTopPlacementPositionGroup;
			}
		}

		public ILabel CurrentTopPlacementPositionLabel
		{
			get
			{
				return this._currentTopPlacementPositionLabel;
			}
		}

		public IDynamicImage CurrentRankDynamicImage
		{
			get
			{
				return this._currentRankDynamicImage;
			}
		}

		public ILabel TooltipCurrentRankLabel
		{
			get
			{
				return this._tooltipCurrentRankLabel;
			}
		}

		public IDynamicImage TooltipCurrentRankDynamicImage
		{
			get
			{
				return this._tooltipCurrentRankDynamicImage;
			}
		}

		public ILabel TooltipTopRankLabel
		{
			get
			{
				return this._tooltipTopRankLabel;
			}
		}

		public IGrid CurrentRankGrid
		{
			get
			{
				return this._currentRankCurrentRankGrid;
			}
		}

		public IDynamicImage TooltipTopRankDynamicImage
		{
			get
			{
				return this._tooltipTopRankDynamicImage;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IPlayerProfileView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IPlayerProfileView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private GameObjectActivatable _mainGroup;

		[SerializeField]
		private NGuiLabel _currentRankLabel;

		[SerializeField]
		private NGuiDynamicImage _currentRankDynamicImage;

		[SerializeField]
		private NGuiLabel _currentScoreLabel;

		[SerializeField]
		private GameObjectActivatable _currentTopPlacementPositionGroup;

		[SerializeField]
		private NGuiLabel _currentTopPlacementPositionLabel;

		[SerializeField]
		private NGuiLabel _tooltipCurrentRankLabel;

		[SerializeField]
		private NGuiDynamicImage _tooltipCurrentRankDynamicImage;

		[SerializeField]
		private NGuiLabel _tooltipTopRankLabel;

		[SerializeField]
		private NGuiDynamicImage _tooltipTopRankDynamicImage;

		[SerializeField]
		private NguiGrid _currentRankCurrentRankGrid;
	}
}
