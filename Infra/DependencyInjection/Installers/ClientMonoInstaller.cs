using System;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class ClientMonoInstaller<T> : MonoInstaller<T> where T : ClientMonoInstaller<T>
	{
		public override void InstallBindings()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				Debug.Log(string.Format("{{ClientMonoInstaller}} Destroying {0} related game object because it is suposed to run only at the client.", typeof(T)));
				Object.Destroy(base.gameObject);
				return;
			}
			this.Bind();
		}

		protected virtual void Bind()
		{
			throw new UnityException(string.Format("{{ClientMonoInstaller}} Bind is not implement for: {0}", typeof(T)));
		}
	}
}
