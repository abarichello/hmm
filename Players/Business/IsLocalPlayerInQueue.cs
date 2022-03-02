using System;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using UnityEngine;

namespace HeavyMetalMachines.Players.Business
{
	public class IsLocalPlayerInQueue : IIsLocalPlayerInQueue
	{
		public IsLocalPlayerInQueue(IMatchmakingService matchmakingService)
		{
			this._matchmakingService = matchmakingService;
		}

		public bool IsInQueue()
		{
			bool flag = this._matchmakingService.IsWaitingInQueue();
			Debug.LogFormat("is in queue: {0}", new object[]
			{
				flag
			});
			return flag;
		}

		private readonly IMatchmakingService _matchmakingService;
	}
}
