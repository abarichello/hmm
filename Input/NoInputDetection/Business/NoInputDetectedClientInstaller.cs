using System;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using UnityEngine;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	public class NoInputDetectedClientInstaller : ClientMonoInstaller<NoInputDetectedClientInstaller>
	{
		protected override void Bind()
		{
			this.InstantiateNoInputDetectedView();
			GameObject gameObject = this.InstantiateNoInputDetectedRpc();
			NoInputDetectedRpc componentInChildren = gameObject.GetComponentInChildren<NoInputDetectedRpc>();
			base.Container.Bind<INoInputDetectedRpc>().FromInstance(componentInChildren);
			base.Container.Bind<INoInputDetectedNotifier>().To<NoInputDetectedNotifier>().AsSingle();
		}

		private GameObject InstantiateNoInputDetectedRpc()
		{
			GameObject gameObject = base.Container.InstantiatePrefab(this.NoInputDetectedRpcPrefab);
			gameObject.transform.parent = null;
			gameObject.name = "NoInputDetected Rpc Client";
			return gameObject;
		}

		private void InstantiateNoInputDetectedView()
		{
			GameObject gameObject = base.Container.InstantiatePrefab(this.NoInputDetectedViewPrefab);
			gameObject.transform.parent = null;
			gameObject.name = "NoInputDetected View Client";
		}

		public GameObject NoInputDetectedRpcPrefab;

		public GameObject NoInputDetectedViewPrefab;
	}
}
