using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;
using Hoplon.Input;
using Hoplon.Logging;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.RadialMenu.View
{
	public class RadialMenuMouseNotifier : MonoBehaviour, IRadialMenuNotifier
	{
		public void Enable()
		{
			base.enabled = true;
		}

		public void Disable()
		{
			base.enabled = false;
		}

		public IObservable<RadialSliceChange> CurrentSliceChanged
		{
			get
			{
				return this._currentSliceChangedSubject;
			}
		}

		private void Awake()
		{
			this._radialMenuNotifierMouseStrategy = new RadialMenuNotifierMouseStrategy(this.image, this._parentRectTransform, this._inputActionPoller);
			this._radialMenuNotifierJoystickStrategy = new RadialMenuNotifierJoystickStrategy(this.image, this._inputActionPoller);
		}

		private void OnEnable()
		{
			this.GetCurrentStrategy().Reset();
			this.GetCurrentStrategy().SetPosition();
			this._currentSliceIndex = -1;
		}

		private IRadialMenuNotifierStrategy GetCurrentStrategy()
		{
			return (this._inputGetActiveDevicePoller.GetActiveDevice() != 3) ? this._radialMenuNotifierMouseStrategy : this._radialMenuNotifierJoystickStrategy;
		}

		private void Update()
		{
			if (!this._currentSliceChangedSubject.HasObservers)
			{
				return;
			}
			Vector3 normalizedDirection;
			float num;
			this.GetCurrentStrategy().GetDirectionData(out normalizedDirection, out num);
			if (num < this._centerDistance)
			{
				this.ChangeCurrentSlice(0);
				return;
			}
			this.UpdateHoveringSlice(normalizedDirection);
		}

		private void UpdateHoveringSlice(Vector3 normalizedDirection)
		{
			float num = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x);
			num = RadialMenuMouseNotifier.RelativeToAbsolute(num);
			if (this._orientation == Orientation.Clockwise)
			{
				num = 6.2831855f - num;
			}
			float num2 = 6.2831855f / (float)this._numberOfSlices;
			num -= this._originAngleDegrees * 0.017453292f - num2 * 0.5f;
			num = RadialMenuMouseNotifier.RelativeToAbsolute(num);
			int num3 = Mathf.FloorToInt(num / num2);
			num3 = Mathf.Clamp(num3, 0, this._numberOfSlices - 1);
			num3++;
			this.ChangeCurrentSlice(num3);
		}

		private static float RelativeToAbsolute(float angleValue)
		{
			if (angleValue < 0f)
			{
				return angleValue + 6.2831855f;
			}
			return angleValue;
		}

		private void ChangeCurrentSlice(int sliceIndex)
		{
			if (sliceIndex == this._currentSliceIndex)
			{
				return;
			}
			this._currentSliceChangedSubject.OnNext(new RadialSliceChange
			{
				PreviousSliceIndex = this._currentSliceIndex,
				CurrentSliceIndex = sliceIndex
			});
			this._currentSliceIndex = sliceIndex;
		}

		private const float TwoPI = 6.2831855f;

		[SerializeField]
		private float _originAngleDegrees;

		[SerializeField]
		private int _numberOfSlices;

		[SerializeField]
		private Orientation _orientation;

		[SerializeField]
		private float _centerDistance;

		[SerializeField]
		private RawImage image;

		[SerializeField]
		private RectTransform _parentRectTransform;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IControllerInputActionPoller _inputActionPoller;

		[InjectOnClient]
		private ILogger<RadialMenuMouseNotifier> _logger;

		private readonly Subject<RadialSliceChange> _currentSliceChangedSubject = new Subject<RadialSliceChange>();

		private IRadialMenuNotifierStrategy _radialMenuNotifierMouseStrategy;

		private IRadialMenuNotifierStrategy _radialMenuNotifierJoystickStrategy;

		private int _currentSliceIndex = -1;
	}
}
