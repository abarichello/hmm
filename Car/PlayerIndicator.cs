using System;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	internal class PlayerIndicator : MonoBehaviour
	{
		public bool IsBorderEnabled
		{
			get
			{
				return this._isBorderEnable;
			}
		}

		public void Intialize(Texture borderTexture, Transform indicatorTransform, Texture driftTexture)
		{
			this._borderTransform = indicatorTransform;
			this._borderMeshRenderer = indicatorTransform.GetComponent<MeshRenderer>();
			this._borderMeshRenderer.material.SetTexture("_MainTex", borderTexture);
			this._driftIndicatorLeft = indicatorTransform.GetChild(0);
			this._driftIndicatorRight = indicatorTransform.GetChild(1);
			this._leftDriftRender = this._driftIndicatorLeft.GetComponent<MeshRenderer>();
			this._leftDriftRender.material.SetTexture("_MainTex", driftTexture);
			this._driftLeftMarkMaterial = this._leftDriftRender.material;
			this._rightDriftRender = this._driftIndicatorRight.GetComponent<MeshRenderer>();
			this._rightDriftRender.material.SetTexture("_MainTex", driftTexture);
			this._driftRightMarkMaterial = this._rightDriftRender.material;
			this._borderAnimation = new PlayerIndicatorBorderAnimation(this._borderTransform, new MeshRenderer[]
			{
				this._borderMeshRenderer,
				this._leftDriftRender,
				this._rightDriftRender
			});
			this._materialPropertyBlock = new MaterialPropertyBlock();
		}

		private void Update()
		{
			this._borderAnimation.UpdateAnimation();
			if (!this._updateDriftPosition)
			{
				return;
			}
			float angle = Mathf.Lerp(this._currentDriftAngle, this._destinationDriftAngle, this._lerpTime);
			this.UpdateDriftMark(angle, this._radius);
			this._lerpTime += 2f * Time.deltaTime;
			if (Mathf.Abs(this._currentDriftAngle - this._destinationDriftAngle) < 0.001f)
			{
				this._updateDriftPosition = false;
				this._lerpTime = 0f;
			}
		}

		public void SetupPlayerIndicator(float angle, float scale, float offset)
		{
			if (this._hasDoneSetup)
			{
				return;
			}
			float radius = 2.85f * scale;
			this._borderTransform.localScale = new Vector3(scale, 1f, scale);
			this._borderTransform.position += this._borderTransform.forward * offset;
			this._radius = radius;
			this._maxDriftAngle = angle;
			this._destinationDriftAngle = this._maxDriftAngle;
			this.UpdateDriftMark(this._maxDriftAngle, this._radius);
			this._hasDoneSetup = true;
		}

		public void SetPlayerIndicationBorderAnimationConfig(PlayerIndicatorBorderAnimationConfig config)
		{
			this._borderAnimation.SetConfig(config);
		}

		private void UpdateDriftMark(float angle, float radius)
		{
			this._currentDriftAngle = angle;
			this._driftIndicatorLeft.localRotation = Quaternion.AngleAxis(-angle, Vector3.up);
			this._driftIndicatorRight.localRotation = Quaternion.AngleAxis(angle, Vector3.up);
			float num = (-this._borderTransform.rotation.eulerAngles.y + angle) * 0.017453292f;
			this._auxAnglePosition.x = -Mathf.Sin(num);
			this._auxAnglePosition.z = Mathf.Cos(num);
			this._driftIndicatorLeft.position = this._borderTransform.position + this._auxAnglePosition * radius;
			num = (-this._borderTransform.rotation.eulerAngles.y - angle) * 0.017453292f;
			this._auxAnglePosition.x = -Mathf.Sin(num);
			this._auxAnglePosition.z = Mathf.Cos(num);
			this._driftIndicatorRight.position = this._borderTransform.position + this._auxAnglePosition * radius;
		}

		public void UpdatePlayerBorder(bool isReverse)
		{
			if (this._wasReverse == isReverse || this._nextDelayClick > Time.time || !this._isBorderEnable)
			{
				return;
			}
			this._nextDelayClick = 0.25f + Time.time;
			this._wasReverse = isReverse;
			this._updateDriftPosition = true;
			this._lerpTime = 0f;
			if (isReverse)
			{
				this._destinationDriftAngle = 180f - this._maxDriftAngle;
				return;
			}
			this._destinationDriftAngle = this._maxDriftAngle;
		}

		public void SetVisibilityAnimated(bool isVisible)
		{
			if (this._isBorderEnable && !this._borderAnimation.IsInProgress)
			{
				this._borderAnimation.Trigger(delegate
				{
					this.SetVisibility(isVisible);
				});
			}
			else
			{
				this.SetVisibility(isVisible);
			}
		}

		public void SetVisibility(bool isVisible)
		{
			this._isBorderVisible = isVisible;
			this._borderTransform.gameObject.SetActive(isVisible && this._isBorderEnable);
		}

		public void SetBorderEnable(bool isEnable)
		{
			this._isBorderEnable = isEnable;
			this.SetVisibility(this._isBorderVisible);
		}

		public void SetBorderAlpha(float alpha)
		{
			if (this._borderAnimation != null && this._borderAnimation.IsInProgress)
			{
				this._borderAnimation.SetupExternalCleanupAction(delegate
				{
					this.SetBorderAlpha(alpha);
				});
				return;
			}
			Color color = this._borderMeshRenderer.material.GetColor("_TintColor");
			color.a = alpha;
			this._materialPropertyBlock.SetColor("_TintColor", color);
			this._borderMeshRenderer.SetPropertyBlock(this._materialPropertyBlock);
			Color color2 = this._driftLeftMarkMaterial.GetColor("_TintColor");
			color2.a = alpha * 1.8f;
			this._materialPropertyBlock.SetColor("_TintColor", color2);
			this._leftDriftRender.SetPropertyBlock(this._materialPropertyBlock);
			this._rightDriftRender.SetPropertyBlock(this._materialPropertyBlock);
		}

		private MeshRenderer _borderMeshRenderer;

		private Material _driftLeftMarkMaterial;

		private Material _driftRightMarkMaterial;

		private Transform _driftIndicatorLeft;

		private Transform _driftIndicatorRight;

		private Transform _borderTransform;

		private MeshRenderer _leftDriftRender;

		private MeshRenderer _rightDriftRender;

		private bool _isBorderEnable;

		private bool _isBorderVisible;

		private float _borderOpacity;

		private bool _wasReverse;

		private Vector3 _auxAnglePosition;

		private float _radius;

		private bool _updateDriftPosition;

		private float _destinationDriftAngle;

		private float _maxDriftAngle;

		private float _currentDriftAngle;

		private float _lerpTime;

		private const float DRIFTMARK_ANIM_SPEED = 2f;

		private MaterialPropertyBlock _materialPropertyBlock;

		private const float DELAY_TO_CHECK_NEXT_INPUT = 0.25f;

		private float _nextDelayClick;

		private const float RADIUS_TO_SCALE_FACTOR = 2.85f;

		private const string DEFAULT_TINT_MATERIAL = "_TintColor";

		private const float DRIFTMARK_TO_BORDER_ALPHA_FACTOR = 1.8f;

		[SerializeField]
		private PlayerIndicatorBorderAnimation _borderAnimation;

		private bool _hasDoneSetup;
	}
}
