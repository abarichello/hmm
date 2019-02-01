using System;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[RequireComponent(typeof(Canvas))]
	public class UnityUiSceneSetup : MonoBehaviour
	{
		protected void Awake()
		{
			bool flag = SceneManager.GetActiveScene().name == base.gameObject.scene.name;
			base.GetComponent<Canvas>().enabled = (this._enableCanvasIfSceneIsActive && flag);
			if (flag && this._createEventSystemIfSceneIsActive && UnityEngine.Object.FindObjectsOfType(typeof(EventSystem)).Length == 0)
			{
				GameObject gameObject = new GameObject("FakeEventSystem - Dont Save In Scene");
				gameObject.AddComponent<EventSystem>();
				gameObject.AddComponent<StandaloneInputModule>();
				gameObject.transform.SetSiblingIndex(0);
			}
			if (flag && this._createCanvasBackgroundIfSceneIsActive && UnityEngine.Object.FindObjectsOfType(typeof(Camera)).Length == 0)
			{
				GameObject gameObject2 = new GameObject("FakeCanvasBackground - Dont Save In Scene");
				Canvas canvas = gameObject2.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.sortingOrder = 0;
				RawImage rawImage = gameObject2.AddComponent<RawImage>();
				rawImage.color = Color.black;
				gameObject2.transform.SetSiblingIndex(1);
			}
			if (flag)
			{
				GameObject gameObject3 = new GameObject("FakeLoadingManager - Dont Save In Scene");
				gameObject3.AddComponent<LoadingManager>();
				gameObject3.transform.SetSiblingIndex(2);
				BitLogger.Initialize(CppFileAppender.GetMainLogger());
			}
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		[SerializeField]
		private bool _enableCanvasIfSceneIsActive = true;

		[SerializeField]
		private bool _createEventSystemIfSceneIsActive = true;

		[SerializeField]
		private bool _createCanvasBackgroundIfSceneIsActive = true;
	}
}
