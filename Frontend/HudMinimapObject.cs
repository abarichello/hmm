using System;
using HeavyMetalMachines.Arena;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public abstract class HudMinimapObject : GameHubBehaviour
	{
		public virtual void Setup()
		{
			this.Setup(true);
		}

		public virtual void Setup(bool compensateRotation)
		{
			if (compensateRotation)
			{
				Quaternion localRotation = base.transform.parent.localRotation;
				localRotation.z = -localRotation.z;
				base.transform.localRotation = localRotation;
			}
			this.CalculateMinimapProportions();
		}

		public void CalculateMinimapProportions()
		{
			IGameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			float preferredWidth = this._minimapBackgroundImage.preferredWidth;
			float preferredHeight = this._minimapBackgroundImage.preferredHeight;
			float num = (float)currentArena.MapSize;
			this._flipPinVerticalPosition = currentArena.FlipPinVerticalPosition;
			this._flipPinHorizontalPosition = currentArena.FlipPinHorizontalPosition;
			this._minimapProportion = new Vector2(preferredWidth / num, preferredHeight / num);
		}

		public abstract void OnUpdate();

		protected void UpdatePosition(Vector3 targetPosition)
		{
			this.UpdatePosition(base.transform, targetPosition);
		}

		protected void UpdatePosition(Transform pin, Vector3 targetPosition)
		{
			float x = this._minimapProportion.x * targetPosition.x;
			float y = this._minimapProportion.y * targetPosition.z;
			this.FlipLocalPositionWhenNecessary(ref x, ref y);
			this.SetPinLocalPosition(pin, x, y);
		}

		private void FlipLocalPositionWhenNecessary(ref float x, ref float y)
		{
			if (this._flipPinHorizontalPosition)
			{
				x = -x;
			}
			if (this._flipPinVerticalPosition)
			{
				y = -y;
			}
		}

		private void SetPinLocalPosition(Transform pin, float x, float y)
		{
			this._pinLocalPosition.x = x;
			this._pinLocalPosition.y = y;
			pin.localPosition = this._pinLocalPosition;
		}

		protected void UpdateRotation(Transform localGuiTransform, Quaternion targetRotation)
		{
			Quaternion localRotation = localGuiTransform.localRotation;
			Vector3 eulerAngles = localRotation.eulerAngles;
			float num = -targetRotation.eulerAngles.y;
			eulerAngles.Set(0f, 0f, num);
			localRotation.eulerAngles = eulerAngles;
			localGuiTransform.localRotation = localRotation;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudMinimapObject));

		[NonSerialized]
		protected Transform TargetTransform;

		[SerializeField]
		private Image _minimapBackgroundImage;

		private Vector2 _minimapProportion = Vector2.one;

		private Vector2 _pinLocalPosition;

		private bool _flipPinVerticalPosition;

		private bool _flipPinHorizontalPosition;
	}
}
