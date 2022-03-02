using System;
using HeavyMetalMachines.Matches;

namespace HeavyMetalMachines.Audio
{
	public class PlayVoiceOver : IPlayVoiceOver
	{
		public PlayVoiceOver(IVoiceOverService voiceOverService)
		{
			this._voiceOverService = voiceOverService;
		}

		public void Play(VoiceOverEventGroup eventGroup, MatchClient sourceClient)
		{
			this._voiceOverService.PlayVoiceOver(eventGroup, sourceClient);
		}

		private readonly IVoiceOverService _voiceOverService;
	}
}
