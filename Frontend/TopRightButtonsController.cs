using System;
using Assets.Standard_Assets.Scripts.Infra.GUI.Hints;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.Friends.Presenting.FriendsList;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids;
using Hoplon.Input;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class TopRightButtonsController : GameHubBehaviour
	{
		protected void OnEnable()
		{
			this._disposables = new CompositeDisposable();
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
			ChatUiFeedbackDispatcher.EvtPendingMsgCountUpdated += this.onPendingMsgCountUpdated;
			HudWindowManager.Instance.OnNewWindowAdded += this.HudWindowManagerOnNewWindowAdded;
			HudWindowManager.Instance.OnWindowRemoved += this.HudWindowManagerOnWindowRemoved;
			GameHubBehaviour.Hub.GuiScripts.Esc.OnVisibilityChange += this.EscOnOnVisibilityChange;
		}

		private void onPendingMsgCountUpdated()
		{
			int pendingMessagesCount = SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.GetPendingMessagesCount(null);
			this.unReadChatMessagesCountGroup.SetActive(pendingMessagesCount > 0);
			this.unReadChatMessagesCount.text = pendingMessagesCount.ToString();
		}

		protected void OnDisable()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
			ChatUiFeedbackDispatcher.EvtPendingMsgCountUpdated -= this.onPendingMsgCountUpdated;
			GameHubBehaviour.Hub.GuiScripts.Esc.OnVisibilityChange -= this.EscOnOnVisibilityChange;
			if (HudWindowManager.DoesInstanceExist())
			{
				HudWindowManager.Instance.OnNewWindowAdded -= this.HudWindowManagerOnNewWindowAdded;
				HudWindowManager.Instance.OnWindowRemoved -= this.HudWindowManagerOnWindowRemoved;
			}
			this._disposables.Dispose();
		}

		private void ListenToStateChanged(GameState changedstate)
		{
			this._currentChangedstate = changedstate;
			GameState.GameStateKind stateKind = this._currentChangedstate.StateKind;
			if (stateKind == GameState.GameStateKind.Game || stateKind == GameState.GameStateKind.Pick)
			{
				this.TopPlayTween.gameObject.SetActive(false);
			}
			if (stateKind == GameState.GameStateKind.MainMenu)
			{
				this.ObserveInputChange();
			}
			else
			{
				this.DisposeInputChange();
			}
		}

		private void ObserveInputChange()
		{
			this.DisposeInputChange();
			this._inputChangeDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._activeDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), new Action<InputDevice>(this.UpdateButtonsShortcut)));
		}

		private void DisposeInputChange()
		{
			if (this._inputChangeDisposable != null)
			{
				this._inputChangeDisposable.Dispose();
				this._inputChangeDisposable = null;
			}
		}

		public void AnimateEnterMainMenu()
		{
			this.TopPlayTween.tweenGroup = 1;
			this.TopPlayTween.Play();
		}

		public void ShowInviteToGroupPendingEffect()
		{
			this.HasPendingInviteToGroup = true;
			if (this.GroupPendingEffectStateAnimator && this.GroupPendingEffectStateAnimator.gameObject.activeInHierarchy && this.GroupPendingEffectStateAnimator.gameObject.activeSelf)
			{
				this.GroupPendingEffectStateAnimator.SetBool("HasGroupInvite", this.HasPendingInviteToGroup);
			}
		}

		public void HideInviteToGroupPendingEffect()
		{
			this.HasPendingInviteToGroup = false;
			if (this.GroupPendingEffectStateAnimator && this.GroupPendingEffectStateAnimator.gameObject.activeInHierarchy && this.GroupPendingEffectStateAnimator.gameObject.activeSelf)
			{
				this.GroupPendingEffectStateAnimator.SetBool("HasGroupInvite", this.HasPendingInviteToGroup);
			}
		}

		public void onButtonClick_OpenSocialPanel()
		{
			this.OpenSocialPresenter();
			this._buttonBILogger.LogButtonClick(ButtonName.SocialPanel);
		}

		private void OpenSocialPresenter()
		{
			if (this._friendsListPresenter == null)
			{
				this._friendsListPresenter = this._diContainer.Resolve<IFriendsListPresenter>();
				IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this.ShowSocialPresenter());
				this._disposables.Add(disposable);
			}
			else
			{
				IDisposable disposable2 = ObservableExtensions.Subscribe<Unit>(this.ShowSocialPresenter());
				this._disposables.Add(disposable2);
			}
		}

		private IObservable<Unit> ShowSocialPresenter()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (this._friendsListPresenter.IsVisible)
				{
					return Observable.ReturnUnit();
				}
				this.ChatButtonGameObject.GetComponent<BoxCollider>().enabled = false;
				ObservableExtensions.Subscribe<Unit>(this._friendsListPresenter.ObserveHide(), delegate(Unit _)
				{
					this.TryCloseSocialPanel();
				});
				return Observable.Do<Unit>(this._friendsListPresenter.Show(), delegate(Unit _)
				{
					SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
				});
			});
		}

		private void UpdateButtonsShortcut(InputDevice device)
		{
			if (device == 3)
			{
				this._socialShortcutImage.gameObject.SetActive(true);
				this._optionsShortcutImage.gameObject.SetActive(true);
				int inputAction = 50;
				this.UpdateButtonShortcutImage(inputAction, this._optionsShortcutImage);
				inputAction = 19;
				this.UpdateButtonShortcutImage(inputAction, this._socialShortcutImage);
				return;
			}
			this._socialShortcutImage.gameObject.SetActive(false);
			this._optionsShortcutImage.gameObject.SetActive(false);
		}

		private void UpdateButtonShortcutImage(int inputAction, UI2DSprite sprite)
		{
			ISprite sprite2;
			string text;
			if (this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(inputAction, ref sprite2, ref text))
			{
				sprite.sprite2D = (sprite2 as UnitySprite).GetSprite();
			}
		}

		public void onButtonClick_CloseSocialPanel()
		{
			this.TryCloseSocialPanel();
		}

		private void CloseSocialPresenter()
		{
			if (this._friendsListPresenter == null)
			{
				return;
			}
			if (!this._friendsListPresenter.IsVisible)
			{
				return;
			}
			ObservableExtensions.Subscribe<Unit>(this._friendsListPresenter.Hide());
		}

		private void HudWindowManagerOnNewWindowAdded(IHudWindow hudWindow)
		{
			if (hudWindow is SocialModalGUI)
			{
				this.OpenSocialPresenter();
				this.ChatToggleGameObject.SetActive(true);
				this.SetButtonEnabled(false, this.ChatButtonGameObject);
				this.SetButtonEnabled(true, this.ChatToggleGameObject);
			}
		}

		private void HudWindowManagerOnWindowRemoved(IHudWindow hudWindow)
		{
			if (hudWindow is SocialModalGUI)
			{
				this.TryCloseSocialPanel();
			}
		}

		private void TryCloseSocialPanel()
		{
			this.CloseSocialPresenter();
			SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<SocialModalGUI>();
			this.ChatToggleGameObject.SetActive(false);
			this.SetButtonEnabled(true, this.ChatButtonGameObject);
			this.SetButtonEnabled(false, this.ChatToggleGameObject);
		}

		private void SetButtonEnabled(bool isEnabled, GameObject buttonGameObject)
		{
			buttonGameObject.GetComponent<BoxCollider>().enabled = isEnabled;
			UIButton[] components = buttonGameObject.GetComponents<UIButton>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].SetState((!isEnabled) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, true);
			}
		}

		public void onButtonClick_OpenEscMenu()
		{
			GameHubBehaviour.Hub.GuiScripts.Esc.ToggleVisibility();
			this._buttonBILogger.LogButtonClick(ButtonName.EscMenu);
		}

		private void EscOnOnVisibilityChange(bool visible)
		{
			this.TryCloseSocialPanel();
		}

		public void TryCloseAll()
		{
			this.TryCloseSocialPanel();
		}

		[InjectOnClient]
		private IMainMenuGuiProvider _mainMenuGuiProvider;

		protected static readonly BitLogger Log = new BitLogger(typeof(TopRightButtonsController));

		public HMMUIPlayTween TopPlayTween;

		public UIWidget WidgetAlpha;

		[Header("BUTTONS")]
		public UIButton chatoverlay;

		[Header("GROUP")]
		private bool HasPendingInviteToGroup;

		private const string GroupPendingEffectStateAnimatorField = "HasGroupInvite";

		public Animator GroupPendingEffectStateAnimator;

		[Header("FRIENDS")]
		public UILabel unReadChatMessagesCount;

		public GameObject unReadChatMessagesCountGroup;

		[Header("[Chat]")]
		[SerializeField]
		protected GameObject ChatButtonGameObject;

		[SerializeField]
		protected GameObject ChatToggleGameObject;

		[SerializeField]
		private UI2DSprite _socialShortcutImage;

		[SerializeField]
		private UI2DSprite _optionsShortcutImage;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		private MainMenu MainMenu;

		private GameState _currentChangedstate;

		[InjectOnClient]
		private readonly DiContainer _diContainer;

		[InjectOnClient]
		private readonly IConfigLoader _configLoader;

		[InjectOnClient]
		private readonly IInputActiveDeviceChangeNotifier _activeDeviceChangeNotifier;

		[InjectOnClient]
		private readonly IInputTranslation _inputTranslation;

		private IFriendsListPresenter _friendsListPresenter;

		private CompositeDisposable _disposables;

		private IDisposable _inputChangeDisposable;
	}
}
