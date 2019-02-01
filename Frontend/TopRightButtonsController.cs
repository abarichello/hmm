using System;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.Infra.GUI.Hints;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class TopRightButtonsController : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event TopRightButtonsController.TopRightOpenNewsDelegate TryOpenNewsCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event TopRightButtonsController.TopRightCloseNewsDelegate CloseNewsCallback;

		protected void OnEnable()
		{
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
		}

		private void ListenToStateChanged(GameState pChangedstate)
		{
			this._currentChangedstate = pChangedstate;
			GameState.GameStateKind stateKind = this._currentChangedstate.StateKind;
			if (stateKind == GameState.GameStateKind.Game || stateKind == GameState.GameStateKind.Pick)
			{
				this.TopPlayTween.gameObject.SetActive(false);
			}
		}

		public void AnimateEnterMainMenu()
		{
			this.TopPlayTween.tweenGroup = 1;
			this.TopPlayTween.Play();
		}

		public void QuitApplication()
		{
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenCloseGameConfirmWindow(delegate
			{
				try
				{
					if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
					{
						GameHubBehaviour.Hub.Swordfish.Msg.Cleanup();
					}
				}
				catch (Exception ex)
				{
				}
			});
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
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
		}

		public void onButtonClick_CloseSocialPanel()
		{
			this.TryCloseSocialPanel();
		}

		private void HudWindowManagerOnNewWindowAdded(IHudWindow hudWindow)
		{
			if (hudWindow is SocialModalGUI)
			{
				this.ChatToggleGameObject.SetActive(true);
				this.SetButtonEnabled(false, this.ChatButtonGameObject);
				this.SetButtonEnabled(true, this.ChatToggleGameObject);
				this.TryCloseNews();
			}
		}

		private void HudWindowManagerOnWindowRemoved(IHudWindow hudWindow)
		{
			if (hudWindow is SocialModalGUI)
			{
				this.TryCloseSocialPanel();
			}
			else if (hudWindow is MainMenuNewsModalWindow)
			{
				this.TryToSetNewsButtonEnabled(true);
			}
		}

		private void TryCloseSocialPanel()
		{
			SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<SocialModalGUI>();
			this.ChatToggleGameObject.SetActive(false);
			this.SetButtonEnabled(true, this.ChatButtonGameObject);
			this.SetButtonEnabled(false, this.ChatToggleGameObject);
		}

		public void onButtonClick_OpenHelpWindow()
		{
			OpenUrlUtils.OpenSteamUrl(GameHubBehaviour.Hub, ConfigAccess.SFHelpUrl, string.Format("?lang={0}", Language.CurrentLanguage()), OpenUrlUtils.HardcodedWidth, (int)((float)Screen.height / 100f * 90f), "Heavy Metal Machines");
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.HelpSiteOpenFromTop, true);
			}
		}

		public void onButtonClick_OpenNews()
		{
			this.TryOpenNews(false);
		}

		private void TryOpenNews(bool isAutoOpen)
		{
			if (this.TryOpenNewsCallback != null)
			{
				if (this.TryOpenNewsCallback(isAutoOpen))
				{
					this.NewsToggleGameObject.SetActive(true);
					this.SetButtonEnabled(false, this.NewsButtonGameObject);
					this.SetButtonEnabled(true, this.NewsToggleGameObject);
					SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<SocialModalGUI>();
				}
				else
				{
					this.NewsToggleGameObject.SetActive(false);
					this.SetButtonEnabled(true, this.NewsButtonGameObject);
					this.SetButtonEnabled(false, this.NewsToggleGameObject);
				}
			}
		}

		public void onButtonClick_CloseNews()
		{
			this.TryCloseNews();
		}

		public void TryCloseNews()
		{
			if (this.CloseNewsCallback != null)
			{
				this.CloseNewsCallback();
			}
		}

		public void TryToSetNewsButtonEnabled(bool isEnabled)
		{
			bool isUserInLobby = ManagerController.Get<MatchManager>().IsUserInLobby;
			if (isUserInLobby)
			{
				isEnabled = false;
			}
			this.NewsToggleGameObject.SetActive(false);
			this.SetButtonEnabled(isEnabled, this.NewsButtonGameObject);
			this.SetButtonEnabled(!isEnabled, this.NewsToggleGameObject);
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
		}

		private void EscOnOnVisibilityChange(bool visible)
		{
			this.TryCloseNews();
			this.TryCloseSocialPanel();
		}

		public void TryCloseAll()
		{
			this.TryCloseNews();
			this.NewsToggleGameObject.SetActive(false);
			this.SetButtonEnabled(true, this.NewsButtonGameObject);
			this.SetButtonEnabled(false, this.NewsToggleGameObject);
			this.TryCloseSocialPanel();
		}

		public void TryOpenNewsOnLobbyReturn()
		{
			if (this.ChatToggleGameObject.activeSelf)
			{
				return;
			}
			if (!this.NewsToggleGameObject.activeSelf)
			{
				this.TryOpenNews(true);
			}
		}

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

		[Header("[News]")]
		[SerializeField]
		protected GameObject NewsButtonGameObject;

		[SerializeField]
		protected GameObject NewsToggleGameObject;

		[Header("[Chat]")]
		[SerializeField]
		protected GameObject ChatButtonGameObject;

		[SerializeField]
		protected GameObject ChatToggleGameObject;

		private MainMenu MainMenu;

		private GameState _currentChangedstate;

		public delegate bool TopRightOpenNewsDelegate(bool isAutoOpen);

		public delegate void TopRightCloseNewsDelegate();
	}
}
