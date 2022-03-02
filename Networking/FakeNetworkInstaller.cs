using System;
using HeavyMetalMachines.Net.Infra;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Networking
{
	public class FakeNetworkInstaller : MonoInstaller<FakeNetworkInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<INetwork>().FromMethod((InjectContext _) => new FakeNetwork(this._isServer)).AsTransient();
		}

		[SerializeField]
		private bool _isServer;
	}
}
