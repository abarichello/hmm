using System;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public interface ITimelineController
	{
		int TimelineSizeMillis { get; }

		float[] AvailableSpeeds { get; }

		int CurrentSpeedIndex { get; }

		bool IsPlaying { get; }

		void Pause();

		void Play();

		int IncreaseSpeed();

		int DecreaseSpeed();

		void SetTime(int targetTime);

		IObservable<int> ObserveSpeedChange();

		IObservable<bool> ObserveAvailability();
	}
}
