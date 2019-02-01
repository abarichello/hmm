using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class BombInstance : IBitStreamSerializable
	{
		public TeamKind TeamOwner
		{
			get
			{
				return this._teamOwner;
			}
			set
			{
				this.LastTeamOwner = this._teamOwner;
				this._teamOwner = value;
			}
		}

		public TeamKind LastTeamOwner { get; private set; }

		public BombInfo GetBombInfo()
		{
			return BombInfo.Instance;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteIntArray(this.BombCarriersIds.ToArray());
			bs.WriteCompressedInt((int)this._teamOwner);
			bs.WriteCompressedInt((int)this.LastTeamOwner);
			bs.WriteCompressedInt(this.LastCarrier);
			bs.WriteCompressedInt((int)this.State);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.BombCarriersIds.Clear();
			this.BombCarriersIds.AddRange(bs.ReadIntArray());
			this._teamOwner = (TeamKind)bs.ReadCompressedInt();
			this.LastTeamOwner = (TeamKind)bs.ReadCompressedInt();
			this.LastCarrier = bs.ReadCompressedInt();
			this.LastState = this.State;
			this.State = (BombInstance.BombState)bs.ReadCompressedInt();
			if (this.State == BombInstance.BombState.Carried && this.BombCarriersIds.Count == 0)
			{
				BombInstance.Log.Error("State is Carried but there is no carrier! How?");
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombInstance));

		public readonly List<int> BombCarriersIds = new List<int>();

		private TeamKind _teamOwner;

		public int LastCarrier;

		public bool IsSpawned;

		public BombInstance.BombState State;

		public Dictionary<TeamKind, int> LastCarriersByTeam = new Dictionary<TeamKind, int>
		{
			{
				TeamKind.Blue,
				-1
			},
			{
				TeamKind.Red,
				-1
			}
		};

		public TeamKind TeamTerrainOwner;

		public int DetonatorCauserId;

		public int eventId;

		public int lastMeteorEventId;

		public BombInstance.BombState LastState;

		public enum BombState
		{
			Idle,
			Carried,
			Spinning,
			Meteor
		}
	}
}
