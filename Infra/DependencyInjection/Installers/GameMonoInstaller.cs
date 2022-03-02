using System;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class GameMonoInstaller<T> : MonoInstaller<T> where T : GameMonoInstaller<T>
	{
		public override void InstallBindings()
		{
			if (GameHubBehaviour.Hub.Net.IsTest())
			{
				this.BindGameTestMode();
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.BindClient();
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.BindServer();
			}
			this.BindBoth();
		}

		protected virtual void BindClient()
		{
		}

		protected virtual void BindServer()
		{
		}

		protected virtual void BindBoth()
		{
		}

		protected virtual void BindGameTestMode()
		{
		}
	}
}
