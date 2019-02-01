using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombObjectives : GameHubBehaviour
	{
		public Transform GetObjective(string waypointTag, TeamKind team)
		{
			BombObjectives.AIWaypoint aiwaypoint;
			return (!this._allWaypoints.TryGetValue(waypointTag, out aiwaypoint)) ? null : aiwaypoint[team];
		}

		private void Awake()
		{
			for (int i = 0; i < this.Waypoints.Length; i++)
			{
				BombObjectives.AIWaypoint aiwaypoint = this.Waypoints[i];
				this._allWaypoints[aiwaypoint.Tag] = aiwaypoint;
			}
		}

		public BombObjectives.AIWaypoint[] Waypoints;

		private readonly Dictionary<string, BombObjectives.AIWaypoint> _allWaypoints = new Dictionary<string, BombObjectives.AIWaypoint>();

		[Serializable]
		public class AIWaypoint
		{
			public Transform this[TeamKind k]
			{
				get
				{
					if (k == TeamKind.Red)
					{
						return this.RedTarget;
					}
					if (k != TeamKind.Blue)
					{
						return null;
					}
					return this.BluTarget;
				}
			}

			public string Tag;

			public Transform RedTarget;

			public Transform BluTarget;
		}
	}
}
