using System;
using System.Diagnostics;
using HeavyMetalMachines.Matches.DataTransferObjects;
using Pocketverse;

namespace HeavyMetalMachines.Match
{
	[Serializable]
	public class MatchData : IBitStreamSerializable
	{
		public PlayersLoadingProgress LoadingProgress
		{
			get
			{
				return this._loadingProgress;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<MatchKind> OnKindChange;

		public virtual bool LevelIsTutorial()
		{
			return this.Kind == 2;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.Serial++);
			bs.WriteCompressedInt(this.UserCount);
			bs.WriteCompressedInt(this.ArenaIndex);
			bs.WriteByte((byte)this.Pick);
			bs.WriteByte((byte)this.State);
			bs.WriteByte(this.Kind);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Serial = bs.ReadCompressedInt();
			this.UserCount = bs.ReadCompressedInt();
			this.ArenaIndex = bs.ReadCompressedInt();
			this.Pick = (PickMode)bs.ReadByte();
			this.State = (MatchData.MatchState)bs.ReadByte();
			this.Kind = bs.ReadByte();
		}

		public MatchKind Kind
		{
			get
			{
				return this.kind;
			}
			set
			{
				this.kind = value;
				if (this.OnKindChange != null)
				{
					this.OnKindChange(this.kind);
				}
			}
		}

		public override string ToString()
		{
			return string.Format("Match.{0}=[Level={1} Pick={2} State={3} PCount={4} Kind={5}]", new object[]
			{
				this.Serial,
				this.ArenaIndex,
				this.Pick,
				this.State,
				this.UserCount,
				this.Kind
			});
		}

		public void FeedData(int playerCount, int arenaIndex, PickMode pick)
		{
			this.Pick = pick;
			this.UserCount = playerCount;
			this.ArenaIndex = arenaIndex;
		}

		public bool MatchOver
		{
			get
			{
				return this.State == MatchData.MatchState.MatchOverTie || this.State == MatchData.MatchState.MatchOverBluWins || this.State == MatchData.MatchState.MatchOverRedWins;
			}
		}

		public static BitLogger Log = new BitLogger(typeof(MatchData));

		public int Serial;

		private readonly PlayersLoadingProgress _loadingProgress = new PlayersLoadingProgress();

		public int UserCount;

		public int ArenaIndex = -1;

		public PickMode Pick;

		public MatchData.MatchState State;

		private MatchKind kind;

		public enum MatchState : byte
		{
			Nothing,
			Tutorial,
			AwaitingConnections,
			CharacterPick,
			PreMatch,
			MatchStarted,
			MatchOverRedWins,
			MatchOverBluWins,
			MatchOverTie
		}

		public enum MatchResult
		{
			Defeat,
			Victory
		}
	}
}
