using System;
using System.Diagnostics;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class GadgetData : StreamContent
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GadgetData.GadgetStateChanged OnGadgetStateChanged;

		public GadgetData.GadgetStateObject GetGadgetState(GadgetSlot slot)
		{
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				return this.G0StateObject;
			case GadgetSlot.CustomGadget1:
				return this.G1StateObject;
			case GadgetSlot.CustomGadget2:
				return this.G2StateObject;
			case GadgetSlot.BoostGadget:
				return this.GBoostStateObject;
			case GadgetSlot.PassiveGadget:
				return this.GPStateObject;
			case GadgetSlot.GenericGadget:
				return this.GGStateObject;
			case GadgetSlot.RespawnGadget:
				return this.RespawnStateObject;
			case GadgetSlot.BombGadget:
				return this.BombStateObject;
			case GadgetSlot.OutOfCombatGadget:
				return this.GOutOfCombatStateObject;
			case GadgetSlot.TrailGadget:
				return this.GTStateObject;
			case GadgetSlot.TakeoffGadget:
				return this.TakeoffStateObject;
			case GadgetSlot.KillGadget:
				return this.KillStateObject;
			case GadgetSlot.BombExplosionGadget:
				return this.BombExplosionStateObject;
			case GadgetSlot.SprayGadget:
				return this.SprayStateObject;
			case GadgetSlot.GridHighlightGadget:
				return this.GridHighlightStateObject;
			}
			return null;
		}

		public void SetEffectState(GadgetData.GadgetStateObject obj, EffectState effectState)
		{
			if (obj == null || obj.EffectState == effectState)
			{
				return;
			}
			obj.EffectState = effectState;
			GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
		}

		public void SetGadgetState(GadgetData.GadgetStateObject obj, GadgetSlot slot, GadgetState gadgetState, long cooldown, int value, float heat, int counter, int[] affectedIds = null)
		{
			if (obj != null)
			{
				if (obj.GadgetState != gadgetState)
				{
					GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
				}
				obj.GadgetState = gadgetState;
				if (obj.CoolDown != cooldown)
				{
					GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
				}
				obj.CoolDown = cooldown;
				if (obj.Value != value)
				{
					GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
				}
				obj.Value = value;
				if (obj.Heat != heat)
				{
					GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
				}
				obj.Heat = heat;
				if (obj.Counter != counter)
				{
					GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
				}
				obj.Counter = counter;
				if (affectedIds != null && obj.AffectedIds != null && obj.AffectedIds.Length == affectedIds.Length)
				{
					for (int i = 0; i < affectedIds.Length; i++)
					{
						if (obj.AffectedIds[i] != affectedIds[i])
						{
							GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
							break;
						}
					}
				}
				else if (obj.AffectedIds != affectedIds)
				{
					GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
				}
				obj.AffectedIds = affectedIds;
			}
			if (this.OnGadgetStateChanged != null)
			{
				this.OnGadgetStateChanged(slot, gadgetState);
			}
		}

		public void SetJokerBarState(float value, float maxvalue)
		{
			if (this.GadgetJokeBarState.Value != value)
			{
				GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
			}
			this.GadgetJokeBarState.Value = value;
			if (this.GadgetJokeBarState.MaxValue != maxvalue)
			{
				GameHubBehaviour.Hub.Stream.GadgetDataStream.Changed(this);
			}
			this.GadgetJokeBarState.MaxValue = maxvalue;
		}

		public override int GetStreamData(ref byte[] data, bool boForceSerialization)
		{
			BitStream stream = base.GetStream();
			if (boForceSerialization)
			{
				this.BombExplosionStateObject.InvalidateCache();
				this.KillStateObject.InvalidateCache();
				this.TakeoffStateObject.InvalidateCache();
				this.RespawnStateObject.InvalidateCache();
				this.BombStateObject.InvalidateCache();
				this.GBoostStateObject.InvalidateCache();
				this.G0StateObject.InvalidateCache();
				this.G1StateObject.InvalidateCache();
				this.G2StateObject.InvalidateCache();
				this.GPStateObject.InvalidateCache();
				this.GOutOfCombatStateObject.InvalidateCache();
				this.GSAStateObject.InvalidateCache();
				this.GSPStateObject.InvalidateCache();
				this.GGStateObject.InvalidateCache();
				this.GTStateObject.InvalidateCache();
				this.SprayStateObject.InvalidateCache();
			}
			this.BombExplosionStateObject.WriteToBitStream(stream);
			this.KillStateObject.WriteToBitStream(stream);
			this.TakeoffStateObject.WriteToBitStream(stream);
			this.RespawnStateObject.WriteToBitStream(stream);
			this.BombStateObject.WriteToBitStream(stream);
			this.GBoostStateObject.WriteToBitStream(stream);
			this.G0StateObject.WriteToBitStream(stream);
			this.G1StateObject.WriteToBitStream(stream);
			this.G2StateObject.WriteToBitStream(stream);
			this.GPStateObject.WriteToBitStream(stream);
			this.GOutOfCombatStateObject.WriteToBitStream(stream);
			this.GSAStateObject.WriteToBitStream(stream);
			this.GSPStateObject.WriteToBitStream(stream);
			this.GGStateObject.WriteToBitStream(stream);
			this.GTStateObject.WriteToBitStream(stream);
			this.GadgetJokeBarState.WriteToBitStream(stream);
			this.SprayStateObject.WriteToBitStream(stream);
			return stream.CopyToArray(data);
		}

		public override void ApplyStreamData(byte[] data)
		{
			BitStream streamFor = base.GetStreamFor(data);
			this.BombExplosionStateObject.ReadFromBitStream(streamFor);
			this.KillStateObject.ReadFromBitStream(streamFor);
			this.TakeoffStateObject.ReadFromBitStream(streamFor);
			this.RespawnStateObject.ReadFromBitStream(streamFor);
			this.BombStateObject.ReadFromBitStream(streamFor);
			this.GBoostStateObject.ReadFromBitStream(streamFor);
			this.G0StateObject.ReadFromBitStream(streamFor);
			this.G1StateObject.ReadFromBitStream(streamFor);
			this.G2StateObject.ReadFromBitStream(streamFor);
			this.GPStateObject.ReadFromBitStream(streamFor);
			this.GOutOfCombatStateObject.ReadFromBitStream(streamFor);
			this.GSAStateObject.ReadFromBitStream(streamFor);
			this.GSPStateObject.ReadFromBitStream(streamFor);
			this.GGStateObject.ReadFromBitStream(streamFor);
			this.GTStateObject.ReadFromBitStream(streamFor);
			this.GadgetJokeBarState.ReadFromBitStream(streamFor);
			this.SprayStateObject.ReadFromBitStream(streamFor);
		}

		public readonly GadgetData.GadgetStateObject RespawnStateObject = new GadgetData.GadgetStateObject(GadgetSlot.RespawnGadget);

		public readonly GadgetData.GadgetStateObject BombStateObject = new GadgetData.GadgetStateObject(GadgetSlot.BombGadget);

		public readonly GadgetData.GadgetStateObject G0StateObject = new GadgetData.GadgetStateObject(GadgetSlot.CustomGadget0);

		public readonly GadgetData.GadgetStateObject G1StateObject = new GadgetData.GadgetStateObject(GadgetSlot.CustomGadget1);

		public readonly GadgetData.GadgetStateObject G2StateObject = new GadgetData.GadgetStateObject(GadgetSlot.CustomGadget2);

		public readonly GadgetData.GadgetStateObject GBoostStateObject = new GadgetData.GadgetStateObject(GadgetSlot.BoostGadget);

		public readonly GadgetData.GadgetStateObject GPStateObject = new GadgetData.GadgetStateObject(GadgetSlot.PassiveGadget);

		public readonly GadgetData.GadgetStateObject GTStateObject = new GadgetData.GadgetStateObject(GadgetSlot.TrailGadget);

		public readonly GadgetData.GadgetStateObject GGStateObject = new GadgetData.GadgetStateObject(GadgetSlot.GenericGadget);

		public readonly GadgetData.GadgetStateObject GOutOfCombatStateObject = new GadgetData.GadgetStateObject(GadgetSlot.OutOfCombatGadget);

		public readonly GadgetData.GadgetStateObject TakeoffStateObject = new GadgetData.GadgetStateObject(GadgetSlot.TakeoffGadget);

		public readonly GadgetData.GadgetStateObject KillStateObject = new GadgetData.GadgetStateObject(GadgetSlot.KillGadget);

		public readonly GadgetData.GadgetStateObject BombExplosionStateObject = new GadgetData.GadgetStateObject(GadgetSlot.BombExplosionGadget);

		public readonly GadgetData.GadgetStateObject SprayStateObject = new GadgetData.GadgetStateObject(GadgetSlot.SprayGadget);

		public readonly GadgetData.GadgetStateObject GridHighlightStateObject = new GadgetData.GadgetStateObject(GadgetSlot.GridHighlightGadget);

		public readonly GadgetData.GadgetStateObject GSAStateObject = new GadgetData.GadgetStateObject(GadgetSlot.None);

		public readonly GadgetData.GadgetStateObject GSPStateObject = new GadgetData.GadgetStateObject(GadgetSlot.None);

		public readonly GadgetData.GadgetJokeBarStateObject GadgetJokeBarState = new GadgetData.GadgetJokeBarStateObject();

		public delegate void GadgetStateChanged(GadgetSlot slot, GadgetState gadgetState);

		[Serializable]
		public class GadgetStateObject : IBitStreamSerializable
		{
			public GadgetStateObject(GadgetSlot slot)
			{
				this._slot = slot;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event Action ListenToGadgetActivation;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event Action ListenToGadgetWarmupStarted;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event Action ListenToGadgetReady;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event Action ListenToGadGetStop;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event Action ListenToGadgetEnterCooldown;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event Action<int> ClientListenToGadgetHit;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event Action<int> ListenToValueChange;

			public EffectState EffectState
			{
				get
				{
					return this._effectState;
				}
				set
				{
					if (this._effectState == value)
					{
						return;
					}
					if (value != EffectState.Running)
					{
						if (value != EffectState.Idle)
						{
							if (value == EffectState.Warmup)
							{
								if (this.ListenToGadgetWarmupStarted != null)
								{
									this.ListenToGadgetWarmupStarted();
								}
							}
						}
						else if (this.ListenToGadGetStop != null)
						{
							this.ListenToGadGetStop();
						}
					}
					else if (this.ListenToGadgetActivation != null)
					{
						this.ListenToGadgetActivation();
					}
					this._effectState = value;
				}
			}

			public int Value
			{
				get
				{
					return this._value;
				}
				set
				{
					if (this._value != value && this.ListenToValueChange != null)
					{
						this.ListenToValueChange(value);
					}
					this._value = value;
				}
			}

			public void ClientGadgetHit(int otherId)
			{
				if (this.ClientListenToGadgetHit != null)
				{
					this.ClientListenToGadgetHit(otherId);
				}
			}

			public void InvalidateCache()
			{
				this._cachedData.Invalidate();
			}

			public void WriteToBitStream(BitStream bs)
			{
				this._cachedData.UpdateCount();
				if (this._cachedData.HasChanged(this.CoolDown, this.Value, this.Heat, this.Counter, this.GadgetState, this.EffectState))
				{
					bs.WriteBool(true);
					bs.WriteCompressedLong(this.CoolDown);
					bs.WriteCompressedInt(this.Value);
					bs.WriteTinyUFloat(this.Heat);
					bs.WriteCompressedInt(this.Counter);
					bs.WriteIntArray(this.AffectedIds);
					bs.WriteBits(4, (int)this.GadgetState);
					bs.WriteBits(4, (int)this.EffectState);
				}
				else
				{
					bs.WriteBool(false);
				}
			}

			public void ReadFromBitStream(BitStream bs)
			{
				if (!bs.ReadBool())
				{
					return;
				}
				GadgetState gadgetState = this.GadgetState;
				this.CoolDown = bs.ReadCompressedLong();
				this.Value = bs.ReadCompressedInt();
				this.Heat = bs.ReadTinyUFloat();
				this.Counter = bs.ReadCompressedInt();
				this.AffectedIds = bs.ReadIntArray();
				this.GadgetState = (GadgetState)bs.ReadBits(4);
				this.EffectState = (EffectState)bs.ReadBits(4);
				if (gadgetState != this.GadgetState)
				{
					GadgetState gadgetState2 = this.GadgetState;
					if (gadgetState2 != GadgetState.Ready)
					{
						if (gadgetState2 == GadgetState.Cooldown)
						{
							if (this.ListenToGadgetEnterCooldown != null)
							{
								this.ListenToGadgetEnterCooldown();
							}
						}
					}
					else if (this.ListenToGadgetReady != null)
					{
						this.ListenToGadgetReady();
					}
				}
			}

			private GadgetSlot _slot;

			private EffectState _effectState;

			public GadgetState GadgetState;

			public long CoolDown;

			private int _value;

			public float Heat;

			public int Counter;

			public int[] AffectedIds;

			private GadgetData.GadgetStateObject.NetworkCachedData _cachedData = new GadgetData.GadgetStateObject.NetworkCachedData();

			private class NetworkCachedData
			{
				public void Invalidate()
				{
					this.currentUpdateCount = this.fullUpdateFrequency;
				}

				public void UpdateCount()
				{
					this.currentUpdateCount++;
				}

				public bool HasChanged(long coolDown, int value, float heat, int counter, GadgetState gadgetState, EffectState effectState)
				{
					if (this.lastCoolDown != coolDown || this.lastValue != value || this.lastHeat != heat || this.lastCounter != counter || this.lastGadgetState != gadgetState || this.lastEffectState != effectState || this.currentUpdateCount >= this.fullUpdateFrequency)
					{
						this.lastCoolDown = coolDown;
						this.lastValue = value;
						this.lastHeat = heat;
						this.lastCounter = counter;
						this.lastGadgetState = gadgetState;
						this.lastEffectState = effectState;
						this.currentUpdateCount = 0;
						return true;
					}
					return false;
				}

				private long lastCoolDown;

				private int lastValue;

				private float lastHeat;

				private int lastCounter;

				private GadgetState lastGadgetState = GadgetState.None;

				private EffectState lastEffectState = EffectState.Running;

				private int fullUpdateFrequency = 30;

				private int currentUpdateCount;
			}
		}

		[Serializable]
		public class GadgetJokeBarStateObject : IBitStreamSerializable
		{
			public void WriteToBitStream(BitStream bs)
			{
				bs.WriteCompressedFloat(this.Value);
				bs.WriteCompressedFloat(this.MaxValue);
			}

			public void ReadFromBitStream(BitStream bs)
			{
				this.Value = bs.ReadCompressedFloat();
				this.MaxValue = bs.ReadCompressedFloat();
			}

			public float Value;

			public float MaxValue;
		}
	}
}
