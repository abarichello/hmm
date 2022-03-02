using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.HACKS.Views
{
	public class ToggleInventoryToolView : MonoBehaviour
	{
		public Guid CategoryGuid
		{
			get
			{
				return this._categoryGuid;
			}
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

		public void SetCategoryGuid(Guid categoryGuid)
		{
			this._categoryGuid = categoryGuid;
		}

		[SerializeField]
		private HmmUiImage _icon;

		[SerializeField]
		private UnityToggle _toggle;

		private Guid _categoryGuid;
	}
}
