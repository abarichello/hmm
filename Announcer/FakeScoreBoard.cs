using System;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Announcer
{
	public class FakeScoreBoard : IScoreBoard, IBitStreamSerializable
	{
		public void WriteToBitStream(BitStream bs)
		{
			throw new NotImplementedException();
		}

		public void ReadFromBitStream(BitStream bs)
		{
			throw new NotImplementedException();
		}

		public int ScoreRed { get; private set; }

		public int ScoreBlue { get; private set; }

		public int Round { get; private set; }

		public int RoundState { get; private set; }

		public bool IsInOvertime { get; private set; }

		public BombScoreboardState CurrentState { get; private set; }

		public IObservable<ScoreBoardState> StateChangedObservation { get; private set; }
	}
}
