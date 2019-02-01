using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiScrollRect")]
	public class HmmUiScrollRect : ScrollRect
	{
		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (!this._ignoreDrag)
			{
				base.OnBeginDrag(eventData);
			}
		}

		public override void OnDrag(PointerEventData eventData)
		{
			if (!this._ignoreDrag)
			{
				base.OnDrag(eventData);
			}
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			if (!this._ignoreDrag)
			{
				base.OnEndDrag(eventData);
			}
		}

		public override void OnScroll(PointerEventData eventData)
		{
			if (!this._ignoreScroll)
			{
				base.OnScroll(eventData);
			}
		}

		[SerializeField]
		private bool _ignoreDrag;

		[SerializeField]
		private bool _ignoreScroll;
	}
}
