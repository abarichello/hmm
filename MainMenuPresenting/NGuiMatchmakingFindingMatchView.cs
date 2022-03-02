using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class NGuiMatchmakingFindingMatchView : MonoBehaviour, IMatchmakingFindingMatchView
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

		public ILabel ElapsedWaitTimeLabel
		{
			get
			{
				return this._elapsedWaitTimeLabel;
			}
		}

		public ILabel AverageWaitTimeLabel
		{
			get
			{
				return this._averageWaitTimeLabel;
			}
		}

		public IButton CancelSearchButton
		{
			get
			{
				return this._cancelSearchButton;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IMatchmakingFindingMatchView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IMatchmakingFindingMatchView>(null);
		}

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private NGuiLabel _elapsedWaitTimeLabel;

		[SerializeField]
		private NGuiLabel _averageWaitTimeLabel;

		[SerializeField]
		private NGuiButton _cancelSearchButton;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
