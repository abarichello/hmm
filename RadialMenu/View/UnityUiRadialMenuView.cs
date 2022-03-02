using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.ContextInputNotifier;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.RadialMenu.View
{
	[RequireComponent(typeof(Canvas), typeof(Animator))]
	public class UnityUiRadialMenuView : MonoBehaviour, IRadialMenuView
	{
		private void OnValidate()
		{
			this._mainCanvas = base.GetComponent<Canvas>();
			this._windowAnimator = base.GetComponent<Animator>();
		}

		private void Start()
		{
			this._mainCanvas.enabled = false;
			this.SetupGlowImages();
			this._viewProvider.Bind<IRadialMenuView>(this, null);
		}

		private void SetupGlowImages()
		{
			for (int i = 0; i < this._selectorGlowImages.Length; i++)
			{
				RawImage rawImage = this._selectorGlowImages[i];
				rawImage.SetAlpha(1f);
				rawImage.CrossFadeAlpha(0f, 0f, true);
			}
		}

		private void OnDestroy()
		{
			if (this._viewProvider != null)
			{
				this._viewProvider.Unbind<IRadialMenuView>(null);
			}
		}

		public ITextureMappingUpdater[] SpritesheetAnimators
		{
			get
			{
				return this._animators;
			}
		}

		public RadialMenuMouseNotifier RadialMenuMouseNotifier
		{
			get
			{
				return this._radialMenuMouseNotifier;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IUiNavigationContextInputNotifier UiNavigationContextInputNotifier
		{
			get
			{
				return this._uiNavigationContextInputNotifier;
			}
		}

		public void WindowAnimationIn()
		{
			this._mainCanvas.enabled = true;
			this._windowAnimator.SetBool(UnityUiRadialMenuView.ActiveAnimatorPropertyId, true);
		}

		public void WindowAnimationOut()
		{
			this._windowAnimator.SetBool(UnityUiRadialMenuView.ActiveAnimatorPropertyId, false);
		}

		public void SelectorGlowIn(int index)
		{
			this._selectorGlowImages[index].CrossFadeAlpha(1f, this._selectorGlowInTimeInSeconds, true);
		}

		public void SelectorGlowOut(int index)
		{
			this._selectorGlowImages[index].CrossFadeAlpha(0f, this._selectorGlowOutTimeInSeconds, true);
		}

		[UsedImplicitly]
		public void OnWindowAnimationOutEnd()
		{
			this._mainCanvas.enabled = false;
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private RawImage[] _selectorGlowImages;

		[SerializeField]
		private RadialMenuMouseNotifier _radialMenuMouseNotifier;

		[SerializeField]
		private AnimatedRawImage[] _animators;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationContextInputNotifier _uiNavigationContextInputNotifier;

		[SerializeField]
		private float _selectorGlowInTimeInSeconds = 0.15f;

		[SerializeField]
		private float _selectorGlowOutTimeInSeconds = 0.15f;

		private static readonly int ActiveAnimatorPropertyId = Animator.StringToHash("active");

		[SerializeField]
		[HideInInspector]
		private Canvas _mainCanvas;

		[SerializeField]
		[HideInInspector]
		private Animator _windowAnimator;
	}
}
