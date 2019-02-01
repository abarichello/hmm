using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Matchmaking;
using ClientAPI.Objects;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class GroupInviteContextMenu : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event GroupInviteContextMenu.HandleInviteButtonDelegate onHandleInviteButton;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event GroupInviteContextMenu.RefreshEligibleFriendsButtonDelegate onRefreshEligibleFriendsButton;

		public int RefreshEligibleFriendsToInvite()
		{
			foreach (KeyValuePair<string, FriendGuiItem> keyValuePair in SocialModalGUI.Current.FriendGuiItemByIDDictionary)
			{
				UserFriend referenceObject = keyValuePair.Value.ReferenceObject;
				bool flag = referenceObject.IsUserEligibleToBeInvited(null);
				if (this._activeFriendGuiItems.ContainsKey(referenceObject.UniversalID))
				{
					if (!flag)
					{
						UnityEngine.Object.Destroy(this._activeFriendGuiItems[referenceObject.UniversalID].gameObject);
						this._activeFriendGuiItems.Remove(referenceObject.UniversalID);
					}
				}
				else if (flag)
				{
					BaseGUIItem<FriendGuiItem, UserFriend> value = keyValuePair.Value;
					UserFriend referenceObject2 = referenceObject;
					Transform transform = this._eligibleFriendsToInviteTable.transform;
					FriendGuiItem friendGuiItem = value.CreateNewGuiItem(referenceObject2, true, transform);
					friendGuiItem.GetComponent<UIWidget>().color = Color.white;
					friendGuiItem.StatusLabel.gameObject.SetActive(false);
					friendGuiItem.ParentPanel = this._panel;
					friendGuiItem.SetParentScrollView(this.FriendListScrollView);
					this._activeFriendGuiItems.Add(referenceObject.UniversalID, friendGuiItem);
				}
			}
			this._eligibleFriendsToInviteTable.repositionNow = true;
			if (this._activeFriendGuiItems.Count == 0)
			{
				this.onButtonClick_Close();
			}
			return this._activeFriendGuiItems.Count;
		}

		private void Start()
		{
			HudWindowManager.Instance.OnNewWindowAdded += this.OnNewWindowAdded;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.onStateChange;
			ManagerController.Get<FriendManager>().EvtFriendListUpdated += this.OnFriendListUpdated;
			ManagerController.Get<FriendManager>().EvtFriendBagUpdated += this.OnFriendBagUpdated;
			ManagerController.Get<GroupManager>().OnGroupUpdate += this.CheckIfHaveFriendsToInviteToParty;
			this._hub = GameHubBehaviour.Hub;
			this._hub.StartCoroutine(this.InitSfCoRoutine());
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private IEnumerator InitSfCoRoutine()
		{
			while (GameHubBehaviour.Hub.Swordfish.Connection == null)
			{
				yield return null;
			}
			GameHubBehaviour.Hub.Swordfish.Connection.ListenToSwordfishConnected += this.OnSwordfishConnected;
			if (GameHubBehaviour.Hub.Swordfish.Connection.Connected)
			{
				this.OnSwordfishConnected();
			}
			yield break;
		}

		protected virtual void OnSwordfishConnected()
		{
			this._SetSfEventsSubscriptionState(true);
		}

		private void _SetSfEventsSubscriptionState(bool targetState)
		{
			if (targetState == this._subscribedToSfEvents)
			{
				return;
			}
			this.SetSfEventsSubscriptionState(targetState);
			this._subscribedToSfEvents = targetState;
		}

		private void SetSfEventsSubscriptionState(bool targetState)
		{
			if (targetState)
			{
				this._hub.Swordfish.Msg.Matchmaking.OnClientConnectedEvent += this.CheckIfHaveFriendsToInviteToParty;
				this._hub.Swordfish.Msg.Matchmaking.OnClientDisconnectedEvent += this.CheckIfHaveFriendsToInviteToParty;
				this._hub.ClientApi.matchmakingClient.MatchMade += this.OnMatchmakingMade;
				this._hub.ClientApi.matchmakingClient.MatchStarted += this.OnMatchmakingStarted;
				this._hub.ClientApi.matchmakingClient.MatchConfirmed += this.OnMatchConfirmed;
				this._hub.ClientApi.matchmakingClient.MatchCanceled += this.OnMatchmakingCanceled;
				this._hub.ClientApi.matchmakingClient.MatchAccepted += this.OnMatchmakingAccepted;
				return;
			}
			if (this._hub == null)
			{
				return;
			}
			if (this._hub.Swordfish.Msg != null && this._hub.Swordfish.Msg.Matchmaking != null)
			{
				this._hub.Swordfish.Msg.Matchmaking.OnClientConnectedEvent -= this.CheckIfHaveFriendsToInviteToParty;
				this._hub.Swordfish.Msg.Matchmaking.OnClientDisconnectedEvent -= this.CheckIfHaveFriendsToInviteToParty;
			}
			if (this._hub.ClientApi != null)
			{
				this._hub.ClientApi.matchmakingClient.MatchMade -= this.OnMatchmakingMade;
				this._hub.ClientApi.matchmakingClient.MatchStarted -= this.OnMatchmakingStarted;
				this._hub.ClientApi.matchmakingClient.MatchConfirmed -= this.OnMatchConfirmed;
				this._hub.ClientApi.matchmakingClient.MatchCanceled -= this.OnMatchmakingCanceled;
				this._hub.ClientApi.matchmakingClient.MatchAccepted -= this.OnMatchmakingAccepted;
			}
		}

		private void OnDestroy()
		{
			if (HudWindowManager.DoesInstanceExist())
			{
				HudWindowManager.Instance.OnNewWindowAdded -= this.OnNewWindowAdded;
			}
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.onStateChange;
			FriendManager friendManager = ManagerController.Get<FriendManager>();
			if (friendManager != null)
			{
				friendManager.EvtFriendListUpdated -= this.OnFriendListUpdated;
				friendManager.EvtFriendBagUpdated -= this.OnFriendBagUpdated;
			}
			GroupManager groupManager = ManagerController.Get<GroupManager>();
			if (groupManager != null)
			{
				groupManager.OnGroupUpdate -= this.CheckIfHaveFriendsToInviteToParty;
			}
			this.SetSfEventsSubscriptionState(false);
		}

		public void onButtonClick_Close()
		{
			this._backgroundWidget.gameObject.SetActive(false);
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
			this.onButtonClick_Close();
		}

		private void onStateChange(GameState gameState)
		{
			if (gameState is Game)
			{
				this.onButtonClick_Close();
			}
		}

		private void CheckIfHaveFriendsToInviteToParty()
		{
			this.RefreshEligibleFriendsToInvite();
			if (this._activeFriendGuiItems.Count > 0)
			{
				this.haveEligibleFrindsToInvite = true;
			}
			else
			{
				this.haveEligibleFrindsToInvite = false;
			}
			if (GroupInviteContextMenu.onHandleInviteButton != null)
			{
				GroupInviteContextMenu.onHandleInviteButton(this.haveEligibleFrindsToInvite);
			}
		}

		public void onButtonClick_OpenGroupInviteContextMenu()
		{
			this._backgroundWidget.gameObject.SetActive(true);
			if (GroupInviteContextMenu.onRefreshEligibleFriendsButton != null)
			{
				GroupInviteContextMenu.onRefreshEligibleFriendsButton();
			}
		}

		private void OnClientDisconnectedEvent()
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private void OnClientConnectedEvent()
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private void OnMatchmakingAccepted(object sender, MatchAcceptedArgs e)
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private void OnMatchmakingCanceled(object sender, MatchCancelledArgs e)
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private void OnMatchConfirmed(object sender, MatchmakingEventArgs e)
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private void OnMatchmakingStarted(object sender, MatchStartedEventArgs e)
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private void OnMatchmakingMade(object sender, MatchmakingEventArgs e)
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private void OnFriendListUpdated(FriendManager friendmanager)
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		private void OnFriendBagUpdated(UserFriend obj)
		{
			this.CheckIfHaveFriendsToInviteToParty();
		}

		public UIScrollView FriendListScrollView;

		[Header("Anchors Order: Left, Right, Bottom, Top")]
		[SerializeField]
		private UIWidget _backgroundWidget;

		[SerializeField]
		private Transform _contextMenuTransform;

		[SerializeField]
		private UITable _eligibleFriendsToInviteTable;

		[SerializeField]
		private UIPanel _panel;

		private Dictionary<string, FriendGuiItem> _activeFriendGuiItems = new Dictionary<string, FriendGuiItem>();

		private HMMHub _hub;

		private bool _subscribedToSfEvents;

		private bool haveEligibleFrindsToInvite;

		public delegate void HandleInviteButtonDelegate(bool activeButton);

		public delegate void RefreshEligibleFriendsButtonDelegate();
	}
}
