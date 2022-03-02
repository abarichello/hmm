using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation.AxisSelector;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting.View
{
	public class LegacyStartMenuPlayModesView : MonoBehaviour, IStartMenuPlayModesView
	{
		public IActivatable PlayActivatable
		{
			get
			{
				return this._playActivatable;
			}
		}

		public IActivatable WaitingActivatable
		{
			get
			{
				return this._waitingActivatable;
			}
		}

		public IActivatable TimerActivatable
		{
			get
			{
				return this._timerActivatable;
			}
		}

		public ILabel PlayLabel
		{
			get
			{
				return this._playLabel;
			}
		}

		public ILabel WaitingLabel
		{
			get
			{
				return this._waitingLabel;
			}
		}

		public IButton CancelSearchButton
		{
			get
			{
				return this._cancelSearchButton;
			}
		}

		public IUiNavigationAxisSelectorTransformHandler AxisSelectorTransformHandler
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public void UiNavigationSelectionOnPlayButton()
		{
			this._uiNavigationAxisSelector.TryForceSelection(this._playButtonTransform);
		}

		private void Awake()
		{
			this._viewProvider.Bind<IStartMenuPlayModesView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IStartMenuPlayModesView>(null);
		}

		[SerializeField]
		private GameObjectActivatable _playActivatable;

		[SerializeField]
		private GameObjectActivatable _waitingActivatable;

		[SerializeField]
		private GameObjectActivatable _timerActivatable;

		[SerializeField]
		private NGuiLabel _playLabel;

		[SerializeField]
		private NGuiLabel _waitingLabel;

		[SerializeField]
		private NGuiButton _cancelSearchButton;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private Transform _playButtonTransform;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
