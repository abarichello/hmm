using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Social.Profile.Presenting;
using UnityEngine;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileCharacterUnlockableView : MonoBehaviour, IProfileCharacterUnlockableView
	{
		public ILabel TitleLabel
		{
			get
			{
				return this._titleLabel;
			}
		}

		public ILabel UnlockLevelLabel
		{
			get
			{
				return this._unlockLevelLabel;
			}
		}

		public IDynamicImage IconImage
		{
			get
			{
				return this._iconImage;
			}
		}

		public void Activate()
		{
			base.gameObject.SetActive(true);
		}

		public void Deactivate()
		{
			base.gameObject.SetActive(false);
		}

		[SerializeField]
		private NGuiLabel _titleLabel;

		[SerializeField]
		private NGuiLabel _unlockLevelLabel;

		[SerializeField]
		private NGuiDynamicImage _iconImage;
	}
}
