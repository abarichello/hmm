using System;
using ClientAPI.Objects;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class FriendInviteGuiItem : BaseGUIItem<FriendInviteGuiItem, UserFriend>
	{
		protected override void SetPropertiesTasks(UserFriend userFriend)
		{
			this._textLabel.text = userFriend.PlayerName;
		}

		[SerializeField]
		private SteamIconLoader _iconLoader;

		[SerializeField]
		private UILabel _textLabel;
	}
}
