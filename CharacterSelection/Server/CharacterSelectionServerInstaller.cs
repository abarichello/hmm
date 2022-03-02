using System;
using HeavyMetalMachines.CharacterSelection.Client.Infra;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.API;
using HeavyMetalMachines.CharacterSelection.Server.API;
using HeavyMetalMachines.CharacterSelection.Server.Bots;
using HeavyMetalMachines.CharacterSelection.Server.Swordfish;
using HeavyMetalMachines.DependencyInjection;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class CharacterSelectionServerInstaller : MonoInstaller<CharacterSelectionServerInstaller>
	{
		public override void InstallBindings()
		{
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			new CharacterSelectionModule(zenjectInjectionBinder).Bind();
			new CharacterSelectionServerModule(zenjectInjectionBinder).Bind();
			new CharacterSelectionClientModule(zenjectInjectionBinder).Bind();
			new CharacterSelectionServerBotsModule(zenjectInjectionBinder).Bind();
			this.BindWithSkipSwordfishAlternative<IExecuteCharacterSelection, ExecuteServerCharacterSelection, SkipSwordfishServerExecuteCharacterSelection>();
			this.Bind<ISendLocalPlayerReady, LocalClientReadyCommunication>();
			this.Bind<IListenForInitializationData, LocalInitializationDataCommunication>();
			this.Bind<IListenForCharacterSelectionToStart, LocalCharacterSelectionStartedCommunication>();
			this.Bind<ISendBanVoteConfirmation, LocalBanVoteConfirmationCommunication>();
			this.Bind<IListenForBanVoteConfirmations, LocalOthersBanConfirmationsCommunication>();
			this.Bind<IListenForBanStepResult, LocalBanStepResultCommunication>();
			this.Bind<ISendPickConfirmation, LocalPickConfirmationCommunication>();
			this.Bind<ISendSkinEquipped, LocalEquipSkinCommunication>();
			this.Bind<IListenForPickConfirmations, LocalOthersPickConfirmationsCommunication>();
			this.Bind<IListenForPickStepResult, LocalPickStepResultCommunication>();
			this.Bind<ISendCharacterChoiceChanges, LocalClientCharacterChoiceChangesCommunication>();
			this.Bind<IListenForCharacterChoiceChanges, LocalOthersCharacterChoiceChangesCommunication>();
			this.Bind<IListenForPickConfirmationRejection, LocalPickConfirmationRejectionCommunication>();
			this.Bind<IListenForCharacterSelectionResult, LocalCharacterSelectionResultCommunication>();
			this.Bind<IListenForClientsConnection, LocalClientsConnectionCommunication>();
		}

		private void Bind<T1, T2>() where T2 : T1
		{
			base.Container.Bind<T1>().FromMethod((InjectContext context) => (T1)((object)context.Container.Resolve<T2>()));
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
