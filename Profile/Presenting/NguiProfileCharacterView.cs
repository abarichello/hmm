using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Social.Profile.Presenting;
using UnityEngine;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileCharacterView : MonoBehaviour, IProfileCharacterView
	{
		public IButton Button
		{
			get
			{
				return this._button;
			}
		}

		public ILabel CharacterNameLabel
		{
			get
			{
				return this._characterNameLabel;
			}
		}

		public IDynamicImage CharacterIconImage
		{
			get
			{
				return this._characterIconImage;
			}
		}

		public ILabel CharacterLevelLabel
		{
			get
			{
				return this._characterLevelLabel;
			}
		}

		public IProgressBar CharacterLevelProgressBar
		{
			get
			{
				return this._characterLevelProgressBar;
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
		private NGuiButton _button;

		[SerializeField]
		private NGuiLabel _characterNameLabel;

		[SerializeField]
		private NGuiDynamicImage _characterIconImage;

		[SerializeField]
		private NGuiLabel _characterLevelLabel;

		[SerializeField]
		private NGuiProgressBar _characterLevelProgressBar;
	}
}
