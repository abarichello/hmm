using System;
using HeavyMetalMachines.Items.DataTransferObjects;
using HeavyMetalMachines.Players.Business;
using UniRx;

namespace HeavyMetalMachines.Social.Avatar.Infra
{
	public class GetAvatarGuid : IGetAvatarGuid
	{
		public GetAvatarGuid(IGetLocalPlayer getLocalPlayer, IGetLocalPlayerCustomizationContent localPlayerCustomizationContent, IAvatarStorageHandler avatarStorageHandler)
		{
			this._getLocalPlayer = getLocalPlayer;
			this._localPlayerCustomizationContent = localPlayerCustomizationContent;
			this._avatarStorageHandler = avatarStorageHandler;
		}

		public IObservable<Guid> Get(long playerId)
		{
			return Observable.Defer<Guid>(delegate()
			{
				if (this.IsLocalPlayer(playerId))
				{
					CustomizationContent customizationContent = this._localPlayerCustomizationContent.Get();
					Guid guidBySlot = customizationContent.GetGuidBySlot(61);
					return Observable.Return<Guid>(guidBySlot);
				}
				return this._avatarStorageHandler.GetValue(playerId);
			});
		}

		private bool IsLocalPlayer(long playerId)
		{
			return playerId == this._getLocalPlayer.Get().PlayerId;
		}

		private readonly IGetLocalPlayer _getLocalPlayer;

		private readonly IGetLocalPlayerCustomizationContent _localPlayerCustomizationContent;

		private readonly IAvatarStorageHandler _avatarStorageHandler;
	}
}
