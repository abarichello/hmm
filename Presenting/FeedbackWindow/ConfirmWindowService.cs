using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using Hoplon.Localization.TranslationTable;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Presenting.FeedbackWindow
{
	public class ConfirmWindowService : IFeedbackWindowService
	{
		public IObservable<Unit> ShowConfirmationWindow(string description, ContextTag context)
		{
			Subject<Unit> subject = new Subject<Unit>();
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties();
			confirmWindowProperties.Guid = confirmWindowGuid;
			confirmWindowProperties.QuestionText = this._translation.Get(description, context);
			confirmWindowProperties.OkButtonText = this._translation.Get("Ok", TranslationContext.GUI);
			confirmWindowProperties.IsStackable = true;
			confirmWindowProperties.OnOk = delegate()
			{
				this._confirmWindow.HideConfirmWindow(confirmWindowGuid);
				subject.OnNext(Unit.Default);
				subject.OnCompleted();
			};
			this._confirmWindow.OpenConfirmWindow(confirmWindowProperties);
			return subject;
		}

		[Inject]
		private ConfirmWindowReference _confirmWindow;

		[Inject]
		private ILocalizeKey _translation;
	}
}
