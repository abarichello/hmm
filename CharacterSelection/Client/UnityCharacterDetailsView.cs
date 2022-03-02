using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using ModelViewer;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityCharacterDetailsView : MonoBehaviour, ICharacterDetailsView
	{
		public ILabel CharacterNameLabel
		{
			get
			{
				return this._characterNameLabel;
			}
		}

		public IDynamicImage RoleDynamicImage
		{
			get
			{
				return this._roleDynamicImage;
			}
		}

		public ILabel RoleNameLabel
		{
			get
			{
				return this._roleNameLabel;
			}
		}

		public IActivatable RotationActivatable
		{
			get
			{
				return this._rotationGameObjectActivatable;
			}
		}

		public IActivatable RemovedActivatable
		{
			get
			{
				return this._removedGameObjectActivatable;
			}
		}

		public IAssetPreview Asset3DPreview
		{
			get
			{
				return this._modelViewer;
			}
		}

		public ISlider DurabilitySlider
		{
			get
			{
				return this._durabilitySlider;
			}
		}

		public ISlider DamageSlider
		{
			get
			{
				return this._damageSlider;
			}
		}

		public ISlider RepairSlider
		{
			get
			{
				return this._repairSlider;
			}
		}

		public ISlider ControlSlider
		{
			get
			{
				return this._controlSlider;
			}
		}

		public ISlider MobilitySlider
		{
			get
			{
				return this._mobilitySlider;
			}
		}

		public IButton HelpButton
		{
			get
			{
				return this._helpButton;
			}
		}

		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICharacterDetailsView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICharacterDetailsView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityLabel _characterNameLabel;

		[SerializeField]
		private UnityDynamicImage _roleDynamicImage;

		[SerializeField]
		private UnityLabel _roleNameLabel;

		[SerializeField]
		private GameObjectActivatable _rotationGameObjectActivatable;

		[SerializeField]
		private GameObjectActivatable _removedGameObjectActivatable;

		[SerializeField]
		private BaseModelViewer _modelViewer;

		[SerializeField]
		private UnitySlider _durabilitySlider;

		[SerializeField]
		private UnitySlider _damageSlider;

		[SerializeField]
		private UnitySlider _repairSlider;

		[SerializeField]
		private UnitySlider _controlSlider;

		[SerializeField]
		private UnitySlider _mobilitySlider;

		[SerializeField]
		private UnityButton _helpButton;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;
	}
}
