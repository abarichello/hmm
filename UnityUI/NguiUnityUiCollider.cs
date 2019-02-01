using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	[RequireComponent(typeof(UIWidget))]
	public class NguiUnityUiCollider : MonoBehaviour
	{
		protected void Awake()
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
			Canvas canvas = gameObject.AddComponent<Canvas>();
			canvas.overrideSorting = true;
			canvas.sortingOrder = this._sortingOrder;
			canvas.worldCamera = UICamera.mainCamera;
			canvas.planeDistance = UICamera.mainCamera.farClipPlane - 1f;
			gameObject.AddComponent<GraphicRaycaster>();
		}

		[SerializeField]
		private int _sortingOrder = 1000;
	}
}
