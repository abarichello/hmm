using System;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public interface IGadgetHudElement
	{
		Animation[] AnimationControllers { get; }

		HMMUI2DDynamicSprite SpriteController { get; }

		GaugeHudElement Gauge { get; }

		RadialTimerHudElement Radial { get; }

		TimerHudElement Timer { get; }
	}
}
