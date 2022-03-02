using System;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.RadialMenu.View
{
	public class UnityUiRadialCustomizationView : MonoBehaviour, IRadialCustomizationView
	{
		private void Start()
		{
			this._viewProvider.Bind<IRadialCustomizationView>(this, null);
		}

		private void OnDestroy()
		{
			if (this._viewProvider != null)
			{
				this._viewProvider.Unbind<IRadialCustomizationView>(null);
			}
		}

		public Animation WindowAnimation
		{
			get
			{
				return this._windowAnimation;
			}
		}

		public IUnequipItem[] UnequipItems
		{
			get
			{
				return this._unequipItems;
			}
		}

		public ITextureMappingUpdater[] SpritesheetAnimators
		{
			get
			{
				return this._animators;
			}
		}

		public ILabel EmoteNameLabel
		{
			get
			{
				return this._emoteNameLabel;
			}
		}

		public IActivatable EquipGroupActivatable
		{
			get
			{
				return this._equipGroupActivatable;
			}
		}

		public ILabel EquipLabel
		{
			get
			{
				return this._equipLabel;
			}
		}

		public void SetupEquipShortcutImage(ISprite shortcutImageSprite)
		{
			this._equipShortcutImage.gameObject.SetActive(true);
			this._equipShortcutImage.sprite = (shortcutImageSprite as UnitySprite).GetSprite();
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private Animation _windowAnimation;

		[SerializeField]
		private UnequipSlotClick[] _unequipItems;

		[SerializeField]
		private AnimatedRawImage[] _animators;

		[SerializeField]
		private UnityLabel _emoteNameLabel;

		[SerializeField]
		private GameObjectActivatable _equipGroupActivatable;

		[SerializeField]
		private UnityLabel _equipLabel;

		[SerializeField]
		private Image _equipShortcutImage;
	}
}
