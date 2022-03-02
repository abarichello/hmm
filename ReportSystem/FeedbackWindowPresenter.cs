using System;
using System.Collections.Generic;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Login;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.ReportSystem;
using HeavyMetalMachines.ReportSystem.DataTransferObjects;
using HeavyMetalMachines.ReportSystem.Exceptions;
using HeavyMetalMachines.ReportSystem.Infra;
using Hoplon;
using Hoplon.Localization;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavymetalMachines.ReportSystem
{
	public class FeedbackWindowPresenter : IFeedbackWindowPresenter
	{
		public FeedbackWindowPresenter(IViewLoader viewLoader, IViewProvider viewProvider, ILocalizeDateTime localizeDateTime, ILocalizeKey localizeKey, IEndSession endSession)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._localizeDateTime = localizeDateTime;
			this._localizeKey = localizeKey;
			this._endSession = endSession;
			this._hideSubject = new Subject<Unit>();
		}

		public IObservable<Unit> ShowExceptionAndObserveHide(AccountBannedException accountBannedException)
		{
			return Observable.First<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(this.Initialize(this.ExtractFeedbackInfoFromException(accountBannedException)), this.Show()), (Unit _) => this.ObserveHide()));
		}

		private IConsolidatedFeedbackInfo ExtractFeedbackInfoFromException(AccountBannedException accountBannedException)
		{
			ConsolidatedFeedbackInfo consolidatedFeedbackInfo = new ConsolidatedFeedbackInfo();
			FeedbackDates feedbackDates = new FeedbackDates();
			feedbackDates.StartTime = accountBannedException.RestrictionStart;
			feedbackDates.EndTime = accountBannedException.RestrictionEnd;
			consolidatedFeedbackInfo.Feedbacks.Add(6, feedbackDates);
			consolidatedFeedbackInfo.Motives = accountBannedException.Motive;
			return consolidatedFeedbackInfo;
		}

		public bool Visible
		{
			get
			{
				return this._visible;
			}
		}

		public IObservable<Unit> Initialize(IConsolidatedFeedbackInfo info)
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_FeedbackBanWindow"), delegate(Unit _)
			{
				this._compositeDisposable = new CompositeDisposable();
			}), delegate(Unit _)
			{
				this.InitializeView(info);
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (this._compositeDisposable != null)
				{
					this._compositeDisposable.Dispose();
				}
				this._compositeDisposable = null;
				this._visible = false;
				this._shouldEndSession = false;
				return this._viewLoader.UnloadView("UI_ADD_FeedbackBanWindow");
			});
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.MainCanvas.Enable();
				this._view.MainCanvasGroup.Interactable = true;
				this._view.UiNavigationGroupHolder.AddHighPriorityGroup();
				this._visible = true;
				return this._view.InAnimation.Play();
			});
		}

		private IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(() => Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(this._view.OutAnimation.Play(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveHighPriorityGroup();
				this._visible = false;
				if (this._shouldEndSession)
				{
					this._endSession.End("FeedbackWindow");
				}
			}), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			}), this.Dispose()));
		}

		private void InitializeView(IConsolidatedFeedbackInfo info)
		{
			this._view = this._viewProvider.Provide<IFeedbackWindowPresenterView>(null);
			this._view.MainCanvas.Disable();
			this._view.MainCanvasGroup.Interactable = false;
			this.InitializeMotives(info.Motives);
			this.InitializePunishments(info.Feedbacks);
			this.InitializeWarning(info.Feedbacks);
			this.InitializeOkButton();
		}

		private void InitializeMotives(ReportMotive motives)
		{
			int num = 0;
			this._view.InfringementsListLabel.Text = string.Empty;
			foreach (KeyValuePair<ReportMotive, string> keyValuePair in ReportSystemDrafts.MotiveDrafts)
			{
				if (EnumExtensions.HasFlag(motives, keyValuePair.Key))
				{
					string arg = this._localizeKey.Get(keyValuePair.Value, TranslationContext.ReportSystem);
					ILabel infringementsListLabel = this._view.InfringementsListLabel;
					infringementsListLabel.Text += string.Format("{0}- {1}", (num != 0) ? Environment.NewLine : string.Empty, arg);
					num++;
				}
			}
			if (num > 0)
			{
				ActivatableExtensions.Activate(this._view.InfringementsTitleActivatable);
				this._view.InfringementsListLabel.IsActive = true;
			}
			else
			{
				ActivatableExtensions.Deactivate(this._view.InfringementsTitleActivatable);
				this._view.InfringementsListLabel.IsActive = false;
			}
		}

		private void InitializePunishments(Dictionary<PlayerFeedbackKind, FeedbackDates> infoFeedback)
		{
			int num = 0;
			this._view.PunishmentsListLabel.Text = string.Empty;
			foreach (KeyValuePair<PlayerFeedbackKind, FeedbackDates> keyValuePair in infoFeedback)
			{
				PlayerFeedbackKind key = keyValuePair.Key;
				if (key != 1)
				{
					string text = ReportSystemDrafts.PlayerFeedbackDrafts[key];
					FeedbackDates value = keyValuePair.Value;
					string text2 = this._localizeKey.Get(text, TranslationContext.ReportSystem);
					if (key != 2)
					{
						if (key == 6)
						{
							this._shouldEndSession = (value.EndTime > DateTime.Now);
						}
						text2 = string.Format(text2, this.GetShortDateTimeString(value.StartTime), this.GetShortDateTimeString(value.EndTime));
					}
					else
					{
						text2 = string.Format(text2, this.GetShortDateTimeString(value.EndTime));
					}
					ILabel punishmentsListLabel = this._view.PunishmentsListLabel;
					punishmentsListLabel.Text += string.Format("{0}- {1}", (num != 0) ? Environment.NewLine : string.Empty, text2);
					num++;
				}
			}
			if (num > 0)
			{
				ActivatableExtensions.Activate(this._view.PunishmentsTitleActivatable);
				this._view.PunishmentsListLabel.IsActive = true;
				this._view.TitleLabel.Text = this._localizeKey.Get(ReportSystemDrafts.TitlePunishmentDraft, TranslationContext.ReportSystem);
				this._view.DescriptionLabel.Text = this._localizeKey.Get(ReportSystemDrafts.DescriptionPunishmentDraft, TranslationContext.ReportSystem);
			}
			else
			{
				ActivatableExtensions.Deactivate(this._view.PunishmentsTitleActivatable);
				this._view.PunishmentsListLabel.IsActive = false;
			}
		}

		private void InitializeWarning(Dictionary<PlayerFeedbackKind, FeedbackDates> infoFeedback)
		{
			FeedbackDates feedbackDates;
			if (!this._view.PunishmentsListLabel.IsActive && infoFeedback.TryGetValue(1, out feedbackDates))
			{
				this._view.TitleLabel.Text = this._localizeKey.Get(ReportSystemDrafts.TitleWarningDraft, TranslationContext.ReportSystem);
				this._view.DescriptionLabel.Text = this._localizeKey.Get(ReportSystemDrafts.DescriptionWarningDraft, TranslationContext.ReportSystem);
			}
		}

		private string GetShortDateTimeString(DateTime dateTime)
		{
			return LocalizationExtensions.GetShortDateString(this._localizeDateTime, dateTime);
		}

		private void InitializeOkButton()
		{
			this._compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.OkButton.OnClick()), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<Unit>(this.Hide());
			})));
		}

		private const string SceneName = "UI_ADD_FeedbackBanWindow";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly ILocalizeDateTime _localizeDateTime;

		private readonly ILocalizeKey _localizeKey;

		private readonly IEndSession _endSession;

		private readonly Subject<Unit> _hideSubject;

		private IFeedbackWindowPresenterView _view;

		private CompositeDisposable _compositeDisposable;

		private bool _visible;

		private bool _shouldEndSession;
	}
}
