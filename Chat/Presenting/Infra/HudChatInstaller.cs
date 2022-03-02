using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Chat.Presenting.Infra
{
	public class HudChatInstaller : MonoInstaller<HudChatInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<HudChatController>().FromInstance(this._hudChatController);
			base.Container.Bind<IHudChatLoader>().To<HudChatLoader>().AsSingle().NonLazy();
			base.Container.Bind<IHudChatPresenter>().To<HudChatPresenter>().AsSingle().NonLazy();
		}

		[SerializeField]
		private HudChatController _hudChatController;
	}
}
