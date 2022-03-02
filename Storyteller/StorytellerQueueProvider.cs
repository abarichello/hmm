using System;
using System.Collections.Generic;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Spectator;
using HeavyMetalMachines.Tournaments;
using HeavyMetalMachines.Tournaments.API;
using Hoplon.Localization.TranslationTable;

namespace HeavyMetalMachines.Storyteller
{
	public class StorytellerQueueProvider : IStorytellerQueueProvider
	{
		public StorytellerQueueProvider(IGetTournamentTier getTournamentTier, ILocalizeKey translation, IIsCrossplayEnabled isCrossplayEnabled, IGetSpectatorQueueFilter getSpectatorQueueFilter)
		{
			this._getTournamentTier = getTournamentTier;
			this._translation = translation;
			this._isCrossplayEnabled = isCrossplayEnabled;
			this._getSpectatorQueueFilter = getSpectatorQueueFilter;
		}

		private static StorytellerSearchableQueue CreateQueueWithDraft(string queueName, string translation)
		{
			return new StorytellerSearchableQueue
			{
				QueueName = queueName,
				LocalizedName = translation
			};
		}

		private StorytellerSearchableQueue CreateQueue(string queueName)
		{
			return new StorytellerSearchableQueue
			{
				QueueName = queueName,
				LocalizedName = this._translation.Get(StorytellerQueueProvider._queueDraftTable[queueName], TranslationContext.Storyteller)
			};
		}

		private void AddQueueByFilterVisibility(string queueName, CrossplayPlatformQueueSettings filter, ref List<StorytellerSearchableQueue> target)
		{
			if (filter.Visibility[queueName])
			{
				target.Add(this.CreateQueue(queueName));
			}
		}

		public StorytellerSearchableQueue[] GetQueues()
		{
			CrossplayPlatformQueueSettings filter = this._getSpectatorQueueFilter.Get();
			List<StorytellerSearchableQueue> list = new List<StorytellerSearchableQueue>();
			this.AddQueueByFilterVisibility("Normal", filter, ref list);
			this.AddQueueByFilterVisibility("Ranked", filter, ref list);
			this.AddQueueByFilterVisibility("NormalPSN", filter, ref list);
			this.AddQueueByFilterVisibility("RankedPSN", filter, ref list);
			this.AddQueueByFilterVisibility("NormalXboxLive", filter, ref list);
			this.AddQueueByFilterVisibility("RankedXboxLive", filter, ref list);
			if (this._isCrossplayEnabled.Get())
			{
				this.SetTournamentQueues(this._getTournamentTier.GetAll(), ref list);
			}
			return list.ToArray();
		}

		private void SetTournamentQueues(IEnumerable<TournamentTier> tiers, ref List<StorytellerSearchableQueue> queues)
		{
			foreach (TournamentTier tournamentTier in tiers)
			{
				string text = string.Format("TOURNAMENT_REWARDS_TITLE_{0}", tournamentTier.Name.ToUpper());
				StorytellerSearchableQueue storytellerSearchableQueue = StorytellerQueueProvider.CreateQueueWithDraft(tournamentTier.QueuName, this._translation.GetFormatted(text, TranslationContext.Tournament, new object[0]));
				storytellerSearchableQueue.IsTournamentTier = true;
				storytellerSearchableQueue.TournamentTier = tournamentTier;
				queues.Add(storytellerSearchableQueue);
			}
		}

		private readonly IGetTournamentTier _getTournamentTier;

		private readonly ILocalizeKey _translation;

		private readonly IIsCrossplayEnabled _isCrossplayEnabled;

		private readonly IGetSpectatorQueueFilter _getSpectatorQueueFilter;

		private static readonly Dictionary<string, string> _queueDraftTable = new Dictionary<string, string>
		{
			{
				"Normal",
				"STORYTELLER_GAMEMODE_PVP"
			},
			{
				"Ranked",
				"STORYTELLER_GAMEMODE_RANKED"
			},
			{
				"NormalPSN",
				"STORYTELLER_GAMEMODE_PVP_PSN"
			},
			{
				"RankedPSN",
				"STORYTELLER_GAMEMODE_RANKED_PSN"
			},
			{
				"NormalXboxLive",
				"STORYTELLER_GAMEMODE_PVP_XBOX"
			},
			{
				"RankedXboxLive",
				"STORYTELLER_GAMEMODE_RANKED_XBOX"
			}
		};
	}
}
