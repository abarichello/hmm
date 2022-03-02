using System;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class CombatTextFollowTarget : GameHubBehaviour
	{
		protected void Start()
		{
			Canvas[] componentsInParent = base.GetComponentsInParent<Canvas>();
			this._mainCanvasRectTransform = componentsInParent[componentsInParent.Length - 1].GetComponent<RectTransform>();
			this._localTransform = base.transform;
			this._camera = this._gameCameraEngine.UnityCamera;
			this._offsetFromCenterX = this.CombatTextSettings.TargetOffsetFromCenterX;
			this._offsetFromCenterY = this.CombatTextSettings.TargetOffsetFromCenterY;
		}

		protected void LateUpdate()
		{
			if (this._targetTransformId == -1)
			{
				return;
			}
			Vector3 vector = this._camera.WorldToViewportPoint(this._targetTransform.position);
			Vector2 sizeDelta = this._mainCanvasRectTransform.sizeDelta;
			Vector2 vector2;
			vector2..ctor(vector.x * sizeDelta.x, vector.y * sizeDelta.y);
			vector2.x += (float)this._offsetFromCenterX;
			vector2.y += (float)this._offsetFromCenterY;
			this._localTransform.localPosition = vector2;
		}

		public void SetTargetTransform(Transform targetTransform)
		{
			this._targetTransform = targetTransform;
			this._targetTransformId = ((!(targetTransform == null)) ? targetTransform.GetInstanceID() : -1);
		}

		public bool IsTargetTransform(int targetTransformId)
		{
			return this._targetTransformId == targetTransformId;
		}

		public CombatTextSettings CombatTextSettings;

		private Transform _targetTransform;

		private int _targetTransformId = -1;

		private int _offsetFromCenterX;

		private int _offsetFromCenterY;

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		private Camera _camera;

		private Transform _localTransform;

		private RectTransform _mainCanvasRectTransform;
	}
}
