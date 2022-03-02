using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Social.Teams.Models;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class FriendContextMenuModalGUI : ModalGUIController
	{
		public UserFriend UserFriend
		{
			get
			{
				return this._userFriend;
			}
			set
			{
				this._userFriend = value;
			}
		}

		public void SetParentPanel(UIPanel panel)
		{
			this._parentPanel = panel;
		}

		protected override void InitDialogTasks()
		{
			HudWindowManager.Instance.OnNewWindowAdded += this.OnNewWindowAdded;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.onStateChange;
			base.SetPositionToCurrentContext(this._contextMenuTransform);
		}

		private void OnNewWindowAdded(IHudWindow obj)
		{
			if (obj is PlayerInfoTooltipModalPanel)
			{
				return;
			}
			if (obj is FriendContextMenuModalGUI)
			{
				return;
			}
			base.ResolveModalWindow();
		}

		protected override IEnumerator ResolveModalWindowTasks()
		{
			HudWindowManager.Instance.OnNewWindowAdded -= this.OnNewWindowAdded;
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.onStateChange;
			yield break;
		}

		public void SetInvitePartyOptionButton(Team localUserTeam)
		{
			if (this._userFriend == null)
			{
				FriendContextMenuModalGUI.Log.Error("Unexpected behaviour: null userFriend");
				return;
			}
			HMMHub hub = GameHubBehaviour.Hub;
			bool flag = this._userFriend.IsUserPlayingHMM(localUserTeam);
			string text;
			bool flag2 = this._userFriend.IsUserEligibleToBeInvited(localUserTeam, hub, out text);
			FriendContextMenuModalGUI.Log.DebugFormat("[SocialDebug] User={0} created with reason={1}. Bag={2}", new object[]
			{
				this._userFriend.UniversalId,
				text,
				this._userFriend.Bag
			});
			if (this._inviteToPartyUIButton.gameObject.activeSelf != flag2)
			{
				this._inviteToPartyUIButton.gameObject.SetActive(flag2);
			}
			this._inviteToPartyUIButton.isEnabled = flag2;
			if (!flag)
			{
				return;
			}
			GroupStatus selfGroupStatus = ManagerController.Get<GroupManager>().GetSelfGroupStatus();
			bool flag3 = selfGroupStatus == GroupStatus.Owner || selfGroupStatus == GroupStatus.None;
			string key = (!flag3) ? "Sugerir para o grupo" : "Convidar para o grupo";
			this._inviteParty_Label.text = Language.Get(key, TranslationContext.MainMenuGui);
		}

		protected override void Update()
		{
			base.Update();
			if (Mathf.Approximately(1f, this._parentPanel.alpha))
			{
				return;
			}
			base.ResolveModalWindow();
		}

		public void onButtonClick_InviteToParty()
		{
			if (this._userFriend == null)
			{
				return;
			}
			Debug.Log(string.Format("onButtonClick_InviteToParty: {0} ({1})", this.UserFriend.PlayerName, this.UserFriend.UniversalID));
			if (this.UserFriend == null)
			{
				return;
			}
			ManagerController.Get<GroupManager>().TryInviteToGroup(this.UserFriend, string.Empty);
			base.ResolveModalWindow();
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(78, true);
		}

		public void onButtonClick_SendMessageToUser()
		{
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
			SocialModalGUI.Current.CreateUserChatTab(this.UserFriend, false, false);
			base.ResolveModalWindow();
		}

		public void onButtonClick_Close()
		{
			base.ResolveModalWindow();
		}

		private void onStateChange(GameState gameState)
		{
			if (gameState is Game)
			{
				base.ResolveModalWindow();
			}
		}

		public void onButtonClick_ViewSteamProfile()
		{
			SingletonMonoBehaviour<SocialController>.Instance.OpenSteamPlayerProfile(this.UserFriend.UniversalID);
			base.ResolveModalWindow();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(FriendContextMenuModalGUI));

		[SerializeField]
		private Transform _contextMenuTransform;

		[SerializeField]
		private UIButton _sendMessageUIButton;

		[SerializeField]
		private UIButton _inviteToPartyUIButton;

		[SerializeField]
		private UILabel _inviteParty_Label;

		private UserFriend _userFriend;

		private UIPanel _parentPanel;
	}
}
