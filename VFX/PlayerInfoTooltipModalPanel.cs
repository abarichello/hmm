using System;
using System.Collections;
using System.Text;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class PlayerInfoTooltipModalPanel : ModalGUIController
	{
		protected override void InitDialogTasks()
		{
			HudWindowManager.Instance.OnNewWindowAdded += this.OnNewWindowAdded;
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
			yield break;
		}

		public void SetUserFriend(UserFriend userFriend)
		{
			this._hub = GameHubBehaviour.Hub;
			this._playerNameLabel.text = userFriend.PlayerName;
			this._iconLoader.UpdatePlayerIcon(userFriend.UniversalID);
			FriendBag friendBag = ManagerController.Get<FriendManager>().GetFriendBag(userFriend);
			StringBuilder stringBuilder = new StringBuilder();
			if (friendBag != null)
			{
				this._playerLevelLabel.text = (friendBag.Level + 1).ToString("0");
				stringBuilder.AppendLine(string.Format("[{0}]{1}[-] [{2}]{3} {4}[-]", new object[]
				{
					HudUtils.RGBToHex(Color.white),
					Language.Get("PLAYER_TOOLTIP_MATCHES_WON", TranslationSheets.Help),
					HudUtils.RGBToHex(this._usedCharacterColor),
					friendBag.MatchesWon,
					Language.Get("PLAYER_TOOLTIP_MATCHES_WON_SUFFIX", TranslationSheets.Help)
				}));
				long num = Convert.ToInt64((DateTime.UtcNow - new DateTime(friendBag.LastMatchStartedTime)).TotalMinutes);
				HeavyMetalMachines.Character.CharacterInfo characterInfo;
				if (this._hub.InventoryColletion.AllCharactersByInfoId.TryGetValue(friendBag.LastUsedCharacterId, out characterInfo))
				{
					string key = (friendBag.HudWindowManagerState != 3) ? "PLAYER_TOOLTIP_LAST_CHARACTER" : "PLAYER_TOOLTIP_CURRENT_CHARACTER";
					stringBuilder.AppendLine(string.Format(Language.Get(key, TranslationSheets.Help), HudUtils.RGBToHex(this._usedCharacterColor), characterInfo.LocalizedName, num));
				}
				PlotKidsExtensions.WriteFriendStatus(stringBuilder, friendBag);
			}
			else
			{
				stringBuilder.AppendLine("\n\n\n\n");
			}
			this._playerInfoLabel.text = stringBuilder.ToString();
		}

		private Transform _contextMenuTransform;

		[SerializeField]
		private UILabel _playerNameLabel;

		[SerializeField]
		private UILabel _playerInfoLabel;

		[SerializeField]
		private SteamIconLoader _iconLoader;

		[SerializeField]
		private Color _infoColor;

		[SerializeField]
		private Color _usedCharacterColor;

		[SerializeField]
		private UILabel _playerLevelLabel;

		[NonSerialized]
		public SocialModalGUI ParentGUI;

		private HMMHub _hub;
	}
}
