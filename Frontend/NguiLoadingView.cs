using System;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class NguiLoadingView : MonoBehaviour, ILoadingView
	{
		protected void Awake()
		{
			this._mainWidgetAlpha.Alpha = 0f;
		}

		public void Show()
		{
			this._closeAnimation.Stop();
			this._mainWidgetAlpha.Alpha = 1f;
			this._loopAnimation.Play();
		}

		public void Hide()
		{
			this._loopAnimation.Stop();
			if (this._mainWidgetAlpha.Alpha > 0.001f)
			{
				this._closeAnimation.Play();
			}
		}

		[SerializeField]
		private NGUIWidgetAlpha _mainWidgetAlpha;

		[SerializeField]
		private Animation _closeAnimation;

		[SerializeField]
		private Animation _loopAnimation;
	}
}
