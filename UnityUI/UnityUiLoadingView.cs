using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiLoadingView : MonoBehaviour, ILoadingView
	{
		protected void Awake()
		{
			this._mainCanvasGroup.alpha = 0f;
		}

		public void Show()
		{
			this._closeAnimation.Stop();
			this._mainCanvasGroup.alpha = 1f;
			this._loopAnimation.Play();
		}

		public void Hide()
		{
			this._loopAnimation.Stop();
			if (this._mainCanvasGroup.alpha > 0.001f)
			{
				this._closeAnimation.Play();
			}
		}

		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[SerializeField]
		private Animation _closeAnimation;

		[SerializeField]
		private Animation _loopAnimation;
	}
}
