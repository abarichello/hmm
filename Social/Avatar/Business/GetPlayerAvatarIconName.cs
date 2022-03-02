using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Customization;
using HeavyMetalMachines.Social.Avatar.Infra;
using UniRx;

namespace HeavyMetalMachines.Social.Avatar.Business
{
	public class GetPlayerAvatarIconName : IGetPlayerAvatarIconName
	{
		public GetPlayerAvatarIconName(ICollectionScriptableObject collection, IGetAvatarGuid getAvatarGuid, ICourtesyItemCollection courtesyItemCollection)
		{
			this._courtesyItemCollection = courtesyItemCollection;
			this._collection = collection;
			this._getAvatarGuid = getAvatarGuid;
			this._courtesyItemCollection = courtesyItemCollection;
		}

		public IObservable<string> Get(long playerId)
		{
			return Observable.Select<Guid, string>(this._getAvatarGuid.Get(playerId), new Func<Guid, string>(this.GetImageName));
		}

		public IObservable<string> GetSmallIcon(long playerId)
		{
			return Observable.Select<Guid, string>(this._getAvatarGuid.Get(playerId), new Func<Guid, string>(this.GetSmallImageName));
		}

		public string GetAvatarIconNameByItemId(Guid itemId)
		{
			return this.GetAvatarComponent(itemId).Sprite;
		}

		public string GetSmallAvatarIconNameByItemId(Guid itemId)
		{
			return this.GetAvatarComponent(itemId).SmallSprite;
		}

		private string GetImageName(Guid itemId)
		{
			AvatarItemTypeComponent avatarComponent = this.GetAvatarComponent(itemId);
			return avatarComponent.Sprite;
		}

		private string GetSmallImageName(Guid itemId)
		{
			AvatarItemTypeComponent avatarComponent = this.GetAvatarComponent(itemId);
			return avatarComponent.SmallSprite;
		}

		private AvatarItemTypeComponent GetAvatarComponent(Guid equippedItemTypeId)
		{
			Guid guid = equippedItemTypeId;
			if (guid == Guid.Empty)
			{
				IItemType defaultItem = this._courtesyItemCollection.GetDefaultItem(61);
				guid = defaultItem.Id;
			}
			IItemType itemType = this._collection.Get(guid);
			return itemType.GetComponent<AvatarItemTypeComponent>();
		}

		private readonly ICollectionScriptableObject _collection;

		private readonly IGetAvatarGuid _getAvatarGuid;

		private readonly ICourtesyItemCollection _courtesyItemCollection;
	}
}
