using System;
using HeavyMetalMachines.Matches;

namespace HeavyMetalMachines.Audio
{
	public interface IVoiceOverService
	{
		void PlayVoiceOver(VoiceOverEventGroup eventGroup, MatchClient sourceClient);
	}
}
