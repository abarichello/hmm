using System;
using Hoplon.Reactive;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Reactive
{
	public class GameObservable : IGameObservable
	{
		public IObservable<float> EveryFrame()
		{
			return Observable.Select<long, float>(Observable.EveryUpdate(), (long _) => Time.deltaTime);
		}
	}
}
