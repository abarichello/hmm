using System;
using System.Linq;
using HeavyMetalMachines.Matchmaking.Configuration;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class CompetitiveQueueJoinabilityResult
	{
		public static CompetitiveQueueJoinabilityResult CreateFromBooleanValue(bool canJoin, CompetitiveQueueUnjoinabilityReason reasonIfFalse)
		{
			if (canJoin)
			{
				return CompetitiveQueueJoinabilityResult.CreateAsJoinable();
			}
			return CompetitiveQueueJoinabilityResult.CreateAsUnjoinable(reasonIfFalse);
		}

		public static CompetitiveQueueJoinabilityResult CreateAsJoinable()
		{
			return new CompetitiveQueueJoinabilityResult
			{
				CanJoin = true,
				Reasons = null
			};
		}

		public static CompetitiveQueueJoinabilityResult CreateAsUnjoinable(CompetitiveQueueUnjoinabilityReason reason)
		{
			return new CompetitiveQueueJoinabilityResult
			{
				CanJoin = false,
				Reasons = new CompetitiveQueueUnjoinabilityReason[]
				{
					reason
				}
			};
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as CompetitiveQueueJoinabilityResult);
		}

		protected bool Equals(CompetitiveQueueJoinabilityResult other)
		{
			return this.CanJoin == other.CanJoin && this.Reasons.SequenceEqual(other.Reasons) && CompetitiveQueueJoinabilityResult.QueuePeriodEquals(this.NextQueuePeriod, other.NextQueuePeriod) && CompetitiveQueueJoinabilityResult.BanEndTimeEquals(this.BanEndTime, other.BanEndTime);
		}

		private static bool QueuePeriodEquals(QueuePeriod? first, QueuePeriod? second)
		{
			if (first != null && second != null)
			{
				return first.Value.OpenDateTimeUtc == second.Value.OpenDateTimeUtc && first.Value.CloseDateTimeUtc == second.Value.CloseDateTimeUtc;
			}
			return first != null == (second != null);
		}

		private static bool BanEndTimeEquals(DateTime? one, DateTime? other)
		{
			if (one != null && other != null)
			{
				return one.Value == other.Value;
			}
			return one != null == (other != null);
		}

		public override int GetHashCode()
		{
			int num = this.Reasons.Length;
			for (int i = 0; i < this.Reasons.Length; i++)
			{
				num = (int)(num * 397 + this.Reasons[i]);
			}
			num = ((this.NextQueuePeriod != null).GetHashCode() * 397 ^ num);
			if (this.NextQueuePeriod != null)
			{
				num = (this.NextQueuePeriod.Value.OpenDateTimeUtc.GetHashCode() * 397 ^ num);
				num = (this.NextQueuePeriod.Value.CloseDateTimeUtc.GetHashCode() * 397 ^ num);
			}
			num = ((this.BanEndTime != null).GetHashCode() * 397 ^ num);
			if (this.BanEndTime != null)
			{
				num = (this.BanEndTime.Value.GetHashCode() * 397 ^ num);
			}
			return this.CanJoin.GetHashCode() * 397 ^ num;
		}

		public bool CanJoin;

		public CompetitiveQueueUnjoinabilityReason[] Reasons;

		public QueuePeriod? NextQueuePeriod;

		public DateTime? BanEndTime;
	}
}
