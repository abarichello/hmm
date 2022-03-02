using System;
using UnityEngine;

namespace HeavyMetalMachines.UnityUI
{
	[RequireComponent(typeof(RectTransform))]
	public class UnityUiNguiCollider : MonoBehaviour
	{
		protected void Start()
		{
			RectTransform component = base.GetComponent<RectTransform>();
			GameObject gameObject = new GameObject("NGUI UIPanel");
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.layer = base.gameObject.layer;
			gameObject.transform.SetSiblingIndex(0);
			UIPanel uipanel = gameObject.AddComponent<UIPanel>();
			this._canvas = base.GetComponentInParent<Canvas>();
			uipanel.sortingOrder = this._canvas.sortingOrder;
			uipanel.depth = 0;
			GameObject gameObject2 = new GameObject("UIWidget");
			gameObject2.layer = base.gameObject.layer;
			gameObject2.AddComponent<UIWidget>();
			this._boxCollider = gameObject2.AddComponent<BoxCollider>();
			this._boxCollider.size = new Vector2(component.rect.width, component.rect.height);
			gameObject2.transform.SetParent(gameObject.transform);
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.transform.localPosition = Vector3.zero;
		}

		private void LateUpdate()
		{
			if (this._boxCollider.enabled != this._canvas.enabled)
			{
				this._boxCollider.enabled = this._canvas.enabled;
			}
		}

		private Canvas _canvas;

		private BoxCollider _boxCollider;
	}
}
