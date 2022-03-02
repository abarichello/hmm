using System;
using Assets.Standard_Assets.Scripts.Infra;
using Assets.Standard_Assets.Scripts.Infra.Publisher;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers.Project
{
	public class ClientPublisherInstaller : MonoInstaller<ClientPublisherInstaller>
	{
		public override void InstallBindings()
		{
			if (ClientPublisherInstaller.IsWindowsPlatform())
			{
				this.BindWindowsPublisher();
			}
			else if (Application.platform == 25)
			{
				this.BindPsnPublisher();
			}
			else
			{
				if (Application.platform != 27)
				{
					throw new PlatformNotSupportedException(string.Format("Cannot inject publisher for unknown platform {0}", Application.platform));
				}
				this.BindXboxLivePublisher();
			}
		}

		private static bool IsWindowsPlatform()
		{
			return Application.platform == 7 || Application.platform == 2;
		}

		private void BindWindowsPublisher()
		{
			if (this._configLoader.GetIntValue(ConfigAccess.PublisherIndex) == 1)
			{
				base.Container.Bind<IPublisher>().To<MyGamesPublisher>().AsSingle();
			}
			else
			{
				base.Container.Bind<IPublisher>().To<SteamPublisher>().AsSingle();
			}
		}

		private void BindPsnPublisher()
		{
			base.Container.Bind<IPublisher>().To<PsnPublisher>().AsSingle();
		}

		private void BindXboxLivePublisher()
		{
			base.Container.Bind<IPublisher>().To<XboxLivePublisher>().AsSingle();
		}

		[Inject]
		private IConfigLoader _configLoader;
	}
}
