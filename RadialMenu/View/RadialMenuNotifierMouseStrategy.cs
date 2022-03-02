using System;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using Hoplon.Math;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.RadialMenu.View
{
	public class RadialMenuNotifierMouseStrategy : IRadialMenuNotifierStrategy
	{
		public RadialMenuNotifierMouseStrategy(RawImage image, RectTransform parentRectTransform, IControllerInputActionPoller inputActionPoller)
		{
			this._image = image;
			this._parentRectTransform = parentRectTransform;
			this._inputActionPoller = inputActionPoller;
		}

		public void SetPosition()
		{
			Vector3 mousePosition = Input.mousePosition;
			Vector2 vector = this._parentRectTransform.ScreenToLocalPosition(mousePosition);
			Vector2 sizeDelta = this._parentRectTransform.sizeDelta;
			Vector2 sizeDelta2 = this._image.rectTransform.sizeDelta;
			float num = sizeDelta.x * 0.5f;
			float num2 = sizeDelta.y * 0.5f;
			float num3 = sizeDelta2.x * 0.5f;
			float num4 = sizeDelta2.y * 0.5f;
			float num5 = Mathf.Clamp(vector.x, -num + num3, num - num3);
			float num6 = Mathf.Clamp(vector.y, -num2 + num4, num2 - num4);
			this._image.rectTransform.anchoredPosition = new Vector2(num5, num6);
		}

		public void Reset()
		{
		}

		public void GetDirectionData(out Vector3 normalizedDirection, out float magnitude)
		{
			Vector2 mousePosition = this._inputActionPoller.GetMousePosition();
			Vector3 vector;
			vector..ctor(mousePosition.x, mousePosition.y);
			Vector3 vector2 = vector - this._image.transform.position;
			normalizedDirection = vector2.normalized;
			magnitude = vector2.sqrMagnitude;
		}

		private readonly RawImage _image;

		private readonly RectTransform _parentRectTransform;

		private readonly IControllerInputActionPoller _inputActionPoller;
	}
}
