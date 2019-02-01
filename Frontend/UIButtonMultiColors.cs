using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("NGUI/Interaction/ButtonMultiColors")]
	public class UIButtonMultiColors : UIWidgetContainer
	{
		public Color GetDefaultColor(UIButtonMultiColors.ButtonColor buttonColor)
		{
			this.Start();
			for (int i = 0; i < this.ButtonColors.Length; i++)
			{
				UIButtonMultiColors.ButtonColor buttonColor2 = this.ButtonColors[i];
				if (buttonColor2 == buttonColor)
				{
					return buttonColor2.mColor;
				}
			}
			Debug.LogWarning("ButtonColor Widget not found");
			return Color.white;
		}

		public void SetDefaultColor(UIButtonMultiColors.ButtonColor buttonColor, Color color)
		{
			this.Start();
			for (int i = 0; i < this.ButtonColors.Length; i++)
			{
				UIButtonMultiColors.ButtonColor buttonColor2 = this.ButtonColors[i];
				if (buttonColor2 == buttonColor)
				{
					buttonColor2.mColor = color;
					return;
				}
			}
		}

		private void Start()
		{
			if (!this.mStarted)
			{
				this.mStarted = true;
				this.Init();
			}
		}

		protected virtual void OnEnable()
		{
			if (this.mStarted)
			{
				for (int i = 0; i < this.ButtonColors.Length; i++)
				{
					UIButtonMultiColors.ButtonColor buttonColor = this.ButtonColors[i];
					this.OnHover(UICamera.IsHighlighted(buttonColor.tweenTarget.gameObject));
				}
			}
		}

		protected virtual void OnDisable()
		{
			for (int i = 0; i < this.ButtonColors.Length; i++)
			{
				UIButtonMultiColors.ButtonColor buttonColor = this.ButtonColors[i];
				if (this.mStarted && buttonColor.tweenTarget != null)
				{
					TweenColor component = buttonColor.tweenTarget.GetComponent<TweenColor>();
					if (component != null)
					{
						component.value = buttonColor.mColor;
						component.enabled = false;
					}
				}
			}
		}

		protected void Init()
		{
			for (int i = 0; i < this.ButtonColors.Length; i++)
			{
				UIButtonMultiColors.ButtonColor buttonColor = this.ButtonColors[i];
				if (buttonColor.tweenTarget == null)
				{
					buttonColor.tweenTarget = base.gameObject;
				}
				buttonColor.widget = buttonColor.tweenTarget.GetComponent<UIWidget>();
				if (buttonColor.widget != null)
				{
					buttonColor.mColor = buttonColor.widget.color;
				}
				else
				{
					Renderer component = buttonColor.tweenTarget.GetComponent<Renderer>();
					if (component != null)
					{
						buttonColor.mColor = component.material.color;
					}
					else
					{
						Light component2 = buttonColor.tweenTarget.GetComponent<Light>();
						if (component2 != null)
						{
							buttonColor.mColor = component2.color;
						}
						else
						{
							buttonColor.tweenTarget = null;
							if (Application.isPlaying)
							{
								Debug.LogWarning(NGUITools.GetHierarchy(base.gameObject) + " has nothing for UIButtonColor to color", this);
								base.enabled = false;
							}
						}
					}
				}
			}
			this.OnEnable();
		}

		public void OnHover(bool isOver)
		{
			if (base.enabled)
			{
				if (!this.mStarted)
				{
					this.Start();
				}
				for (int i = 0; i < this.ButtonColors.Length; i++)
				{
					UIButtonMultiColors.ButtonColor buttonColor = this.ButtonColors[i];
					TweenColor.Begin(buttonColor.tweenTarget, buttonColor.duration, (!isOver) ? buttonColor.mColor : buttonColor.hover);
				}
			}
		}

		protected virtual void OnPress(bool isPressed)
		{
			if (base.enabled)
			{
				if (!this.mStarted)
				{
					this.Start();
				}
				for (int i = 0; i < this.ButtonColors.Length; i++)
				{
					UIButtonMultiColors.ButtonColor buttonColor = this.ButtonColors[i];
					TweenColor.Begin(buttonColor.tweenTarget, buttonColor.duration, (!isPressed) ? buttonColor.mColor : buttonColor.pressed);
				}
			}
		}

		public bool isEnabled
		{
			get
			{
				Collider component = base.GetComponent<Collider>();
				return component && component.enabled;
			}
			set
			{
				Collider component = base.GetComponent<Collider>();
				if (!component)
				{
					return;
				}
				if (component.enabled != value)
				{
					component.enabled = value;
					this.UpdateColor(value, false);
				}
			}
		}

		public void UpdateColor(bool shouldBeEnabled, bool immediate)
		{
			for (int i = 0; i < this.ButtonColors.Length; i++)
			{
				UIButtonMultiColors.ButtonColor buttonColor = this.ButtonColors[i];
				if (buttonColor.tweenTarget)
				{
					if (!this.mStarted)
					{
						this.mStarted = true;
						this.Init();
					}
					Color color = (!shouldBeEnabled) ? buttonColor.disabledColor : this.GetDefaultColor(buttonColor);
					TweenColor tweenColor = TweenColor.Begin(buttonColor.tweenTarget, 0.15f, color);
					if (immediate)
					{
						tweenColor.value = color;
						tweenColor.enabled = false;
					}
				}
			}
		}

		public UIButtonMultiColors.ButtonColor[] ButtonColors;

		public bool mStarted;

		public bool mHighlighted;

		[Serializable]
		public class ButtonColor
		{
			public GameObject tweenTarget;

			public Color hover = new Color(0.6f, 1f, 0.2f, 1f);

			public Color pressed = Color.grey;

			public float duration = 0.2f;

			public Color disabledColor = Color.grey;

			public UIWidget widget;

			public Color mColor;
		}
	}
}
