using System;
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
			this._gameCamera = CarCamera.Singleton.GetComponent<Camera>();
			this._offsetFromCenterX = this.CombatTextSettings.TargetOffsetFromCenterX;
			this._offsetFromCenterY = this.CombatTextSettings.TargetOffsetFromCenterY;
		}

		protected void LateUpdate()
		{
			if (this._targetTransformId == -1)
			{
				return;
			}
			Vector3 vector = this._gameCamera.WorldToViewportPoint(this._targetTransform.position);
			Vector2 sizeDelta = this._mainCanvasRectTransform.sizeDelta;
			Vector2 v = new Vector2(vector.x * sizeDelta.x, vector.y * sizeDelta.y);
			v.x += (float)this._offsetFromCenterX;
			v.y += (float)this._offsetFromCenterY;
			this._localTransform.localPosition = v;
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

		private Camera _gameCamera;

		private Transform _localTransform;

		private RectTransform _mainCanvasRectTransform;
	}
}
