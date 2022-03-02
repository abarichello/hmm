using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityCharacterSelectionView : MonoBehaviour, ICharacterSelectionView
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

		public IButton ConfirmPickButton
		{
			get
			{
				return this._confirmPickButton;
			}
		}

		public IButton ConfirmVoteButton
		{
			get
			{
				return this._confirmVoteButton;
			}
		}

		public IButton HideSkinsButton
		{
			get
			{
				return this._hideSkinsButton;
			}
		}

		public IButton ShowSkinsButton
		{
			get
			{
				return this._showSkinsButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IActivatable GenericLoading
		{
			get
			{
				return this._genericLoadingActivatable;
			}
		}

		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICharacterSelectionView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICharacterSelectionView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityButton _confirmVoteButton;

		[SerializeField]
		private UnityButton _confirmPickButton;

		[SerializeField]
		private UnityButton _hideSkinsButton;

		[SerializeField]
		private UnityButton _showSkinsButton;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private GameObjectActivatable _genericLoadingActivatable;
	}
}
