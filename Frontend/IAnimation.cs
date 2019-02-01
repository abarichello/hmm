using System;

namespace HeavyMetalMachines.Frontend
{
	public interface IAnimation
	{
		void Play();

		bool IsPlaying { get; }
	}
}
