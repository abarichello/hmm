using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
	public class UnityUiCanvasFix : MonoBehaviour
	{
		public void Awake()
		{
			Canvas component = base.GetComponent<Canvas>();
			Camera camera;
			if (this.NguiCameraAdapter.HasCamera(out camera))
			{
				component.renderMode = 1;
				component.worldCamera = camera;
				component.planeDistance = camera.farClipPlane - 1f;
			}
			else
			{
				component.renderMode = 0;
			}
			if (Application.isPlaying)
			{
				CanvasScaler component2 = base.GetComponent<CanvasScaler>();
				GraphicRaycaster component3 = base.GetComponent<GraphicRaycaster>();
				Object.Destroy(this);
			}
		}

		[SerializeField]
		protected NguiCameraAdapter NguiCameraAdapter;
	}
}
