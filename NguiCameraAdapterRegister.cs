using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class NguiCameraAdapterRegister : MonoBehaviour
	{
		protected void Awake()
		{
			this._nguiCameraAdapter.RegisterCamera(UICamera.mainCamera);
		}

		[SerializeField]
		private NguiCameraAdapter _nguiCameraAdapter;
	}
}
