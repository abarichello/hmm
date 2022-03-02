using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Players.Business
{
	public class IsLocalPlayerInMatch : IIsLocalPlayerInMatch
	{
		public bool IsInMatch()
		{
			bool flag = GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.Game || GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.Pick || GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.Loading || GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.GameWrapUp || GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.MatchMaking;
			Debug.LogFormat("is in match: {0}", new object[]
			{
				flag
			});
			return flag;
		}
	}
}
