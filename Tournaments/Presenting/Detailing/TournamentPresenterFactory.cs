using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.OpenUrl;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Social.Teams.Business;
using Hoplon.Localization.TranslationTable;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing
{
	public class TournamentPresenterFactory
	{
		public TournamentPresenterFactory(IViewLoader viewLoader, IViewProvider viewProvider, ILocalizeKey translation, IOpenUrl openUrl, IObserveLocalPlayerTeamChanges observeLocalPlayerTeamChanges, IClientButtonBILogger buttonBILogger)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._translation = translation;
			this._openUrl = openUrl;
			this._observeLocalPlayerTeamChanges = observeLocalPlayerTeamChanges;
			this._buttonBILogger = buttonBILogger;
		}

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly ILocalizeKey _translation;

		private readonly IOpenUrl _openUrl;

		private readonly IObserveLocalPlayerTeamChanges _observeLocalPlayerTeamChanges;

		private readonly IClientButtonBILogger _buttonBILogger;
	}
}
