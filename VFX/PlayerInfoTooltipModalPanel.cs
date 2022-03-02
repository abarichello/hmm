using System;
using System.Collections;
using System.Text;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Friends.GUI;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using Hoplon.Serialization;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public class PlayerInfoTooltipModalPanel : ModalGUIController
	{
		private void OnDisable()
		{
			this.TryDisposeLoadRankDisposable();
		}

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
			if (userFriend.IsRegisteredOnServer)
			{
				this.LoadRank(userFriend.PlayerId);
			}
			FriendBag friendBag = (FriendBag)((JsonSerializeable<!0>)userFriend.Bag);
			StringBuilder stringBuilder = new StringBuilder();
			if (friendBag != null)
			{
				this._playerLevelLabel.text = (friendBag.Level + 1).ToString("0");
				stringBuilder.AppendLine(string.Format("[{0}]{1}[-] [{2}]{3} {4}[-]", new object[]
				{
					HudUtils.RGBToHex(Color.white),
					Language.Get("PLAYER_TOOLTIP_MATCHES_WON", TranslationContext.Help),
					HudUtils.RGBToHex(this._usedCharacterColor),
					friendBag.MatchesWon,
					Language.Get("PLAYER_TOOLTIP_MATCHES_WON_SUFFIX", TranslationContext.Help)
				}));
				long num = Convert.ToInt64((DateTime.UtcNow - new DateTime(friendBag.LastMatchStartedTime)).TotalMinutes);
				IItemType itemType;
				if (this._hub.InventoryColletion.AllCharactersByCharacterId.TryGetValue(friendBag.LastUsedCharacterId, out itemType))
				{
					string characterLocalizedName = itemType.GetComponent<CharacterItemTypeComponent>().GetCharacterLocalizedName();
					string key = (friendBag.HudWindowManagerState != 3) ? "PLAYER_TOOLTIP_LAST_CHARACTER" : "PLAYER_TOOLTIP_CURRENT_CHARACTER";
					stringBuilder.AppendLine(Language.GetFormatted(key, TranslationContext.Help, new object[]
					{
						HudUtils.RGBToHex(this._usedCharacterColor),
						characterLocalizedName,
						num
					}));
				}
				PlotKidsExtensions.WriteFriendStatus(stringBuilder, friendBag);
			}
			else
			{
				stringBuilder.AppendLine("\n\n\n\n");
			}
			this._playerInfoLabel.text = stringBuilder.ToString();
		}

		private void LoadRank(long playerId)
		{
			this.TryDisposeLoadRankDisposable();
			this._loadRankDisposable = ObservableExtensions.Subscribe<Unit>(this._friendTooltipRankPresenter.LoadRank(playerId));
		}

		private void TryDisposeLoadRankDisposable()
		{
			if (this._loadRankDisposable == null)
			{
				return;
			}
			this._loadRankDisposable.Dispose();
			this._loadRankDisposable = null;
		}

		[Inject]
		private IFriendTooltipRankPresenter _friendTooltipRankPresenter;

		private Transform _contextMenuTransform;

		[SerializeField]
		private UILabel _playerNameLabel;

		[SerializeField]
		private UILabel _playerInfoLabel;

		[SerializeField]
		private Color _infoColor;

		[SerializeField]
		private Color _usedCharacterColor;

		[SerializeField]
		private UILabel _playerLevelLabel;

		[NonSerialized]
		public SocialModalGUI ParentGUI;

		private HMMHub _hub;

		private IDisposable _loadRankDisposable;
	}
}
