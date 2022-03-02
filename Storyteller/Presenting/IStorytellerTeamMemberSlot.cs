using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public interface IStorytellerTeamMemberSlot
	{
		ILabel NameLabel { get; }

		IActivatable PsnIdIconActivatable { get; }

		ILabel PsnIdLabel { get; }
	}
}
