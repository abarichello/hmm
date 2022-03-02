using System;
using HeavyMetalMachines.DependencyInjection;
using HeavyMetalMachines.Durango;
using HeavyMetalMachines.Orbis;
using HeavyMetalMachines.ParentalControl.Restrictions;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.ParentalControl
{
	public class ParentalControlInstaller : MonoInstaller<ParentalControlInstaller>
	{
		public override void InstallBindings()
		{
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			ParentalControlSystem parentalControlSystem = new ParentalControlSystem(zenjectInjectionBinder);
			parentalControlSystem.Bind();
			base.Container.Bind<IParentalControlRestrictionsConfigurationProvider>().FromInstance(this._parentalControlRestrictionsConfigurationProvider).AsSingle();
			base.Container.Bind<ICheckPlatformPrivileges>().To<CheckPlatformPrivileges>().AsTransient();
			if (Platform.Current is OrbisPlatform)
			{
				ParentalControlInstaller.Log.Debug("orbis parental control");
				zenjectInjectionBinder.BindSingle<IPersistLocalPlayerUGCRestrictionService, PersistLocalPlayerUGCRestrictionService>();
				zenjectInjectionBinder.BindSingle<IGetParentalControlSettings, OrbisGetParentalControlSettings>();
				zenjectInjectionBinder.BindSingle<IParentalControlInfoService, OrbisParentalControlInfoService>();
				zenjectInjectionBinder.BindSingle<IChatRestrictionDialogPresenter, OrbisChatRestrictionDialogPresenter>();
				zenjectInjectionBinder.BindSingle<IUGCRestrictionDialogPresenter, OrbisUGCRestrictionDialogPresenter>();
				zenjectInjectionBinder.BindSingle<IPlatformPrivilegesService, OrbisPlatformPrivilegesService>();
			}
			else if (Platform.Current is DurangoPlatform)
			{
				ParentalControlInstaller.Log.Debug("durango parental control");
				zenjectInjectionBinder.BindSingle<IPersistLocalPlayerUGCRestrictionService, PersistLocalPlayerUGCRestrictionService>();
				zenjectInjectionBinder.BindSingle<IGetParentalControlSettings, DurangoGetParentalControlSettings>();
				zenjectInjectionBinder.BindSingle<IParentalControlInfoService, DurangoParentalControlInfoService>();
				zenjectInjectionBinder.BindSingle<IChatRestrictionDialogPresenter, DefaultChatRestrictionDialogPresenter>();
				zenjectInjectionBinder.BindSingle<IUGCRestrictionDialogPresenter, DurangoUgcRestrictionDialogPresenter>();
				zenjectInjectionBinder.BindSingle<IPlatformPrivilegesService, SwordfishPlatformPrivilegesService>();
			}
			else
			{
				ParentalControlInstaller.Log.Debug("default parental control");
				zenjectInjectionBinder.BindSingle<IPersistLocalPlayerUGCRestrictionService, DefaultPersistLocalPlayerUGCRestrictionService>();
				zenjectInjectionBinder.BindSingle<IGetParentalControlSettings, DefaultGetParentalControlSettings>();
				zenjectInjectionBinder.BindSingle<IParentalControlInfoService, DefaultParentalControlInfoService>();
				zenjectInjectionBinder.BindSingle<IChatRestrictionDialogPresenter, DefaultChatRestrictionDialogPresenter>();
				zenjectInjectionBinder.BindSingle<IUGCRestrictionDialogPresenter, DefaultUGCRestrictionDialogPresenter>();
				zenjectInjectionBinder.BindSingle<IPlatformPrivilegesService, SwordfishPlatformPrivilegesService>();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ParentalControlInstaller));

		[SerializeField]
		private ParentalControlRestrictionsConfigurationProvider _parentalControlRestrictionsConfigurationProvider;
	}
}
