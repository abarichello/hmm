using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityCanvas : ICanvas
	{
		public UnityCanvas(Canvas canvas)
		{
			this._canvas = canvas;
		}

		public bool IsActive
		{
			get
			{
				return this._canvas.gameObject.activeSelf;
			}
			set
			{
				this._canvas.gameObject.SetActive(value);
			}
		}

		public void Enable()
		{
			this._canvas.enabled = true;
		}

		public void Disable()
		{
			this._canvas.enabled = false;
		}

		public Camera Camera
		{
			get
			{
				return this._canvas.worldCamera;
			}
		}

		public int SortOrder
		{
			get
			{
				return this._canvas.sortingOrder;
			}
		}

		public Transform Transform
		{
			get
			{
				return this._canvas.transform;
			}
		}

		[SerializeField]
		private Canvas _canvas;
	}
}
