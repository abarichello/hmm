using System;
using Holoville.HOTween;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(UIWidget))]
	public class NGUIWidgetWipeFX : MonoBehaviour
	{
		public float amount
		{
			get
			{
				return this._amount;
			}
			set
			{
				if (value != this._amount)
				{
					this._amount = value;
					this.UpdateFX();
				}
				this._amount = value;
			}
		}

		public UIPanel fxPanel { get; private set; }

		public UIWidget fxClone { get; private set; }

		public void Init()
		{
			if (this._uiWidget == null)
			{
				this._uiWidget = base.GetComponent<UIWidget>();
			}
			this.CreateFXPanel();
		}

		private void OnEnable()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.Init();
		}

		private void OnDisable()
		{
			if (this.fxPanel == null)
			{
				return;
			}
			this.DestroyFXPanel();
		}

		private void CreateFXPanel()
		{
			if (this._uiWidget == null || this._uiWidget.panel == null || this._uiWidget.transform.parent == null)
			{
				return;
			}
			if (this.fxPanel == null)
			{
				this.fxPanel = this._uiWidget.transform.parent.gameObject.AddChild<UIPanel>();
				this.fxPanel.name = string.Format("{0} (FXPanel)", this._uiWidget.name);
				this.fxPanel.transform.localPosition = this._uiWidget.transform.localPosition;
				this.fxPanel.clipping = UIDrawCall.Clipping.SoftClip;
				this.fxPanel.clipSoftness = new Vector2(this.horizontalSmooth, this.verticalSmooth);
				this.fxPanel.baseClipRegion = new Vector4(0f, 0f, this._horizontalSize, this._verticalSize);
				this.fxPanel.depth = this._uiWidget.depth + 1;
				this.fxClone = UnityEngine.Object.Instantiate<GameObject>(this._uiWidget.gameObject).GetComponent<UIWidget>();
				NGUIWidgetWipeFX component = this.fxClone.GetComponent<NGUIWidgetWipeFX>();
				UnityEngine.Object.Destroy(component);
				this.fxClone.transform.parent = this.fxPanel.transform;
				this.fxClone.transform.localScale = this._uiWidget.transform.localScale;
				this.fxClone.transform.localPosition = Vector3.zero;
				this.fxClone.depth = this._uiWidget.depth + 1;
			}
		}

		public void DestroyFXPanel()
		{
			if (this.fxPanel == null)
			{
				return;
			}
			HOTween.Kill(this.fxPanel);
			UnityEngine.Object.Destroy(this.fxPanel.gameObject);
		}

		private void UpdateFX()
		{
			if (this.fxPanel == null)
			{
				return;
			}
			float num = (float)this.fxClone.width;
			float num2 = (float)this.fxClone.height;
			float num3 = this.amount;
			float num4 = this.amount;
			float num5 = this.horizontalSmooth;
			float num6 = this.verticalSmooth;
			float x = 0f;
			float y = 0f;
			NGUIWidgetWipeFX.FxOrientation fxOrientation = this.fxOrientation;
			if (fxOrientation != NGUIWidgetWipeFX.FxOrientation.Horizontal)
			{
				if (fxOrientation == NGUIWidgetWipeFX.FxOrientation.Vertical)
				{
					num3 = 1f;
				}
			}
			else
			{
				num4 = 1f;
			}
			NGUIWidgetWipeFX.FxHorizontalDirection fxHorizontalDirection = this.fxHorizontalDirection;
			if (fxHorizontalDirection != NGUIWidgetWipeFX.FxHorizontalDirection.Left2Right)
			{
				if (fxHorizontalDirection != NGUIWidgetWipeFX.FxHorizontalDirection.Right2Left)
				{
					if (fxHorizontalDirection == NGUIWidgetWipeFX.FxHorizontalDirection.FromCenter)
					{
						num5 *= 2f;
					}
				}
				else
				{
					x = (float)this.fxClone.width / 2f;
					num5 *= 2f;
					num *= 2f;
				}
			}
			else
			{
				x = (float)(-(float)this.fxClone.width) / 2f;
				num5 *= 2f;
				num *= 2f;
			}
			NGUIWidgetWipeFX.FxVerticalDirection fxVerticalDirection = this.fxVerticalDirection;
			if (fxVerticalDirection != NGUIWidgetWipeFX.FxVerticalDirection.Bottom2Top)
			{
				if (fxVerticalDirection != NGUIWidgetWipeFX.FxVerticalDirection.Top2Bottom)
				{
					if (fxVerticalDirection == NGUIWidgetWipeFX.FxVerticalDirection.FromCenter)
					{
						num6 *= 2f;
					}
				}
				else
				{
					y = (float)this.fxClone.height / 2f;
					num6 *= 2f;
					num2 *= 2f;
				}
			}
			else
			{
				y = (float)(-(float)this.fxClone.height) / 2f;
				num6 *= 2f;
				num2 *= 2f;
			}
			this._horizontalSize = num3 * (num + num5);
			this._verticalSize = num4 * (num2 + num6);
			this.fxPanel.clipSoftness = new Vector2(this.horizontalSmooth, this.verticalSmooth);
			this.fxPanel.baseClipRegion = new Vector4(x, y, this._horizontalSize, this._verticalSize);
		}

		public float horizontalSmooth = 250f;

		public float verticalSmooth = 250f;

		[Range(0.0001f, 1f)]
		[SerializeField]
		private float _amount;

		public bool debug;

		public NGUIWidgetWipeFX.FxOrientation fxOrientation;

		public NGUIWidgetWipeFX.FxHorizontalDirection fxHorizontalDirection;

		public NGUIWidgetWipeFX.FxVerticalDirection fxVerticalDirection;

		private UIWidget _uiWidget;

		private float _horizontalSize;

		private float _verticalSize;

		public enum FxHorizontalDirection
		{
			Left2Right,
			Right2Left,
			FromCenter
		}

		public enum FxVerticalDirection
		{
			Top2Bottom,
			Bottom2Top,
			FromCenter
		}

		public enum FxOrientation
		{
			Horizontal,
			Vertical,
			Both
		}
	}
}
