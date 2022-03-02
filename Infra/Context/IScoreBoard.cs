using System;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IScoreBoard : IBitStreamSerializable
	{
		int ScoreRed { get; }

		int ScoreBlue { get; }

		int Round { get; }

		int RoundState { get; }

		bool IsInOvertime { get; }

		BombScoreboardState CurrentState { get; }

		IObservable<ScoreBoardState> StateChangedObservation { get; }
	}
}
