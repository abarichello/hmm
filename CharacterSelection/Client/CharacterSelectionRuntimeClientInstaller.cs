using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.API;
using HeavyMetalMachines.CharacterSelection.Client.Skins;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class CharacterSelectionRuntimeClientInstaller : MonoInstaller<CharacterSelectionRuntimeClientInstaller>
	{
		public override void InstallBindings()
		{
			this.BindWithSkipSwordfishAlternative<IExecuteLocalClientCharacterSelection, ExecuteLocalClientCharacterSelection, SkipSwordfishClientExecuteLocalClientCharacterSelection>();
			base.Container.Bind<IProceedToClientCharacterSelectionState>().To<ProceedToClientCharacterSelectionState>().AsTransient();
			base.Container.BindInterfacesTo<CharacterSelectionClientRpcProxy>().AsTransient();
			base.Container.Bind<IGetLocalEquippedSkin>().To<LegacyGetLocalEquippedSkin>().AsTransient();
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
