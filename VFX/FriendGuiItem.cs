using System;
using System.Linq;
using System.Text;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class FriendGuiItem : BaseGUIItem<FriendGuiItem, UserFriend>
	{
		public UILabel StatusLabel
		{
			get
			{
				return this._statusLabel;
			}
		}

		private void OnEnable()
		{
			this._hub = GameHubBehaviour.Hub;
			ManagerController.Get<GroupManager>().OnGroupUpdate += this.UpdateInviteEligibility;
			ManagerController.Get<FriendManager>().EvtFriendListUpdated += this.OnFriendListUpdated;
			ManagerController.Get<FriendManager>().EvtFriendRefresh += this.OnFriendRefreshed;
			GroupInviteContextMenu.onRefreshEligibleFriendsButton += this.UpdateInviteEligibility;
		}

		private void OnFriendRefreshed(UserFriend friend)
		{
			if (!friend.UniversalID.Equals(base.ReferenceObject.UniversalId))
			{
				return;
			}
			if (friend.Status == FriendStatus.Pending)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			base.SetProperties(friend);
		}

		private void OnFriendListUpdated(FriendManager friendManager)
		{
			this.UpdateInviteEligibility();
		}

		private void OnDisable()
		{
			if (SingletonMonoBehaviour<SocialController>.DoesInstanceExist())
			{
				if (ManagerController.Get<GroupManager>() != null)
				{
					ManagerController.Get<GroupManager>().OnGroupUpdate -= this.UpdateInviteEligibility;
				}
				if (ManagerController.Get<FriendManager>() != null)
				{
					ManagerController.Get<FriendManager>().EvtFriendListUpdated -= this.OnFriendListUpdated;
					ManagerController.Get<FriendManager>().EvtFriendRefresh -= this.OnFriendRefreshed;
				}
			}
			GroupInviteContextMenu.onRefreshEligibleFriendsButton -= this.UpdateInviteEligibility;
			if (this._playerInfoTooltipModalPanel != null)
			{
				this._playerInfoTooltipModalPanel.ForceResolveModalWindow(false);
				this._playerInfoTooltipModalPanel = null;
			}
		}

		protected override void SetPropertiesTasks(UserFriend userFriend)
		{
			this.UpdateInviteEligibility();
			this._nameLabel.text = NGUIText.EscapeSymbols(userFriend.PlayerName);
			SocialConfig socialConfiguration = SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration;
			switch (userFriend.State)
			{
			case FriendState.Offline:
				this._statusLabel.color = socialConfiguration.OfflineColor;
				this._statusLabel.text = Language.Get("OFFLINE", TranslationSheets.Friends);
				goto IL_2B4;
			case FriendState.Away:
			case FriendState.Snooze:
				this._statusLabel.color = socialConfiguration.AwayColor;
				this._statusLabel.text = Language.Get("AWAY", TranslationSheets.Friends);
				goto IL_2B4;
			case FriendState.PlayingGame:
			{
				FriendHolder friendHolder = ManagerController.Get<FriendManager>().GetFriendHolder(userFriend);
				this._statusLabel.color = socialConfiguration.PlayingHmmColor;
				if (!friendHolder.IsFriendBagLoaded)
				{
					string text = Language.Get("Loading_loading", TranslationSheets.Loading);
					this._statusLabel.text = text.First<char>().ToString().ToUpper() + text.Substring(1);
					goto IL_2B4;
				}
				FriendBag friendBag = ManagerController.Get<FriendManager>().GetFriendBag(userFriend);
				if (friendBag.IsSpectator)
				{
					this._statusLabel.text = Language.Get("ISNARRATOR", TranslationSheets.Friends);
					goto IL_2B4;
				}
				string portraitItemTypeId = friendBag.PortraitItemTypeId;
				if (!string.IsNullOrEmpty(portraitItemTypeId))
				{
					PortraitDecoratorGui.UpdatePortraitSprite(new Guid(portraitItemTypeId), this._borderDynamicSprite, PortraitDecoratorGui.PortraitSpriteType.MainMenuGroupIcon);
				}
				bool flag = friendBag.IsInMatchOrQueue();
				StringBuilder stringBuilder = new StringBuilder(string.Empty);
				PlotKidsExtensions.WriteFriendStatus(stringBuilder, friendBag);
				HeavyMetalMachines.Character.CharacterInfo characterInfo;
				if (friendBag.HudWindowManagerState == 3 && friendBag.GameMapIndex != 2 && this._hub != null && this._hub.InventoryColletion.IsItemTypesCollectionReady && this._hub.InventoryColletion.AllCharactersByInfoId.TryGetValue(friendBag.LastUsedCharacterId, out characterInfo))
				{
					stringBuilder.Append(" - " + string.Format(Language.Get("PLAYER_SOCIALMENU_CURRENT_CHARACTER", TranslationSheets.Help), characterInfo.LocalizedName));
				}
				this._statusLabel.text = stringBuilder.ToString();
				if (flag)
				{
					this._statusLabel.color = socialConfiguration.PlayingHmmMatchColor;
				}
				goto IL_2B4;
			}
			case FriendState.PlayingOtherGame:
				this._statusLabel.color = socialConfiguration.PlayingAnotherGameColor;
				this._statusLabel.text = Language.Get("PLAYINGANOTHERGAME", TranslationSheets.Friends);
				goto IL_2B4;
			}
			this._statusLabel.color = Color.green;
			this._statusLabel.text = Language.Get("ONLINE", TranslationSheets.Friends);
			IL_2B4:
			if (!this._iconLoader.IsLoaded)
			{
				this._iconLoader.UpdatePlayerIcon(userFriend.UniversalID);
			}
		}

		public void AssignParentFilter(FriendFilterGuiItem newParentFilter)
		{
			if (this._parentFilterGuiItem != null)
			{
				if (newParentFilter == this._parentFilterGuiItem)
				{
					if (newParentFilter.IsEnabled != base.gameObject.activeSelf)
					{
						base.gameObject.SetActive(newParentFilter.IsEnabled);
					}
					return;
				}
				this._parentFilterGuiItem.CurrentFriendCount--;
			}
			this._parentFilterGuiItem = newParentFilter;
			this._parentFilterGuiItem.CurrentFriendCount++;
			int num = (!base.ReferenceObject.IsUserAvailableToBeInvitedToGroup(this._hub)) ? 1 : 0;
			if (base.ReferenceObject.State == FriendState.PlayingGame)
			{
				num = ((!base.ReferenceObject.IsUserInMatchOrQueue()) ? num : 2);
			}
			base.gameObject.name = string.Format("{0}_{1}_{2}_{3}", new object[]
			{
				newParentFilter.gameObject.name,
				num,
				base.ReferenceObject.PlayerName,
				base.ReferenceObject.PlayerId
			});
			base.gameObject.SetActive(newParentFilter.IsEnabled);
		}

		public void OnDoubleClick()
		{
			if (UICamera.currentTouchID != -1)
			{
				return;
			}
			this.OpenChatTab(base.ReferenceObject);
		}

		public void OpenChatTab(UserFriend userFriend)
		{
			this._parentUI.CreateUserChatTab(userFriend, false, false);
		}

		public void OnClick()
		{
			if (UICamera.currentTouchID != -2)
			{
				return;
			}
			FriendContextMenuModalGUI friendContextMenuModalGUI;
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<FriendContextMenuModalGUI>(out friendContextMenuModalGUI);
			friendContextMenuModalGUI.SetParentPanel(this.ParentPanel);
			friendContextMenuModalGUI.UserFriend = base.ReferenceObject;
		}

		public void onButtonClick_SendInvite()
		{
			ManagerController.Get<GroupManager>().TryInviteToGroup(base.ReferenceObject);
		}

		private void UpdateInviteEligibility()
		{
			if (this.SendInviteButton == null || this.SendInviteButton.gameObject == null)
			{
				return;
			}
			bool flag = base.ReferenceObject.State == FriendState.PlayingGame;
			if (this.SendInviteButton.gameObject.activeSelf != flag)
			{
				this.SendInviteButton.gameObject.SetActive(flag);
			}
			if (!flag)
			{
				return;
			}
			bool flag2 = base.ReferenceObject.IsUserEligibleToBeInvited(null);
			if (this.SendInviteButton.isEnabled != flag2)
			{
				this.SendInviteButton.isEnabled = flag2;
			}
		}

		public void SetIfCanShowTooltip(bool canShowTooltip)
		{
			this._canShowTooltip = canShowTooltip;
		}

		private void OnTooltip(bool isOver)
		{
			if (!this._canShowTooltip)
			{
				return;
			}
			if (base.ReferenceObject.State != FriendState.PlayingGame || !isOver)
			{
				this._playerInfoTooltipModalPanel = null;
				SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<PlayerInfoTooltipModalPanel>();
				return;
			}
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<PlayerInfoTooltipModalPanel>(out this._playerInfoTooltipModalPanel);
			if (this._playerInfoTooltipModalPanel == null)
			{
				Debug.LogError("[FriendGUIItem] Player Tooltip panel is null! This isn't supposed to happen. Aborting tooltip...");
				return;
			}
			this._playerInfoTooltipModalPanel.ParentGUI = this._parentUI;
			this._playerInfoTooltipModalPanel.SetUserFriend(base.ReferenceObject);
		}

		private void OnPress()
		{
			SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<PlayerInfoTooltipModalPanel>();
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(FriendGuiItem));

		private FriendFilterGuiItem _parentFilterGuiItem;

		[SerializeField]
		private SteamIconLoader _iconLoader;

		[SerializeField]
		private UILabel _nameLabel;

		[SerializeField]
		private UILabel _statusLabel;

		[SerializeField]
		private HMMUI2DDynamicSprite _borderDynamicSprite;

		[NonSerialized]
		public UIPanel ParentPanel;

		[SerializeField]
		private SocialModalGUI _parentUI;

		public UIButton SendInviteButton;

		private HMMHub _hub;

		private PlayerInfoTooltipModalPanel _playerInfoTooltipModalPanel;

		private bool _canShowTooltip = true;
	}
}
