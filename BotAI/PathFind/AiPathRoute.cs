using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.BotAI.PathFind
{
	public struct AiPathRoute
	{
		public AiPathRoute(int startingNode, int targetNode, bool bombBlocked, TeamKind team)
		{
			this.StartingNode = startingNode;
			this.TargetNode = targetNode;
			this.BombBlocked = bombBlocked;
			this.Team = team;
		}

		public bool Equals(AiPathRoute other)
		{
			return this.StartingNode == other.StartingNode && this.TargetNode == other.TargetNode && this.BombBlocked == other.BombBlocked && this.Team == other.Team;
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && obj is AiPathRoute && this.Equals((AiPathRoute)obj);
		}

		public override int GetHashCode()
		{
			int num = this.StartingNode;
			num = (num * 397 ^ this.TargetNode);
			num = (num * 397 ^ this.BombBlocked.GetHashCode());
			return num * 397 ^ (int)this.Team;
		}

		public static bool operator ==(AiPathRoute one, AiPathRoute other)
		{
			return one.StartingNode == other.StartingNode && one.TargetNode == other.TargetNode && one.BombBlocked == other.BombBlocked && one.Team == other.Team;
		}

		public static bool operator !=(AiPathRoute one, AiPathRoute other)
		{
			return !(one == other);
		}

		public readonly int StartingNode;

		public readonly int TargetNode;

		public readonly bool BombBlocked;

		public readonly TeamKind Team;
	}
}
