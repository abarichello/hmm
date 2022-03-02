using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.HACKS.Views
{
	public class ItemInventoryToolView : MonoBehaviour
	{
		public Guid ItemGuid
		{
			get
			{
				return this._itemGuid;
			}
		}

		public void SetGuid(Guid itemGuid)
		{
			this._itemGuid = itemGuid;
		}

		public IToggle Button
		{
			get
			{
				return this._toggle;
			}
		}

		public void SetIcon(string sprite)
		{
			this._icon.TryToLoadAsset(sprite);
			base.transform.localScale = Vector3.one;
		}

		[SerializeField]
		private HmmUiImage _icon;

		[SerializeField]
		private UnityToggle _toggle;

		private Guid _itemGuid;
	}
}
