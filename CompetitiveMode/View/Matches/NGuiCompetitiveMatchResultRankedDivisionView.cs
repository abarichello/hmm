using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class NGuiCompetitiveMatchResultRankedDivisionView : MonoBehaviour, ICompetitiveMatchResultRankedDivisionView
	{
		public IActivatable Group
		{
			get
			{
				return this._group;
			}
		}

		public IAnimation DivisionUpAnimation
		{
			get
			{
				return this._divisionUpAnimation;
			}
		}

		public IAnimation DivisionDownAnimation
		{
			get
			{
				return this._divisionDownAnimation;
			}
		}

		public IAnimation SubdivisionUpAnimation
		{
			get
			{
				return this._subdivisionUpAnimation;
			}
		}

		public IAnimation SubdivisionDownAnimation
		{
			get
			{
				return this._subdivisionDownAnimation;
			}
		}

		public IAnimation IdleAnimation
		{
			get
			{
				return this._idleAnimation;
			}
		}

		public IAnimation GlowAnimation
		{
			get
			{
				return this._glowAnimation;
			}
		}

		public IDynamicImage AnimatedDivisionImage
		{
			get
			{
				return this._divisionImage;
			}
		}

		public IDynamicImage StaticDivisionImage
		{
			get
			{
				return this._previousDivisionImage;
			}
		}

		public IDynamicImage CurrentSubdivisionImage
		{
			get
			{
				return this._subdivisionImage;
			}
		}

		public IDynamicImage PreviousSubdivisionImage
		{
			get
			{
				return this._previousSubdivisionImage;
			}
		}

		public ILabel PreviousTopPlacementLabel
		{
			get
			{
				return this._previousTopPlacementLabel;
			}
		}

		public ILabel TopPlacementLabel
		{
			get
			{
				return this._topPlacementLabel;
			}
		}

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private UnityAnimation _divisionUpAnimation;

		[SerializeField]
		private UnityAnimation _divisionDownAnimation;

		[SerializeField]
		private UnityAnimation _subdivisionUpAnimation;

		[SerializeField]
		private UnityAnimation _subdivisionDownAnimation;

		[SerializeField]
		private UnityAnimation _idleAnimation;

		[SerializeField]
		private UnityAnimation _glowAnimation;

		[SerializeField]
		private NGuiDynamicImage _divisionImage;

		[SerializeField]
		private NGuiDynamicImage _previousDivisionImage;

		[SerializeField]
		private NGuiDynamicImage _subdivisionImage;

		[SerializeField]
		private NGuiDynamicImage _previousSubdivisionImage;

		[SerializeField]
		private NGuiLabel _previousTopPlacementLabel;

		[SerializeField]
		private NGuiLabel _topPlacementLabel;
	}
}
