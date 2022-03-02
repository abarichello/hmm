using System;
using FMod;
using HeavyMetalMachines.Localization;
using Hoplon.Input.Business;
using Hoplon.ToggleableFeatures;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class Initializable : IInitializable
	{
		public Initializable(ManualInitializeLocalization initializeLocalization, DiContainer diContainer)
		{
			this._initializeLocalization = initializeLocalization;
			this._diContainer = diContainer;
		}

		public void Initialize()
		{
			this._initializeLocalization.Language = LanguageCode.EN;
			this._initializeLocalization.Initialize();
			this._diContainer.Resolve<IInputInitializer>().Initialize();
			ObservableExtensions.Subscribe<Unit>(this._diContainer.Resolve<IInitializeToggleableFeatures>().Initialize());
			FMODAudioManager fmodaudioManager = new FMODAudioManager(new FakeIsOutsideAudibleDistance(), new FakeIsFeatureToggled());
		}

		private readonly ManualInitializeLocalization _initializeLocalization;

		private readonly DiContainer _diContainer;
	}
}
