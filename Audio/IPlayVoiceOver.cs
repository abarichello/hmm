using System;
using HeavyMetalMachines.Matches;

namespace HeavyMetalMachines.Audio
{
	public interface IPlayVoiceOver
	{
		void Play(VoiceOverEventGroup eventGroup, MatchClient matchClient);
	}
}
