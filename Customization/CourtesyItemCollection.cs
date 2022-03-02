using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using UnityEngine;

namespace HeavyMetalMachines.Customization
{
	[CreateAssetMenu(menuName = "Scriptable Object/Customization/CourtesyItemCollection")]
	public class CourtesyItemCollection : ScriptableObject, ICourtesyItemCollection
	{
		public IItemType GetDefaultItem(PlayerCustomizationSlot slot)
		{
			switch (slot)
			{
			case 1:
				return this._spray;
			case 2:
				return this._takeOffVfx;
			case 3:
				return this._scoreVfx;
			case 4:
				return this._killVfx;
			case 5:
				return this._respawnVfx;
			default:
				if (slot == 60)
				{
					return this._portrait;
				}
				if (slot != 61)
				{
					return null;
				}
				return this._defaultAvatar;
			}
		}

		public IItemType[] GetItems(PlayerCustomizationSlot slot)
		{
			if (slot != 61)
			{
				return null;
			}
			return this._avatars;
		}

		[SerializeField]
		private ItemTypeScriptableObject _takeOffVfx;

		[SerializeField]
		private ItemTypeScriptableObject _scoreVfx;

		[SerializeField]
		private ItemTypeScriptableObject _killVfx;

		[SerializeField]
		private ItemTypeScriptableObject _respawnVfx;

		[SerializeField]
		private ItemTypeScriptableObject _spray;

		[SerializeField]
		private ItemTypeScriptableObject _portrait;

		[SerializeField]
		private ItemTypeScriptableObject _defaultAvatar;

		[SerializeField]
		private ItemTypeScriptableObject[] _avatars;
	}
}
