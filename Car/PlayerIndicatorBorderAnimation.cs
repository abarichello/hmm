using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	public class PlayerIndicatorBorderAnimation
	{
		public PlayerIndicatorBorderAnimation(Transform targetTransform, params MeshRenderer[] targetMeshRenderers)
		{
			this._targetTransform = targetTransform;
			this._targetMeshRenderers = targetMeshRenderers;
			this._targetAlphas = new float[this._targetMeshRenderers.Length];
			this._materialPropertyBlock = new MaterialPropertyBlock();
		}

		public bool IsInProgress { get; private set; }

		public void SetConfig(PlayerIndicatorBorderAnimationConfig playerIndicatorBorderAnimationConfig)
		{
			this._config = playerIndicatorBorderAnimationConfig;
		}

		public void Trigger(Action callback = null)
		{
			this.IsInProgress = true;
			this._triggerCallback = callback;
			this._targetTransform.gameObject.SetActive(true);
			this._externalCleanupAction = null;
			this.InitializeScale();
			this.InitializeAlphas();
			this._timer = 0f;
		}

		public void SetupExternalCleanupAction(Action action)
		{
			this._externalCleanupAction = action;
		}

		private void InitializeScale()
		{
			this._targetSize = this._targetTransform.localScale.x;
			Vector3 localScale = this._targetTransform.localScale;
			localScale.x = this._config.StartingSize;
			localScale.z = this._config.StartingSize;
			this._targetTransform.localScale = localScale;
		}

		private void InitializeAlphas()
		{
			for (int i = 0; i < this._targetMeshRenderers.Length; i++)
			{
				MeshRenderer meshRenderer = this._targetMeshRenderers[i];
				meshRenderer.GetPropertyBlock(this._materialPropertyBlock);
				Vector4 vector = this._materialPropertyBlock.GetVector("_TintColor");
				this._targetAlphas[i] = vector.w;
				this.SetMeshAlpha(meshRenderer, this._config.StartingAlpha);
			}
		}

		public void UpdateAnimation()
		{
			if (!this.IsInProgress)
			{
				return;
			}
			float num = this._config.DurationInSeconds * 1000f;
			float num2 = this._timer / num;
			num2 *= this._config.AnimationCurve.Evaluate(num2);
			this.UpdateScale(num2);
			this.UpdateAlpha(num2);
			if (Mathf.Approximately(this._timer, num))
			{
				this.Cleanup();
				this.InvokeTriggerCallback();
				return;
			}
			this._timer = Mathf.Min(num, this._timer + Time.deltaTime * 1000f);
		}

		private void UpdateScale(float progress)
		{
			Vector3 localScale = this._targetTransform.localScale;
			localScale.x = Mathf.Lerp(this._config.StartingSize, this._targetSize, progress);
			localScale.z = localScale.x;
			this._targetTransform.localScale = localScale;
		}

		private void UpdateAlpha(float progress)
		{
			for (int i = 0; i < this._targetMeshRenderers.Length; i++)
			{
				float b = this._targetAlphas[i];
				float newAlpha = Mathf.Lerp(this._config.StartingAlpha, b, progress);
				MeshRenderer renderer = this._targetMeshRenderers[i];
				this.SetMeshAlpha(renderer, newAlpha);
			}
		}

		private void SetMeshAlpha(Renderer renderer, float newAlpha)
		{
			Color color = renderer.material.GetColor("_TintColor");
			color.a = newAlpha;
			this._materialPropertyBlock.SetColor("_TintColor", color);
			renderer.SetPropertyBlock(this._materialPropertyBlock);
		}

		private void Cleanup()
		{
			this.IsInProgress = false;
			this.RestoreScale();
			this.RestoreAlpha();
			this.InvokeExternalCleanupAction();
		}

		private void InvokeTriggerCallback()
		{
			if (this._triggerCallback != null)
			{
				this._triggerCallback();
			}
		}

		private void InvokeExternalCleanupAction()
		{
			if (this._externalCleanupAction != null)
			{
				this._externalCleanupAction();
			}
		}

		private void RestoreScale()
		{
			Vector3 localScale = new Vector3(this._targetSize, this._targetTransform.localScale.y, this._targetSize);
			this._targetTransform.localScale = localScale;
		}

		private void RestoreAlpha()
		{
			for (int i = 0; i < this._targetMeshRenderers.Length; i++)
			{
				MeshRenderer renderer = this._targetMeshRenderers[i];
				float newAlpha = this._targetAlphas[i];
				this.SetMeshAlpha(renderer, newAlpha);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerIndicatorBorderAnimation));

		private const string PlayerIndicatorShaderColorTag = "_TintColor";

		private readonly Transform _targetTransform;

		private readonly MeshRenderer[] _targetMeshRenderers;

		private readonly float[] _targetAlphas;

		private float _targetSize;

		private float _timer;

		private Action _externalCleanupAction;

		private readonly MaterialPropertyBlock _materialPropertyBlock;

		[SerializeField]
		private PlayerIndicatorBorderAnimationConfig _config;

		private Action _triggerCallback;
	}
}
