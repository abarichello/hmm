using System;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using UnityEngine;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	public class NoInputDetectedServerInstaller : ServerMonoInstaller<NoInputDetectedServerInstaller>
	{
		protected override void Bind()
		{
			GameObject gameObject = this.InstantiateNoInputDetected();
			NoInputDetectedRpc component = gameObject.GetComponent<NoInputDetectedRpc>();
			base.Container.Bind<INoInputDetectedRpc>().FromInstance(component);
		}

		private GameObject InstantiateNoInputDetected()
		{
			GameObject gameObject = base.Container.InstantiatePrefab(this.NoInputDetectedRpcPrefab);
			gameObject.transform.parent = null;
			gameObject.name = "NoInputDetected Server";
			return gameObject;
		}

		public GameObject NoInputDetectedRpcPrefab;
	}
}
