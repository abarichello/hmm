using System;
using UnityEngine;

namespace HeavyMetalMachines.UnityUI
{
	[RequireComponent(typeof(RectTransform))]
	public class UnityUiNguiCollider : MonoBehaviour
	{
		protected void Awake()
		{
			RectTransform component = base.GetComponent<RectTransform>();
			GameObject gameObject = new GameObject("NGUI UIPanel");
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.layer = base.gameObject.layer;
			gameObject.transform.SetSiblingIndex(0);
			UIPanel uipanel = gameObject.AddComponent<UIPanel>();
			uipanel.sortingOrder = this._sortingOrder;
			uipanel.depth = this._depth;
			GameObject gameObject2 = new GameObject("UIWidget");
			gameObject2.layer = base.gameObject.layer;
			gameObject2.AddComponent<UIWidget>();
			BoxCollider boxCollider = gameObject2.AddComponent<BoxCollider>();
			boxCollider.size = new Vector2(component.rect.width, component.rect.height);
			gameObject2.transform.SetParent(gameObject.transform);
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.transform.localPosition = Vector3.zero;
		}

		[SerializeField]
		private int _sortingOrder = 1000;

		[SerializeField]
		private int _depth = 999999;
	}
}
