using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.Presenting
{
	public class LegacyDialogPresenter : IDialogPresenter
	{
		public LegacyDialogPresenter(ConfirmWindowReference confirmWindow, ILocalizeKey translation)
		{
			this._confirmWindow = confirmWindow;
			this._translation = translation;
		}

		public IObservable<Unit> Show(DialogConfiguration configuration)
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				Guid windowGuid = Guid.NewGuid();
				ConfirmWindowProperties properties = this.CreateProperties(observer, configuration, windowGuid);
				this._confirmWindow.OpenConfirmWindow(properties);
				return Disposable.Create(delegate()
				{
					<Show>c__AnonStorey._confirmWindow.HideConfirmWindow(windowGuid);
				});
			});
		}

		public IObservable<bool> ShowQuestionWindow(QuestionConfiguration configuration)
		{
			return Observable.Create<bool>(delegate(IObserver<bool> observer)
			{
				Guid windowGuid = Guid.NewGuid();
				ConfirmWindowProperties properties = this.CreateProperties(observer, configuration, windowGuid);
				this._confirmWindow.OpenConfirmWindow(properties);
				return Disposable.Create(delegate()
				{
					<ShowQuestionWindow>c__AnonStorey._confirmWindow.HideConfirmWindow(windowGuid);
				});
			});
		}

		private ConfirmWindowProperties CreateProperties(IObserver<Unit> observer, DialogConfiguration configuration, Guid windowGuid)
		{
			return new ConfirmWindowProperties
			{
				Guid = windowGuid,
				QuestionText = configuration.Message,
				OkButtonText = this._translation.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					this._confirmWindow.HideConfirmWindow(windowGuid);
					observer.OnNext(Unit.Default);
					observer.OnCompleted();
				}
			};
		}

		private ConfirmWindowProperties CreateProperties(IObserver<bool> observer, QuestionConfiguration configuration, Guid windowGuid)
		{
			return new ConfirmWindowProperties
			{
				Guid = windowGuid,
				QuestionText = configuration.Message,
				ConfirmButtonText = configuration.AcceptMessage,
				RefuseButtonText = configuration.DeclineMessage,
				OnConfirm = delegate()
				{
					this._confirmWindow.HideConfirmWindow(windowGuid);
					observer.OnNext(true);
					observer.OnCompleted();
				},
				OnRefuse = delegate()
				{
					this._confirmWindow.HideConfirmWindow(windowGuid);
					observer.OnNext(false);
					observer.OnCompleted();
				}
			};
		}

		private readonly ConfirmWindowReference _confirmWindow;

		private readonly ILocalizeKey _translation;
	}
}
