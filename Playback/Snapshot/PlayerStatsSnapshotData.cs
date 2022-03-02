using System;
using HeavyMetalMachines.Bank;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class PlayerStatsSnapshotData : ISnapshotStreamContent, IPlayerStatsSerialData, IBaseStreamSerialData<IPlayerStatsSerialData>
	{
		public PlayerStatsSnapshotData()
		{
		}

		public PlayerStatsSnapshotData(PlayerStatsSnapshotData other)
		{
			this.Version = other.Version;
			this.Apply(other);
		}

		public short Version { get; set; }

		public int TimedScrap
		{
			get
			{
				return this._timedScrap;
			}
		}

		public int TotalScrapCollected
		{
			get
			{
				return this._totalScrapCollected;
			}
		}

		public int OtherScrap
		{
			get
			{
				return this._otherScrap;
			}
		}

		public int ReliableScrap
		{
			get
			{
				return this._reliableScrap;
			}
		}

		public int ScrapSpent
		{
			get
			{
				return this._scrapSpent;
			}
		}

		public int ScrapCollected
		{
			get
			{
				return this._scrapCollected;
			}
		}

		public int Level
		{
			get
			{
				return this._level;
			}
		}

		public int CreepKills
		{
			get
			{
				return this._creepKills;
			}
		}

		public int Kills
		{
			get
			{
				return this._kills;
			}
		}

		public int Deaths
		{
			get
			{
				return this._deaths;
			}
		}

		public int Assists
		{
			get
			{
				return this._assists;
			}
		}

		public int BombsDelivered
		{
			get
			{
				return this._bombsDelivered;
			}
		}

		public float DamageDealtToPlayers
		{
			get
			{
				return this._damageDealtToPlayers;
			}
		}

		public float DamageReceived
		{
			get
			{
				return this._damageReceived;
			}
		}

		public float HealingProvided
		{
			get
			{
				return this._healingProvided;
			}
		}

		public float BombPossessionTime
		{
			get
			{
				return this._bombPossessionTime;
			}
		}

		public float DebuffTime
		{
			get
			{
				return this._debuffTime;
			}
		}

		public bool Disconnected
		{
			get
			{
				return this._disconnected;
			}
		}

		public void Apply(IPlayerStatsSerialData other)
		{
			this._timedScrap = other.TimedScrap;
			this._totalScrapCollected = other.TotalScrapCollected;
			this._otherScrap = other.OtherScrap;
			this._reliableScrap = other.ReliableScrap;
			this._scrapSpent = other.ScrapSpent;
			this._scrapCollected = other.ScrapCollected;
			this._level = other.Level;
			this._creepKills = other.CreepKills;
			this._kills = other.Kills;
			this._deaths = other.Deaths;
			this._assists = other.Assists;
			this._bombsDelivered = other.BombsDelivered;
			this._damageDealtToPlayers = other.DamageDealtToPlayers;
			this._damageReceived = other.DamageReceived;
			this._healingProvided = other.HealingProvided;
			this._bombPossessionTime = other.BombPossessionTime;
			this._debuffTime = other.DebuffTime;
			this._disconnected = other.Disconnected;
		}

		public void ApplyStreamData(byte[] data)
		{
			BitStream readStream = StaticBitStream.GetReadStream(data);
			DeltaSerializableValueReader.ReadIntField(ref this._timedScrap, readStream);
			this._totalScrapCollected = readStream.ReadCompressedInt();
			if (readStream.ReadBool())
			{
				DeltaSerializableValueReader.ReadIntField(ref this._otherScrap, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._reliableScrap, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._level, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._scrapSpent, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._scrapCollected, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._creepKills, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._kills, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._deaths, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._assists, readStream);
				DeltaSerializableValueReader.ReadIntField(ref this._bombsDelivered, readStream);
			}
			DeltaSerializableValueReader.ReadFloatField(ref this._damageDealtToPlayers, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._damageReceived, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._healingProvided, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._bombPossessionTime, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._debuffTime, readStream);
			this._disconnected = readStream.ReadBool();
		}

		private int _timedScrap;

		private int _totalScrapCollected;

		private int _otherScrap;

		private int _reliableScrap;

		private int _scrapSpent;

		private int _scrapCollected;

		private int _level;

		private int _creepKills;

		private int _kills;

		private int _deaths;

		private int _assists;

		private int _bombsDelivered;

		private float _damageDealtToPlayers;

		private float _damageReceived;

		private float _healingProvided;

		private float _bombPossessionTime;

		private float _debuffTime;

		private bool _disconnected;
	}
}
