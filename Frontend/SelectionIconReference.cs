using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class SelectionIconReference : GameHubBehaviour
	{
		private void OnHover(bool isOver)
		{
			if (base.enabled)
			{
				if (this.selected)
				{
					return;
				}
				if (isOver)
				{
					this.SetMouseOver(true);
				}
				else
				{
					this.SetMouseOver(false);
				}
			}
		}

		public void SetMouseOver(bool isover)
		{
			if (isover && !this.selected)
			{
				this.BorderFeedBack.color = new Color32(200, 180, 0, byte.MaxValue);
			}
			else
			{
				this.BorderFeedBack.color = Color.black;
			}
		}

		public void SetSelected()
		{
			this.BorderFeedBack.color = new Color32(0, 150, 0, byte.MaxValue);
			this.selected = true;
		}

		public void SetUnSelected()
		{
			this.BorderFeedBack.color = Color.black;
			this.selected = false;
		}

		public UISprite PlayerIcon;

		public UISprite BorderFeedBack;

		public int IconId;

		private bool selected;
	}
}
