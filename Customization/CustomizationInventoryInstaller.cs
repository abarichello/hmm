using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Customization
{
	public class CustomizationInventoryInstaller : ScriptableObjectInstaller<CustomizationInventoryInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IGetCustomizationChange>().FromInstance(this._customizationInventoryComponent).AsSingle();
		}

		[SerializeField]
		private CustomizationInventoryComponent _customizationInventoryComponent;
	}
}
