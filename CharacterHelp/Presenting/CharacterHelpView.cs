using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterHelp.Presenting
{
	public class CharacterHelpView : MonoBehaviour, ICharacterHelpView
	{
		public IButton ExitButton
		{
			get
			{
				return this._exitButton;
			}
		}

		public IButton DetailsButton
		{
			get
			{
				return this._detailsButton;
			}
		}

		public IButton BackgroundButton
		{
			get
			{
				return this._backgroundButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public ICharacterHelpInfoView InfoView
		{
			get
			{
				return this._characterHelpInfoView;
			}
		}

		public ICharacterHelpGadgetView GadgetPassiveView
		{
			get
			{
				return this._gadgetPassiveView;
			}
		}

		public ICharacterHelpGadgetView GadgetBasicView
		{
			get
			{
				return this._gadgetBasicView;
			}
		}

		public ICharacterHelpGadgetShortcutView GadgetBasicShortcutView
		{
			get
			{
				return this._gadgetBasicShortcutView;
			}
		}

		public ICharacterHelpGadgetView Gadget0View
		{
			get
			{
				return this._gadget0View;
			}
		}

		public ICharacterHelpGadgetShortcutView Gadget0ShortcutView
		{
			get
			{
				return this._gadget0ShortcutView;
			}
		}

		public ICharacterHelpGadgetView GadgetNitroView
		{
			get
			{
				return this._gadgetNitroView;
			}
		}

		public ICharacterHelpGadgetShortcutView GadgetNitroShortcutView
		{
			get
			{
				return this._gadgetNitroShortcutView;
			}
		}

		public ICharacterHelpGadgetView Gadget1View
		{
			get
			{
				return this._gadget1View;
			}
		}

		public ICharacterHelpGadgetShortcutView Gadget1ShortcutView
		{
			get
			{
				return this._gadget1ShortcutView;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<ICharacterHelpView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ICharacterHelpView>(null);
			this._onDestroySubject.OnNext(Unit.Default);
		}

		public void EnableCanvases()
		{
			this._mainCanvas.Enable();
			this._gadgetBasicShortcutView.EnableCanvases();
			this._gadget0ShortcutView.EnableCanvases();
			this._gadgetNitroShortcutView.EnableCanvases();
			this._gadget1ShortcutView.EnableCanvases();
		}

		public void DisableCanvases()
		{
			this._mainCanvas.Disable();
			this._gadgetBasicShortcutView.DisableCanvases();
			this._gadget0ShortcutView.DisableCanvases();
			this._gadgetNitroShortcutView.DisableCanvases();
			this._gadget1ShortcutView.DisableCanvases();
		}

		public void PlayShowAnimation()
		{
			this._mainAnimator.SetBool(CharacterHelpView.ActiveAnimatorProperty, true);
		}

		public void PlayHideAnimation()
		{
			this._mainAnimator.SetBool(CharacterHelpView.ActiveAnimatorProperty, false);
		}

		[UsedImplicitly]
		public void OnHideAnimationEnd()
		{
			this.DisableCanvases();
		}

		public IObservable<Unit> ObserveOnDestroy()
		{
			return this._onDestroySubject;
		}

		private static readonly int ActiveAnimatorProperty = Animator.StringToHash("active");

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private Animator _mainAnimator;

		[SerializeField]
		private UnityButton _exitButton;

		[SerializeField]
		private UnityButton _detailsButton;

		[SerializeField]
		private UnityButton _backgroundButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private CharacterHelpInfoView _characterHelpInfoView;

		[SerializeField]
		private CharacterHelpGadgetView _gadgetPassiveView;

		[SerializeField]
		private CharacterHelpGadgetView _gadgetBasicView;

		[SerializeField]
		private CharacterHelpGadgetShortcutView _gadgetBasicShortcutView;

		[SerializeField]
		private CharacterHelpGadgetView _gadget0View;

		[SerializeField]
		private CharacterHelpGadgetShortcutView _gadget0ShortcutView;

		[SerializeField]
		private CharacterHelpGadgetView _gadgetNitroView;

		[SerializeField]
		private CharacterHelpGadgetShortcutView _gadgetNitroShortcutView;

		[SerializeField]
		private CharacterHelpGadgetView _gadget1View;

		[SerializeField]
		private CharacterHelpGadgetShortcutView _gadget1ShortcutView;

		[Inject]
		private IViewProvider _viewProvider;

		private readonly Subject<Unit> _onDestroySubject = new Subject<Unit>();
	}
}
