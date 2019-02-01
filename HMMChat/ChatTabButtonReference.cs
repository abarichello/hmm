using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.HMMChat
{
	public class ChatTabButtonReference : MonoBehaviour
	{
		public void SetFocus()
		{
			this.Focus.gameObject.SetActive(true);
		}

		public void SetNormal()
		{
			this.Focus.gameObject.SetActive(false);
		}

		public void SetOffLine()
		{
			this.Base.color = this.Offlinecolor;
			this.Focus.color = this.Offlinecolor;
			this.HasUnreadMessageFX.color = this.Offlinecolor;
			if (!this.IsGroup)
			{
				this.CloseButtonListener.GetComponent<UI2DSprite>().color = this.Offlinecolor;
			}
		}

		public void SetOnLine()
		{
			this.SetKind(this.IsGroup);
		}

		public void SetHasUnreadMessage(bool state)
		{
			this.HasUnreadMessageFX.gameObject.SetActive(state);
		}

		public void SetKind(bool isGroupTab)
		{
			this.IsGroup = isGroupTab;
			if (isGroupTab)
			{
				this.Base.color = this.GroupColor;
				this.Focus.color = this.GroupColor;
				this.HasUnreadMessageFX.color = this.GroupColor;
			}
			else
			{
				this.Base.color = this.NormalColor;
				this.Focus.color = this.NormalColor;
				this.HasUnreadMessageFX.color = this.NormalColor;
				this.CloseButtonListener.GetComponent<UI2DSprite>().color = this.NormalColor;
			}
		}

		public UI2DSprite Base;

		public UI2DSprite Focus;

		public UI2DSprite HasUnreadMessageFX;

		public UILabel TabNameLabel;

		public GUIEventListener Listener;

		public GUIEventListener CloseButtonListener;

		public bool IsGroup;

		public Color NormalColor;

		public Color GroupColor;

		public Color Offlinecolor;
	}
}
