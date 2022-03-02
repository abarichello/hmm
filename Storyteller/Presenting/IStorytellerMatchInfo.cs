using System;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Server;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public interface IStorytellerMatchInfo
	{
		GameServerRunningInfo ServerInfo { get; set; }

		ServerStatusBag ServerBag { get; set; }

		int MaxStorytellers { get; set; }

		string TranslatedPhase { get; set; }

		bool IsLocalPlayerInGroup { get; set; }

		bool IsLocalPlayerInQueue { get; set; }

		bool IsGameRunning { get; }

		bool IsFullOfSpectators { get; }

		bool CanConnect { get; }

		StorytellerMatchMember[] RedMembers { get; set; }

		StorytellerMatchMember[] BlueMembers { get; set; }
	}
}
