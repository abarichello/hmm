using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityCharactersListItemView : MonoBehaviour, ICharactersListItemView
	{
		public Toggle SelectionToggle
		{
			get
			{
				return this._selectionToggle.Toggle;
			}
		}

		public void SetAsGrayscale()
		{
			Color color = this._iconImage.Color;
			color.R = 0f;
			this._iconImage.Color = color;
		}

		public ILabel CharacterNameLabel
		{
			get
			{
				return this._characterNameLabel;
			}
		}

		public IDynamicImage IconImage
		{
			get
			{
				return this._iconImage;
			}
		}

		public IToggle Toggle
		{
			get
			{
				return this._selectionToggle;
			}
		}

		public IActivatable SelectedIndicator
		{
			get
			{
				return this._selectedIndicator;
			}
		}

		public IActivatable BanVoteIndicator
		{
			get
			{
				return this._banVoteIndicator;
			}
		}

		public IActivatable BannedIndicator
		{
			get
			{
				return this._bannedIndicator;
			}
		}

		public IActivatable RotationIndicator
		{
			get
			{
				return this._rotationIndicator;
			}
		}

		public IActivatable BannedByModeIndicator
		{
			get
			{
				return this._bannedByModeIndicator;
			}
		}

		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		[SerializeField]
		private UnityLabel _characterNameLabel;

		[SerializeField]
		private UnityDynamicImage _iconImage;

		[SerializeField]
		private UnityToggle _selectionToggle;

		[SerializeField]
		private GameObjectActivatable _selectedIndicator;

		[SerializeField]
		private GameObjectActivatable _banVoteIndicator;

		[SerializeField]
		private GameObjectActivatable _bannedIndicator;

		[SerializeField]
		private GameObjectActivatable _rotationIndicator;

		[SerializeField]
		private GameObjectActivatable _bannedByModeIndicator;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;
	}
}
