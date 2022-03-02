using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	[RequireComponent(typeof(UIWidget))]
	public class NguiUnityUiCollider : MonoBehaviour
	{
		protected void Start()
		{
			UIWidget component = base.GetComponent<UIWidget>();
			GameObject gameObject = new GameObject("UnityUI Object");
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.layer = base.gameObject.layer;
			gameObject.transform.SetSiblingIndex(0);
			RawImage rawImage = gameObject.AddComponent<RawImage>();
			rawImage.SetAlpha(0f);
			rawImage.rectTransform.sizeDelta = component.localSize;
			this._canvas = gameObject.AddComponent<Canvas>();
			this._canvas.overrideSorting = true;
			this._parentPanel = base.GetComponentInParent<UIPanel>();
			this._canvas.sortingOrder = this._parentPanel.sortingOrder;
			this._canvas.worldCamera = UICamera.mainCamera;
			this._canvas.planeDistance = UICamera.mainCamera.farClipPlane - 1f;
			gameObject.AddComponent<GraphicRaycaster>();
		}

		private void LateUpdate()
		{
			if (!this._checkAlphaEveryFrame)
			{
				return;
			}
			bool flag = this._parentPanel.alpha > 0.001f;
			if (flag != this._canvas.enabled)
			{
				this._canvas.enabled = flag;
			}
		}

		[SerializeField]
		private bool _checkAlphaEveryFrame;

		private Canvas _canvas;

		private UIPanel _parentPanel;
	}
}
