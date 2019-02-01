using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[ExecuteInEditMode]
	public class HMMItemDropSurface : GameHubBehaviour
	{
		private void Update()
		{
			if (HMMDragItem.CurrentDrag == null)
			{
				return;
			}
			if (UICamera.hoveredObject == base.gameObject)
			{
				if (!this.isHover)
				{
					this.OnMouseHover(true);
					this.isHover = true;
				}
			}
			else if (this.isHover)
			{
				this.OnMouseHover(false);
				this.isHover = false;
			}
		}

		public void OnMouseHover(bool hover)
		{
			this.eventTarget.SendMessage((!hover) ? this.hoverOutEvent : this.hoverInEvent, HMMDragItem.CurrentDrag);
		}

		public void OnDrop(GameObject obj)
		{
			if (HMMDragItem.CurrentDrag == null)
			{
				return;
			}
			this.eventTarget.SendMessage(this.dropEvent, HMMDragItem.CurrentDrag);
		}

		public bool ValidateDrop(GameObject obj)
		{
			return true;
		}

		public Transform eventTarget;

		public string dropEvent;

		public string hoverInEvent;

		public string hoverOutEvent;

		private bool isHover;
	}
}
