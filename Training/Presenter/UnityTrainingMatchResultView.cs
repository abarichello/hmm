using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;

namespace HeavyMetalMachines.Training.Presenter
{
	public class UnityTrainingMatchResultView : MonoBehaviour, ITrainingMatchResultView
	{
		public ICanvas Canvas
		{
			get
			{
				return this._canvas;
			}
		}

		public IDynamicImage BackgroundImage
		{
			get
			{
				return this._backgroundImage;
			}
		}

		public IAnimation ShowAnimationVictory
		{
			get
			{
				return this._showAnimationVictory;
			}
		}

		public IAnimation ShowAnimationDefeat
		{
			get
			{
				return this._showAnimationDefeat;
			}
		}

		public IAnimation HideAnimation
		{
			get
			{
				return this._hideAnimation;
			}
		}

		public IButton GoBackToMainMenuButton
		{
			get
			{
				return this._goBackToMainMenuButton;
			}
		}

		public ILabel ArenaName
		{
			get
			{
				return this._arenaName;
			}
		}

		public IDynamicImage IconArena
		{
			get
			{
				return this._iconArena;
			}
		}

		public IDynamicImage IconGlow
		{
			get
			{
				return this._iconGlow;
			}
		}

		public IDynamicImage AssertIcon
		{
			get
			{
				return this._assertIcon;
			}
		}

		public IDynamicImage AssertIconGlow
		{
			get
			{
				return this._assertIconGlow;
			}
		}

		public ILabel MatchConcludeLabel
		{
			get
			{
				return this._matchConcludeLabel;
			}
		}

		public Color WinColor
		{
			get
			{
				return this._winColor.ToHmmColor();
			}
		}

		public Color LoseColor
		{
			get
			{
				return this._loseColor.ToHmmColor();
			}
		}

		public string MatchWinDraft
		{
			get
			{
				return this._matchWinDraft;
			}
		}

		public string MatchLoseDraft
		{
			get
			{
				return this._matchLoseDraft;
			}
		}

		public string AssertIconMatchWonName
		{
			get
			{
				return this._assertIconMatchWonName;
			}
		}

		public string AssertIconMatchLoseName
		{
			get
			{
				return this._assertIconMatchLoseName;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ITrainingMatchResultView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ITrainingMatchResultView>(null);
			this._uiNavigationGroupHolder.RemoveGroup();
		}

		[InjectOnClient]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _canvas;

		[SerializeField]
		private UnityDynamicImage _backgroundImage;

		[SerializeField]
		private UnityAnimation _showAnimationVictory;

		[SerializeField]
		private UnityAnimation _showAnimationDefeat;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityButton _goBackToMainMenuButton;

		[SerializeField]
		private UnityLabel _arenaName;

		[SerializeField]
		private UnityDynamicImage _iconArena;

		[SerializeField]
		private UnityDynamicImage _iconGlow;

		[SerializeField]
		private UnityDynamicImage _assertIcon;

		[SerializeField]
		private UnityDynamicImage _assertIconGlow;

		[SerializeField]
		private UnityLabel _matchConcludeLabel;

		[SerializeField]
		private Color _winColor;

		[SerializeField]
		private Color _loseColor;

		[SerializeField]
		private string _matchWinDraft;

		[SerializeField]
		private string _matchLoseDraft;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private string _assertIconMatchWonName;

		[SerializeField]
		private string _assertIconMatchLoseName;
	}
}
