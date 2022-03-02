using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Social.Profile.Presenting;
using UnityEngine;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileCharacterRoleProgressView : MonoBehaviour, IProfileCharacterRoleProgressView
	{
		public ILabel NumberLabel
		{
			get
			{
				return this._numberLabel;
			}
		}

		public IFillImage FillImage
		{
			get
			{
				return this._fillImage;
			}
		}

		public ISpriteImage FillImageBorder
		{
			get
			{
				return this._fillImageBorder;
			}
		}

		[SerializeField]
		private NGuiLabel _numberLabel;

		[SerializeField]
		private NguiFillImage _fillImage;

		[SerializeField]
		private NGuiImage _fillImageBorder;
	}
}
