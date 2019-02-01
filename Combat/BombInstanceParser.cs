using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class BombInstanceParser : KeyFrameParser
	{
		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.BombInstance;
			}
		}

		public int LastId
		{
			get
			{
				return this.LastFrameId;
			}
		}

		public override void Process(BitStream stream)
		{
			GameHubObject.Hub.BombManager.ActiveBomb.ReadFromBitStream(stream);
			int num = stream.ReadCompressedInt();
			SpawnReason spawnReason = (SpawnReason)stream.ReadCompressedInt();
			GameHubObject.Hub.BombManager.MatchUpdated();
			if (spawnReason != SpawnReason.TriggerDrop && spawnReason != SpawnReason.InputDrop)
			{
				switch (spawnReason)
				{
				case SpawnReason.Grabbed:
					GameHubObject.Hub.BombManager.OnBombCarrierChanged(num);
					return;
				default:
					if (spawnReason != SpawnReason.Death)
					{
						return;
					}
					break;
				case SpawnReason.BrokenLink:
					break;
				}
			}
			GameHubObject.Hub.BombManager.OnBombDropped(num, spawnReason);
		}

		public void Update(int causerId, SpawnReason reason)
		{
			BitStream stream = base.GetStream();
			GameHubObject.Hub.BombManager.ActiveBomb.WriteToBitStream(stream);
			stream.WriteCompressedInt(causerId);
			stream.WriteCompressedInt((int)reason);
			this.SendKeyframe(stream.ToArray());
		}

		public void UpdateDataTo(byte playerAddress)
		{
			BitStream stream = base.GetStream();
			BombInstance activeBomb = GameHubObject.Hub.BombManager.ActiveBomb;
			activeBomb.WriteToBitStream(stream);
			int value = (activeBomb.BombCarriersIds.Count <= 0) ? -1 : activeBomb.BombCarriersIds[0];
			stream.WriteCompressedInt(value);
			stream.WriteCompressedInt(11);
			this.SendFullFrame(playerAddress, stream.ToArray());
		}
	}
}
