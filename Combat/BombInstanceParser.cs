using System;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Combat
{
	public class BombInstanceParser : KeyFrameParser, IBombInstanceDispatcher
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
			this._bombManager.UpdateBombInstance(stream);
		}

		public void Update(int causerId, BombInstance bomb, SpawnReason reason)
		{
			BitStream stream = base.GetStream();
			bomb.WriteToBitStream(stream);
			stream.WriteCompressedInt(causerId);
			stream.WriteCompressedInt((int)reason);
			this.SendKeyframe(stream.ToArray());
		}

		public void UpdateDataTo(byte playerAddress, BombInstance bomb)
		{
			BitStream stream = base.GetStream();
			bomb.WriteToBitStream(stream);
			int value = (bomb.BombCarriersIds.Count <= 0) ? -1 : bomb.BombCarriersIds[0];
			stream.WriteCompressedInt(value);
			stream.WriteCompressedInt(11);
			this.SendFullFrame(playerAddress, stream.ToArray());
		}

		[Inject]
		private IBombManager _bombManager;
	}
}
