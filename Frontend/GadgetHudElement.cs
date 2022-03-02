using System;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	internal class GadgetHudElement : MonoBehaviour, IGadgetHudElement
	{
		public Animation[] AnimationControllers
		{
			get
			{
				return this._animationControllers;
			}
		}

		public HMMUI2DDynamicSprite SpriteController
		{
			get
			{
				return this._spriteController;
			}
		}

		public GaugeHudElement Gauge
		{
			get
			{
				return this._gauge;
			}
		}

		public RadialTimerHudElement Radial
		{
			get
			{
				return this._radial;
			}
		}

		public TimerHudElement Timer
		{
			get
			{
				return this._timer;
			}
		}

		[SerializeField]
		private Animation[] _animationControllers;

		[SerializeField]
		private HMMUI2DDynamicSprite _spriteController;

		[SerializeField]
		private GaugeHudElement _gauge;

		[SerializeField]
		private RadialTimerHudElement _radial;

		[SerializeField]
		private TimerHudElement _timer;
	}
}
