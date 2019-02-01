using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GUIEventListener : GameHubBehaviour
	{
		private void Start()
		{
		}

		public void OnClick()
		{
			GUIEventListener.ClickKind kind = this.Kind;
			if (kind != GUIEventListener.ClickKind.Left)
			{
				if (kind != GUIEventListener.ClickKind.Both)
				{
					if (kind == GUIEventListener.ClickKind.Right)
					{
						if (UICamera.currentTouchID == -2)
						{
							this.OnEvent();
						}
					}
				}
				else if (UICamera.currentTouchID == -1 || UICamera.currentTouchID == -2)
				{
					this.OnEvent();
				}
			}
			else if (UICamera.currentTouchID == -1)
			{
				this.OnEvent();
			}
		}

		public void OnRightClick()
		{
			GUIEventListener.ClickKind kind = this.Kind;
			if (kind != GUIEventListener.ClickKind.Left)
			{
				if (kind == GUIEventListener.ClickKind.Both || kind == GUIEventListener.ClickKind.Right)
				{
					this.OnEvent();
				}
			}
		}

		private void OnHover(bool isHovered)
		{
			switch (this.Hover)
			{
			case GUIEventListener.HoverKind.Over:
				if (isHovered)
				{
					this.OnEvent();
				}
				break;
			case GUIEventListener.HoverKind.Out:
				if (!isHovered)
				{
					this.OnEvent();
				}
				break;
			case GUIEventListener.HoverKind.Any:
				this.OnEvent();
				break;
			}
		}

		private void OnActivate(bool isActive)
		{
			if (!isActive)
			{
				return;
			}
			this.OnEvent();
		}

		public void OnSelect(bool selected)
		{
			GUIEventListener.SelectionKind select = this.Select;
			if (select != GUIEventListener.SelectionKind.Select)
			{
				if (select == GUIEventListener.SelectionKind.DeSelect)
				{
					if (!selected)
					{
						this.OnEvent();
					}
				}
			}
			else if (selected)
			{
				this.OnEvent();
			}
		}

		public void OnKey(KeyCode key)
		{
			if (this.Key != key)
			{
				return;
			}
			this.OnEvent();
		}

		public void OnEvent()
		{
			if (string.IsNullOrEmpty(this.MethodName) || this.EventListener == null || !base.enabled)
			{
				return;
			}
			GUIEventListener.ParameterKind theParameterKind = this.TheParameterKind;
			switch (theParameterKind + 1)
			{
			case GUIEventListener.ParameterKind.String:
				this.EventListener.SendMessage(this.MethodName);
				break;
			case GUIEventListener.ParameterKind.Integer:
				if (this.StringParameter.Equals(string.Empty))
				{
					this.EventListener.SendMessage(this.MethodName);
				}
				else
				{
					this.EventListener.SendMessage(this.MethodName, this.StringParameter);
				}
				break;
			case GUIEventListener.ParameterKind.ObjectListener:
				this.EventListener.SendMessage(this.MethodName, this.IntParameter);
				break;
			case GUIEventListener.ParameterKind.TargetGameObject:
				this.EventListener.SendMessage(this.MethodName, base.gameObject);
				break;
			case (GUIEventListener.ParameterKind)4:
				this.EventListener.SendMessage(this.MethodName, this.TargetGameObject);
				break;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GUIEventListener));

		public GameObject EventListener;

		[KeyCodeDrawer]
		public KeyCode Key;

		public GUIEventListener.SelectionKind Select;

		public GUIEventListener.ClickKind Kind;

		public GUIEventListener.HoverKind Hover;

		public GUIEventListener.ParameterKind TheParameterKind;

		public int IntParameter;

		public string StringParameter = string.Empty;

		public string MethodName = string.Empty;

		public GameObject TargetGameObject;

		public enum ClickKind
		{
			None,
			Both,
			Left,
			Right
		}

		public enum HoverKind
		{
			None,
			Over,
			Out,
			Any
		}

		public enum SelectionKind
		{
			None,
			Select,
			DeSelect
		}

		public enum ParameterKind
		{
			None = -1,
			String,
			Integer,
			ObjectListener,
			TargetGameObject
		}
	}
}
