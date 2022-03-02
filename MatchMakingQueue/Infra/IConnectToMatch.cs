using System;
using HeavyMetalMachines.Matches;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public interface IConnectToMatch
	{
		IObservable<Unit> Connect(Match matchState);
	}
}
