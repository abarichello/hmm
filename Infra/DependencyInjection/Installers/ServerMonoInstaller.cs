using System;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class ServerMonoInstaller<T> : MonoInstaller<T> where T : ServerMonoInstaller<T>
	{
		public override void InstallBindings()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				Debug.Log(string.Format("{{ServerMonoInstaller}} Destroying {0} related game object because it is supposed to run only at the server.", typeof(T)));
				Object.Destroy(base.gameObject);
				return;
			}
			this.Bind();
		}

		protected virtual void Bind()
		{
			throw new UnityException(string.Format("{{ServerMonoInstaller}} Bind is not implement for: {0}", typeof(T)));
		}
	}
}
