using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class NGuiMatchmakingMatchAcceptView : MonoBehaviour, IMatchmakingMatchAcceptView
	{
		public IActivatable MainGroup
		{
			get
			{
				return this._mainGroup;
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

		public IActivatable PlayerDeclinedMessage
		{
			get
			{
				return this._playerDeclinedMessage;
			}
		}

		public IButton AcceptButton
		{
			get
			{
				return this._acceptButton;
			}
		}

		public IButton DeclineButton
		{
			get
			{
				return this._declineButton;
			}
		}

		public IActivatable AcceptButtonParent
		{
			get
			{
				return this._acceptButtonParent;
			}
		}

		public IActivatable DeclineButtonParent
		{
			get
			{
				return this._declineButtonParent;
			}
		}

		public IProgressBar TimeoutProgressBar
		{
			get
			{
				return this._timeoutProgressBar;
			}
		}

		public ILabel TimeoutLabel
		{
			get
			{
				return this._timeoutLabel;
			}
		}

		public void SetPlayerIndexAsAccepted(int playerIndex)
		{
			this._playersAcceptanceIndicators[playerIndex].gameObject.SetActive(true);
			this._playersAcceptanceIndicators[playerIndex].sprite2D = this._acceptedIndicatorImage;
		}

		public void SetPlayerIndexAsDeclined(int playerIndex)
		{
			this._playersAcceptanceIndicators[playerIndex].gameObject.SetActive(true);
			this._playersAcceptanceIndicators[playerIndex].sprite2D = this._declinedIndicatorImage;
		}

		public void DeactivateAllPlayerIndicators()
		{
			for (int i = 0; i < this._playersAcceptanceIndicators.Length; i++)
			{
				this._playersAcceptanceIndicators[i].gameObject.SetActive(false);
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IMatchmakingMatchAcceptView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IMatchmakingMatchAcceptView>(null);
		}

		[SerializeField]
		private GameObjectActivatable _mainGroup;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private GameObjectActivatable _playerDeclinedMessage;

		[SerializeField]
		private UI2DSprite[] _playersAcceptanceIndicators;

		[SerializeField]
		private Sprite _acceptedIndicatorImage;

		[SerializeField]
		private Sprite _declinedIndicatorImage;

		[SerializeField]
		private NGuiButton _acceptButton;

		[SerializeField]
		private NGuiButton _declineButton;

		[SerializeField]
		private GameObjectActivatable _acceptButtonParent;

		[SerializeField]
		private GameObjectActivatable _declineButtonParent;

		[SerializeField]
		private NGuiProgressBar _timeoutProgressBar;

		[SerializeField]
		private NGuiLabel _timeoutLabel;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
