using System;
using HeavyMetalMachines.DataTransferObjects.Server;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Regions.Infra;
using Hoplon.Localization.TranslationTable;

namespace HeavyMetalMachines.Storyteller
{
	public class StorytellerTranslationProvider : IStorytellerTranslationProvider
	{
		public string GetTranslatedTitle()
		{
			return this._translation.Get("STORYTELLER_TITLE", TranslationContext.Storyteller);
		}

		public string GetTranslatedRegion(string region)
		{
			return this._translation.Get(this._regionService.GetRegionDraft(region), TranslationContext.Region);
		}

		public string GetTranslatedServerPhase(ServerStatusBag.ServerPhaseKind phase)
		{
			switch (phase)
			{
			case 2:
				return this._translation.Get("STORYTELLER_MATCH_STATUS_ONGOING", TranslationContext.Storyteller);
			case 3:
				return this._translation.Get("STORYTELLER_MATCH_STATUS_ENDING", TranslationContext.Storyteller);
			}
			return this._translation.Get("STORYTELLER_MATCH_STATUS_PICK", TranslationContext.Storyteller);
		}

		[InjectOnClient]
		private ILocalizeKey _translation;

		[InjectOnClient]
		private IRegionService _regionService;

		[InjectOnClient]
		private IStorytellerQueueProvider _queueProvider;
	}
}
