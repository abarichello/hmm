using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudEmoteView : MonoBehaviour, IHudEmoteView
	{
		public IAnimation AnimationIn
		{
			get
			{
				return this._animationIn;
			}
		}

		public IAnimation AnimationOut
		{
			get
			{
				return this._animationOut;
			}
		}

		public ITextureMappingUpdater SpritesheetAnimator
		{
			get
			{
				return this._spritesheetAnimator;
			}
		}

		[SerializeField]
		private UnityAnimation _animationIn;

		[SerializeField]
		private UnityAnimation _animationOut;

		[SerializeField]
		private AnimatedRawImage _spritesheetAnimator;
	}
}
