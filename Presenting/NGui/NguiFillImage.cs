using System;
using HeavyMetalMachines.Extensions;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	public class NguiFillImage : MonoBehaviour, IFillImage
	{
		public float NormalizedFillAmount
		{
			get
			{
				return this._normalizedFillAmount;
			}
			set
			{
				this._normalizedFillAmount = value.Clamp(0f, 1f);
				this.UpdateImage();
			}
		}

		public bool IsActive
		{
			get
			{
				return base.gameObject.activeSelf;
			}
			set
			{
				base.gameObject.SetActive(value);
			}
		}

		public float TransformWidth
		{
			get
			{
				return this._fillImage.cachedTransform.localScale.x;
			}
			set
			{
				this._fillImage.cachedTransform.SetLocalScaleWidth(value);
			}
		}

		private void OnValidate()
		{
			this.UpdateImage();
		}

		private void UpdateImage()
		{
			this._fillImage.width = (int)Mathf.Lerp((float)this._minWidth, (float)this._maxWidth, this._normalizedFillAmount);
		}

		[SerializeField]
		private UI2DSprite _fillImage;

		[SerializeField]
		private int _minWidth;

		[SerializeField]
		private int _maxWidth = 100;

		[SerializeField]
		[Range(0f, 1f)]
		private float _normalizedFillAmount;
	}
}
