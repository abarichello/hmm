using System;
using HeavyMetalMachines.Input.ControllerInput;
using Hoplon.Math;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.RadialMenu.View
{
	public class RadialMenuNotifierJoystickStrategy : IRadialMenuNotifierStrategy
	{
		public RadialMenuNotifierJoystickStrategy(RawImage image, IControllerInputActionPoller inputActionPoller)
		{
			this._image = image;
			this._inputActionPoller = inputActionPoller;
		}

		public void SetPosition()
		{
			this._image.transform.position = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
		}

		public void Reset()
		{
			this._lastMagnitude = 0f;
		}

		public void GetDirectionData(out Vector3 normalizedDirection, out float magnitude)
		{
			bool flag;
			Vector2 compositeAxis = this._inputActionPoller.GetCompositeAxis(22, 21, ref flag);
			normalizedDirection..ctor(compositeAxis.x, compositeAxis.y);
			if (flag)
			{
				magnitude = 0f;
				magnitude = this._lastMagnitude;
				normalizedDirection = this._lastNormalizedDirection;
			}
			else
			{
				magnitude = float.MaxValue;
				this._lastNormalizedDirection = normalizedDirection;
				this._lastMagnitude = magnitude;
			}
		}

		private RawImage _image;

		private readonly IControllerInputActionPoller _inputActionPoller;

		private Vector3 _lastNormalizedDirection;

		private float _lastMagnitude;
	}
}
