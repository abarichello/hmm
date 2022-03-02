using System;

namespace HeavyMetalMachines.Players.Business
{
	public class LocalPlayerStorage : ILocalPlayerStorage
	{
		public LocalPlayerStorage(UserInfo userInfo)
		{
			this._userInfo = userInfo;
		}

		public IPlayer Player
		{
			get
			{
				return this.ConvertFromSwordfishUser();
			}
			set
			{
				throw new NotImplementedException("A player cannot be set on PlayerStorage because the UserInfo is still the class resposible for storing this information.");
			}
		}

		private Player ConvertFromSwordfishUser()
		{
			Player player = new Player();
			player.Id = Guid.NewGuid();
			player.PlayerId = this._userInfo.PlayerSF.Id;
			player.Bag = this._userInfo.Bag;
			player.Nickname = this._userInfo.PlayerSF.Name;
			Player player2 = player;
			long? nameTag = this._userInfo.PlayerSF.NameTag;
			player2.PlayerTag = new long?((nameTag == null) ? 0L : nameTag.Value);
			player.UniversalId = this._userInfo.UniversalId;
			player.Email = this._userInfo.PlayerSF.Email;
			return player;
		}

		private readonly UserInfo _userInfo;
	}
}
