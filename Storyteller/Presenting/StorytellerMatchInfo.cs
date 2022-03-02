using System;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Server;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public class StorytellerMatchInfo : IStorytellerMatchInfo
	{
		public GameServerRunningInfo ServerInfo { get; set; }

		public ServerStatusBag ServerBag { get; set; }

		public int MaxStorytellers { get; set; }

		public string TranslatedPhase { get; set; }

		public bool IsLocalPlayerInGroup { get; set; }

		public bool IsLocalPlayerInQueue { get; set; }

		public bool IsGameRunning
		{
			get
			{
				return this.ServerBag.ServerPhase == 2;
			}
		}

		public bool IsFullOfSpectators
		{
			get
			{
				return this.ServerBag.StorytellerCount >= this.MaxStorytellers;
			}
		}

		public bool CanConnect
		{
			get
			{
				return this.IsGameRunning && !this.IsFullOfSpectators && !this.IsLocalPlayerInGroup && !this.IsLocalPlayerInQueue;
			}
		}

		public StorytellerMatchMember[] RedMembers { get; set; }

		public StorytellerMatchMember[] BlueMembers { get; set; }
	}
}
