using System;

namespace HeavyMetalMachines.Storyteller
{
	public interface IStorytellerQueueProvider
	{
		StorytellerSearchableQueue[] GetQueues();
	}
}
