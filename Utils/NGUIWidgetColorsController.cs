using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUIWidgetColorsController : MonoBehaviour
	{
		public Color color
		{
			get
			{
				return this._color;
			}
			set
			{
				this._color = value;
				this.UpdateWidgetsColor(value);
			}
		}

		private void Start()
		{
			this._widgets = base.GetComponentsInChildren<UIWidget>();
		}

		private void UpdateWidgetsColor(Color32 pColor)
		{
			if (this._widgets == null)
			{
				this._widgets = base.GetComponentsInChildren<UIWidget>();
			}
			for (int i = 0; i < this._widgets.Length; i++)
			{
				UIWidget uiwidget = this._widgets[i];
				uiwidget.color = pColor;
			}
		}

		[SerializeField]
		private Color _color;

		private UIWidget[] _widgets;
	}
}
