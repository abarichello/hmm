using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(UIPanel))]
	public class BaseGUIController : BaseMonoBehaviour
	{
		public UIPanel Panel
		{
			get
			{
				if (this._panel == null)
				{
					this._panel = base.GetComponent<UIPanel>();
				}
				return this._panel;
			}
		}

		protected virtual void Reset()
		{
			if (this._panel == null)
			{
				this._panel = base.GetComponent<UIPanel>();
			}
		}

		protected void SetPositionToCurrentContext(Transform targetTransform)
		{
			if (!Input.mousePresent)
			{
				return;
			}
			base.transform.position = UICamera.currentCamera.ScreenToWorldPoint(Input.mousePosition);
		}

		[Header("Check this if screen game object must be destroyed on screen resolution")]
		public bool DestroyOnScreenResolution = true;

		[Header("Base GUI Controller")]
		[SerializeField]
		private UIPanel _panel;
	}
}
