using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CharacterHelp.Presenting
{
	public class CharacterHelpInfoView : MonoBehaviour, ICharacterHelpInfoView
	{
		public IDynamicImage IconDynamicImage
		{
			get
			{
				return this._iconDynamicImage;
			}
		}

		public ILabel NameLabel
		{
			get
			{
				return this._nameLabel;
			}
		}

		public ILabel RoleNameLabel
		{
			get
			{
				return this._roleNameLabel;
			}
		}

		[SerializeField]
		private UnityDynamicImage _iconDynamicImage;

		[SerializeField]
		private UnityLabel _nameLabel;

		[SerializeField]
		private UnityLabel _roleNameLabel;
	}
}
