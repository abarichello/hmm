using System;
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

		public virtual bool LevelIsTutorial()
		{
			HMMHub hub = GameHubBehaviour.Hub;
			string tutorialSceneName = hub.SharedConfigs.TutorialConfig.TutorialSceneName;
			string sceneName = hub.ArenaConfig.GetSceneName(this.ArenaIndex);
			return tutorialSceneName.Equals(sceneName);
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.Serial++);
			bs.WriteCompressedInt(this.UserCount);
			bs.WriteCompressedInt(this.ArenaIndex);
			bs.WriteByte((byte)this.Pick);
			bs.WriteByte((byte)this.State);
			bs.WriteByte((byte)this.Kind);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Serial = bs.ReadCompressedInt();
			this.UserCount = bs.ReadCompressedInt();
			this.ArenaIndex = bs.ReadCompressedInt();
			this.Pick = (PickMode)bs.ReadByte();
			this.State = (MatchData.MatchState)bs.ReadByte();
			this.Kind = (MatchData.MatchKind)bs.ReadByte();
		}

		public MatchData.MatchKind Kind
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

		public static BitLogger Log = new BitLogger(typeof(MatchData));

		public int Serial;

		private readonly PlayersLoadingProgress _loadingProgress = new PlayersLoadingProgress();

		public int UserCount;

		public int ArenaIndex = -1;

		public PickMode Pick;

		public MatchData.MatchState State;

		private MatchData.MatchKind kind;

		public Action<MatchData.MatchKind> OnKindChange;

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

		public enum MatchKind : byte
		{
			PvP,
			PvE,
			Tutorial,
			Ranked,
			Custom
		}
	}
}
