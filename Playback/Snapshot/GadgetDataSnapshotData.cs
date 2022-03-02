using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class GadgetDataSnapshotData : ISnapshotStreamContent, IGadgetDataSerialData, IBaseStreamSerialData<IGadgetDataSerialData>
	{
		public GadgetDataSnapshotData()
		{
		}

		public GadgetDataSnapshotData(GadgetDataSnapshotData other)
		{
			this.Version = other.Version;
			this.Apply(other);
		}

		public short Version { get; set; }

		public IGadgetStateObjectSerialData BombExplosionStateObjectData
		{
			get
			{
				return this._bombExplosionStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData KillStateObjectData
		{
			get
			{
				return this._killStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData TakeoffStateObjectData
		{
			get
			{
				return this._takeoffStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData RespawnStateObjectData
		{
			get
			{
				return this._respawnStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData BombStateObjectData
		{
			get
			{
				return this._bombStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData GBoostStateObjectData
		{
			get
			{
				return this._gBoostStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData G0StateObjectData
		{
			get
			{
				return this._g0StateObjectData;
			}
		}

		public IGadgetStateObjectSerialData G1StateObjectData
		{
			get
			{
				return this._g1StateObjectData;
			}
		}

		public IGadgetStateObjectSerialData G2StateObjectData
		{
			get
			{
				return this._g2StateObjectData;
			}
		}

		public IGadgetStateObjectSerialData GPStateObjectData
		{
			get
			{
				return this._gPStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData GOutOfCombatStateObjectData
		{
			get
			{
				return this._gOutOfCombatStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData GGStateObjectData
		{
			get
			{
				return this._gGStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData GTStateObjectData
		{
			get
			{
				return this._gTStateObjectData;
			}
		}

		public IGadgetStateObjectSerialData SprayStateObjectData
		{
			get
			{
				return this._sprayStateObjectData;
			}
		}

		public float JokerBarValue { get; private set; }

		public float JokerBarMaxValue { get; private set; }

		public void Apply(IGadgetDataSerialData other)
		{
			this._bombExplosionStateObjectData.Apply(other.BombExplosionStateObjectData);
			this._killStateObjectData.Apply(other.KillStateObjectData);
			this._takeoffStateObjectData.Apply(other.TakeoffStateObjectData);
			this._respawnStateObjectData.Apply(other.RespawnStateObjectData);
			this._bombStateObjectData.Apply(other.BombStateObjectData);
			this._gBoostStateObjectData.Apply(other.GBoostStateObjectData);
			this._g0StateObjectData.Apply(other.G0StateObjectData);
			this._g1StateObjectData.Apply(other.G1StateObjectData);
			this._g2StateObjectData.Apply(other.G2StateObjectData);
			this._gPStateObjectData.Apply(other.GPStateObjectData);
			this._gOutOfCombatStateObjectData.Apply(other.GOutOfCombatStateObjectData);
			this._gGStateObjectData.Apply(other.GGStateObjectData);
			this._gTStateObjectData.Apply(other.GTStateObjectData);
			this._sprayStateObjectData.Apply(other.SprayStateObjectData);
			this.JokerBarValue = other.JokerBarValue;
			this.JokerBarMaxValue = other.JokerBarMaxValue;
		}

		public void ApplyStreamData(byte[] data)
		{
			BitStream readStream = StaticBitStream.GetReadStream(data);
			this._bombExplosionStateObjectData.Apply(readStream);
			this._killStateObjectData.Apply(readStream);
			this._takeoffStateObjectData.Apply(readStream);
			this._respawnStateObjectData.Apply(readStream);
			this._bombStateObjectData.Apply(readStream);
			this._gBoostStateObjectData.Apply(readStream);
			this._g0StateObjectData.Apply(readStream);
			this._g1StateObjectData.Apply(readStream);
			this._g2StateObjectData.Apply(readStream);
			this._gPStateObjectData.Apply(readStream);
			this._gOutOfCombatStateObjectData.Apply(readStream);
			this._gGStateObjectData.Apply(readStream);
			this._gTStateObjectData.Apply(readStream);
			this.JokerBarValue = readStream.ReadCompressedFloat();
			this.JokerBarMaxValue = readStream.ReadCompressedFloat();
			this._sprayStateObjectData.Apply(readStream);
		}

		private GadgetStateObjectSerialData _bombExplosionStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _killStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _takeoffStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _respawnStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _bombStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _gBoostStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _g0StateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _g1StateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _g2StateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _gPStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _gOutOfCombatStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _gGStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _gTStateObjectData = new GadgetStateObjectSerialData();

		private GadgetStateObjectSerialData _sprayStateObjectData = new GadgetStateObjectSerialData();
	}
}
