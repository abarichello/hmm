using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavyMetalMachines.UnityUI
{
	public class AnimatedImageMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
	{
		private void OnEnable()
		{
			if (null != this._spritesheetAnimator.texture)
			{
				this._spritesheetAnimator.StartAnimation();
				this._spritesheetAnimator.Stop();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			this.StartAnimation();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			this.StopAnimation();
		}

		public void OnSelect(BaseEventData eventData)
		{
			this.StartAnimation();
		}

		public void OnDeselect(BaseEventData eventData)
		{
			this.StopAnimation();
		}

		private void StartAnimation()
		{
			if (null != this._spritesheetAnimator.texture)
			{
				this._spritesheetAnimator.StartAnimation();
			}
		}

		private void StopAnimation()
		{
			if (null != this._spritesheetAnimator.texture)
			{
				this._spritesheetAnimator.Stop();
			}
		}

		[SerializeField]
		private AnimatedRawImage _spritesheetAnimator;
	}
}
