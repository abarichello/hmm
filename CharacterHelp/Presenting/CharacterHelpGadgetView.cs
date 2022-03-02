using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CharacterHelp.Presenting
{
	public class CharacterHelpGadgetView : MonoBehaviour, ICharacterHelpGadgetView
	{
		public IActivatable MainActivatable
		{
			get
			{
				return this._mainGameObjectActivatable;
			}
		}

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

		public ILabel CooldownLabel
		{
			get
			{
				return this._cooldownLabel;
			}
		}

		public ILabel DescriptionLabel
		{
			get
			{
				return this._descriptionLabel;
			}
		}

		[SerializeField]
		private GameObjectActivatable _mainGameObjectActivatable;

		[SerializeField]
		private UnityDynamicImage _iconDynamicImage;

		[SerializeField]
		private UnityLabel _nameLabel;

		[SerializeField]
		private UnityLabel _cooldownLabel;

		[SerializeField]
		private UnityLabel _descriptionLabel;
	}
}
