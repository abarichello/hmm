using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UnityAnimation : IAnimation
	{
		public UnityAnimation(Animation animation, string stateName)
		{
			this._animation = animation;
			this._stateName = stateName;
		}

		public void Play()
		{
			this._animation.Play(this._stateName);
		}

		public bool IsPlaying
		{
			get
			{
				return this._animation.isPlaying;
			}
		}

		private readonly Animation _animation;

		private readonly string _stateName;
	}
}
