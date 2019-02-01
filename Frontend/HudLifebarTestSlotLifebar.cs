using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarTestSlotLifebar : MonoBehaviour
	{
		protected void Start()
		{
			this._rectTransform = base.GetComponent<RectTransform>();
		}

		protected void LateUpdate()
		{
			this._rectTransform.sizeDelta = new Vector2((float)this.Width, (float)this.Height);
		}

		public int Width;

		public int Height;

		private RectTransform _rectTransform;
	}
}
