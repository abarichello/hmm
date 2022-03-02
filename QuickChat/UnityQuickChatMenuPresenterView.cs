using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.RadialMenu.View;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.ContextInputNotifier;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.QuickChat
{
	public class UnityQuickChatMenuPresenterView : MonoBehaviour, IQuickChatMenuPresenterView
	{
		public IRadialMenuNotifier RadialMenuNotifier
		{
			get
			{
				return this._radialMenuMouseNotifier;
			}
		}

		public IAnimator ContainerAnimator
		{
			get
			{
				return this._containerAnimator;
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

		public void ShowHighlight(int index)
		{
			this._sliceHighlightImages[index].CrossFadeAlpha(1f, this._highlightShowSeconds, true);
		}

		public void HideHighlight(int index)
		{
			this._sliceHighlightImages[index].CrossFadeAlpha(0f, this._highlightHideSeconds, true);
		}

		public void HideAllHighlights()
		{
			for (int i = 0; i < this._sliceHighlightImages.Length; i++)
			{
				this.HideHighlight(i);
			}
		}

		private void Start()
		{
			foreach (RawImage rawImage in this._sliceHighlightImages)
			{
				rawImage.SetAlpha(1f);
				rawImage.CrossFadeAlpha(0f, 0f, true);
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IQuickChatMenuPresenterView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IQuickChatMenuPresenterView>(null);
		}

		[UsedImplicitly]
		public void OnWindowAnimationOutEnd()
		{
		}

		[SerializeField]
		private RawImage[] _sliceHighlightImages;

		[SerializeField]
		private RadialMenuMouseNotifier _radialMenuMouseNotifier;

		[SerializeField]
		private UnityAnimator _containerAnimator;

		[SerializeField]
		private float _highlightShowSeconds = 0.15f;

		[SerializeField]
		private float _highlightHideSeconds = 0.15f;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationContextInputNotifier _uiNavigationContextInputNotifier;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
