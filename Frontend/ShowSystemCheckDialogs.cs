using System;
using HeavyMetalMachines.HardwareAnalysis;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Serialization;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public class ShowSystemCheckDialogs : IShowSystemCheckDialogs
	{
		public ShowSystemCheckDialogs(IPerformSystemCheck performSystemCheck, ConfirmWindowReference confirmWindowReference, IQuitApplication quitApplication, ILocalPlayerPreferences localPlayerPreferences)
		{
			this._performSystemCheck = performSystemCheck;
			this._confirmWindowReference = confirmWindowReference;
			this._quitApplication = quitApplication;
			this._localPlayerPreferences = localPlayerPreferences;
		}

		public IObservable<Unit> Show()
		{
			if (Platform.Current.IsConsole())
			{
				return Observable.ReturnUnit();
			}
			return Observable.ContinueWith<SystemCheckResult, Unit>(Observable.Start<SystemCheckResult>(() => this._performSystemCheck.Check(), Scheduler.Immediate), new Func<SystemCheckResult, IObservable<Unit>>(this.ShowDialogs));
		}

		private IObservable<Unit> ShowDialogs(SystemCheckResult result)
		{
			if (result == 2)
			{
				return this.ShowQuitApplicationDialog();
			}
			if (result != 1)
			{
				return Observable.ReturnUnit();
			}
			return this.ShowWarningWindow();
		}

		private IObservable<Unit> ShowWarningWindow()
		{
			if (!this._localPlayerPreferences.GetBoolean("SHOW_SYSTEM_CHECK_WARNING", true))
			{
				return Observable.ReturnUnit();
			}
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				QuestionText = Language.Get("MEMORY_WARNING_DESCRIPTION", TranslationContext.MainMenuGui),
				OkButtonText = Language.Get("MEMORY_WARNING_CONFIRM", TranslationContext.MainMenuGui),
				CheckboxText = Language.Get("MEMORY_WARNING_DEACTIVE_CHECKBOX", TranslationContext.MainMenuGui),
				CheckboxInitialState = false
			};
			return Observable.AsUnitObservable<ConfirmWindowResult>(Observable.Do<ConfirmWindowResult>(this._confirmWindowReference.OpenConfirmWindowAsync(properties), new Action<ConfirmWindowResult>(this.SaveWarningOption)));
		}

		private void SaveWarningOption(ConfirmWindowResult result)
		{
			if (result.CheckboxValue)
			{
				this._localPlayerPreferences.SetBoolean("SHOW_SYSTEM_CHECK_WARNING", false);
			}
		}

		private IObservable<Unit> ShowQuitApplicationDialog()
		{
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				TileText = Language.Get("SYSTEMINFO_DENY_TITLE", TranslationContext.MainMenuGui),
				QuestionText = Language.Get("SYSTEMINFO_DENY_DESCRIPTION", TranslationContext.MainMenuGui).Replace("\\n", "\n"),
				OkButtonText = Language.Get("SYSTEMINFO_DENY_CONFIRM", TranslationContext.MainMenuGui),
				CountDownTime = 5f
			};
			return Observable.AsUnitObservable<ConfirmWindowResult>(Observable.Do<ConfirmWindowResult>(this._confirmWindowReference.OpenConfirmWindowAsync(properties), delegate(ConfirmWindowResult _)
			{
				this.QuitApplication();
			}));
		}

		private void QuitApplication()
		{
			this._quitApplication.Quit(14);
		}

		private const string ShowSystemCheckWarningPreferencesKey = "SHOW_SYSTEM_CHECK_WARNING";

		private readonly IPerformSystemCheck _performSystemCheck;

		private readonly ConfirmWindowReference _confirmWindowReference;

		private readonly IQuitApplication _quitApplication;

		private readonly ILocalPlayerPreferences _localPlayerPreferences;
	}
}
