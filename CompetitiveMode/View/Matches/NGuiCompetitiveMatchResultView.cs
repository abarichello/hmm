using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class NGuiCompetitiveMatchResultView : MonoBehaviour, ICompetitiveMatchResultView
	{
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

		public ICompetitiveMatchResultCalibrationView CalibrationView
		{
			get
			{
				return this._calibrationView;
			}
		}

		public ICompetitiveMatchResultRankedView RankedView
		{
			get
			{
				return this._rankedView;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICompetitiveMatchResultView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveMatchResultView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private NGuiCompetitiveMatchResultCalibrationView _calibrationView;

		[SerializeField]
		private NGuiCompetitiveMatchResultRankedView _rankedView;
	}
}
