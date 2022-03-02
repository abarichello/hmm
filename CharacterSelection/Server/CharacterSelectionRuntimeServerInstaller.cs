using System;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.Server.Swordfish;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class CharacterSelectionRuntimeServerInstaller : MonoInstaller<CharacterSelectionRuntimeServerInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.BindInterfacesTo<CharacterSelectionServerRpcProxy>().AsTransient();
			base.Container.Bind<INotifyCharacterSelectionReady>().To<LegacyNotifyCharacterSelectionReady>().AsTransient();
			this.BindWithSkipSwordfishAlternative<IEquippedSkinsProvider, LegacyServerEquippedSkinsProvider, SkipSwordfishEquippedSkinsProvider>();
		}

		private void BindWithSkipSwordfishAlternative<TBase, TSwordfish, TSkipSwordfish>() where TSwordfish : TBase where TSkipSwordfish : TBase
		{
			IConfigLoader configLoader = base.Container.Resolve<IConfigLoader>();
			if (configLoader.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.Container.Bind<TBase>().To<TSkipSwordfish>().AsTransient();
			}
			else
			{
				base.Container.Bind<TBase>().To<TSwordfish>().AsTransient();
			}
		}
	}
}
