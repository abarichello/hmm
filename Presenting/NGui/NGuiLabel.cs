using System;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiLabel : ILabel, IValueHolder
	{
		public bool IsActive
		{
			get
			{
				return this._label.gameObject.activeSelf;
			}
			set
			{
				this._label.gameObject.SetActive(value);
			}
		}

		public string Text
		{
			get
			{
				return this._label.text;
			}
			set
			{
				this._label.text = value;
			}
		}

		public Color Color
		{
			get
			{
				return this._label.color.ToHmmColor();
			}
			set
			{
				this._label.color = value.ToUnityColor();
			}
		}

		public float Size
		{
			get
			{
				return (float)this._label.fontSize;
			}
			set
			{
				this._label.fontSize = (int)value;
			}
		}

		public bool HasValue
		{
			get
			{
				return this._label != null;
			}
		}

		[SerializeField]
		private UILabel _label;
	}
}
