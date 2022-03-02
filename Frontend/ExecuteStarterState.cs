using System;
using HeavyMetalMachines.FeaturesToggle.View;
using HeavyMetalMachines.Input.NoInputDetection.Business;
using HeavyMetalMachines.Input.NoInputDetection.Presenting;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.GenericConfirmWindow;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Social;
using HeavyMetalMachines.Swordfish;
using Hoplon.Input.Business;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using Hoplon.Reactive;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class ExecuteStarterState : IExecuteStarterState
	{
		public ExecuteStarterState(IShowSystemCheckDialogs showSystemCheckDialogs, ISendSystemSpecificationsToBi sendSystemSpecificationsToBi, IInputInitializer inputInitializer, INoInputDetectedNotifier noInputDetectedNotifier, INoInputDetectedPresenterActivator noInputDetectedPresenterActivator, INoInputDetectedPresenter noInputDetectedPresenter, IAllowedCharactersProvider allowedCharactersProvider, IBadNamesProvider badNamesProvider, IInitializeLocalization initializeLocalization, SwordfishServices swordfishServices, IGenericConfirmWindowPresenter confirmWindowPresenter, ILocalizeKey localizeKey, IGetLanguageLocale getLanguageLocale, IOpenFeatureToggleConfiguration openFeatureToggleConfiguration, IBadWordsProvider badWordsProvider, ILogPlatformFocusChange logPlatformFocusChange, DiContainer diContainer, ILogger<ExecuteStarterState> logger)
		{
			this._showSystemCheckDialogs = showSystemCheckDialogs;
			this._sendSystemSpecificationsToBi = sendSystemSpecificationsToBi;
			this._inputInitializer = inputInitializer;
			this._noInputDetectedNotifier = noInputDetectedNotifier;
			this._noInputDetectedPresenterActivator = noInputDetectedPresenterActivator;
			this._noInputDetectedPresenter = noInputDetectedPresenter;
			this._allowedCharactersProvider = allowedCharactersProvider;
			this._badNamesProvider = badNamesProvider;
			this._initializeLocalization = initializeLocalization;
			this._swordfishServices = swordfishServices;
			this._confirmWindowPresenter = confirmWindowPresenter;
			this._localizeKey = localizeKey;
			this._getLanguageLocale = getLanguageLocale;
			this._openFeatureToggleConfiguration = openFeatureToggleConfiguration;
			this._badWordsProvider = badWordsProvider;
			this._logPlatformFocusChange = logPlatformFocusChange;
			this._diContainer = diContainer;
			this._logger = logger;
		}

		public IObservable<Unit> Execute()
		{
			return ObservableExtensions.ContinueWith<Unit, Unit>(ObservableExtensions.ContinueWith<Unit, Unit>(Observable.ReturnUnit(), new Func<IObservable<Unit>>(this.InitializeFeatures)), new Func<IObservable<Unit>>(this.OpenFeatureToggleConfiguration));
		}

		private IObservable<Unit> InitializeFeatures()
		{
			return Observable.DoOnCompleted<Unit>(ObservableExtensions.ContinueWith<Unit, Unit>(Observable.DoOnCompleted<Unit>(Observable.DoOnCompleted<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(ObservableExtensions.ContinueWith<Unit, Unit>(Observable.DoOnCompleted<Unit>(Observable.DoOnCompleted<Unit>(Observable.DoOnCompleted<Unit>(Observable.ReturnUnit(), new Action(this._inputInitializer.Initialize)), new Action(this.InitializeNoInputDetection)), new Action(this.InitializeLogPlatformFocusChange)), new Func<IObservable<Unit>>(this.InitializeLocalization)), this._allowedCharactersProvider.Load()), this._badNamesProvider.Load()), this._badWordsProvider.Load()), new Action(this._swordfishServices.Initialize)), new Action(this._sendSystemSpecificationsToBi.Send)), new Func<IObservable<Unit>>(this._showSystemCheckDialogs.Show)), new Action(this.InitializePublisher));
		}

		private void InitializePublisher()
		{
			ObservableExtensions.Subscribe<Unit>(this._diContainer.Resolve<IPublisherInitializer>().Initialize());
		}

		private void InitializeLogPlatformFocusChange()
		{
			this._logPlatformFocusChange.Initialize();
		}

		private void InitializeNoInputDetection()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.Merge<Unit>(Observable.Merge<Unit>(this._noInputDetectedPresenter.Initialize(), new IObservable<Unit>[]
			{
				this._noInputDetectedPresenterActivator.Initialize()
			}), new IObservable<Unit>[]
			{
				this._noInputDetectedNotifier.Initialize()
			}));
		}

		private IObservable<Unit> InitializeLocalization()
		{
			LocalizationInitializationResult localizationInitializationResult = this._initializeLocalization.Initialize();
			return Observable.ReturnUnit();
		}

		private IObservable<Unit> ShowLocalizationChangedFeedback(LocalizationInitializationResult localizationInitializationResult)
		{
			if (localizationInitializationResult == LocalizationInitializationResult.ChangedToSystemLanguage)
			{
				return this.ShowLocalizationMessage("FEEDBACK_WINDOW_SET_LANGUAGE");
			}
			if (localizationInitializationResult == LocalizationInitializationResult.FallbackedToDefaultLanguage)
			{
				return this.ShowLocalizationMessage("FEEDBACK_WINDOW_SET_LANGUAGE_DEFAULT");
			}
			return Observable.ReturnUnit();
		}

		private IObservable<Unit> ShowLocalizationMessage(string messageDraft)
		{
			return this._confirmWindowPresenter.Show(new DialogConfiguration
			{
				Title = this._localizeKey.Get("Idioma", TranslationContext.Options),
				Message = this._localizeKey.GetFormatted(messageDraft, TranslationContext.MainMenuGui, new object[]
				{
					this._getLanguageLocale.GetLocalizedCurrent()
				})
			});
		}

		private IObservable<Unit> OpenFeatureToggleConfiguration()
		{
			return Observable.ReturnUnit();
		}

		private readonly IShowSystemCheckDialogs _showSystemCheckDialogs;

		private readonly ISendSystemSpecificationsToBi _sendSystemSpecificationsToBi;

		private readonly IInputInitializer _inputInitializer;

		private readonly INoInputDetectedNotifier _noInputDetectedNotifier;

		private readonly INoInputDetectedPresenterActivator _noInputDetectedPresenterActivator;

		private readonly INoInputDetectedPresenter _noInputDetectedPresenter;

		private readonly IAllowedCharactersProvider _allowedCharactersProvider;

		private readonly IBadNamesProvider _badNamesProvider;

		private readonly IInitializeLocalization _initializeLocalization;

		private readonly SwordfishServices _swordfishServices;

		private readonly IGenericConfirmWindowPresenter _confirmWindowPresenter;

		private readonly ILocalizeKey _localizeKey;

		private readonly IGetLanguageLocale _getLanguageLocale;

		private readonly IOpenFeatureToggleConfiguration _openFeatureToggleConfiguration;

		private readonly IBadWordsProvider _badWordsProvider;

		private readonly ILogPlatformFocusChange _logPlatformFocusChange;

		private readonly DiContainer _diContainer;

		private readonly ILogger<ExecuteStarterState> _logger;
	}
}
