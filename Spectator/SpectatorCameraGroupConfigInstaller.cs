using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Spectator
{
	[CreateAssetMenu(menuName = "Spectator/SpectatorCameraGroupConfigInstaller")]
	internal class SpectatorCameraGroupConfigInstaller : ScriptableObjectInstaller<SpectatorCameraGroupConfigInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.BindInstance<SpectatorCameraConfig>(this._spectatorCameraConfigInstaller);
		}

		[SerializeField]
		private SpectatorCameraConfig _spectatorCameraConfigInstaller;
	}
}
