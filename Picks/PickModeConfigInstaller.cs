using System;
using HeavyMetalMachines.Pick;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Picks
{
	[CreateAssetMenu(fileName = "PickModeConfigInstaller", menuName = "Installers/PickModeConfigInstaller")]
	public class PickModeConfigInstaller : ScriptableObjectInstaller<PickModeConfigInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IPickModeConfigProvider>().FromInstance(this._pickModeConfig);
		}

		[SerializeField]
		private PickModeConfig _pickModeConfig;
	}
}
