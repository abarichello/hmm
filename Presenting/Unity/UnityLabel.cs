using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityLabel : ILabel, IValueHolder
	{
		public UnityLabel(Text text)
		{
			this._text = text;
		}

		public bool IsActive
		{
			get
			{
				return !(this._text == null) && !(this._text.gameObject == null) && this._text.gameObject.activeSelf;
			}
			set
			{
				if (this._text == null || this._text.gameObject == null)
				{
					return;
				}
				this._text.gameObject.SetActive(value);
			}
		}

		public string Text
		{
			get
			{
				return this._text.text;
			}
			set
			{
				this._text.text = value;
			}
		}

		public Color Color
		{
			get
			{
				return this._text.color.ToHmmColor();
			}
			set
			{
				this._text.color = value.ToUnityColor();
			}
		}

		public float Size
		{
			get
			{
				return (float)this._text.fontSize;
			}
			set
			{
				this._text.fontSize = (int)value;
			}
		}

		public float Width
		{
			get
			{
				return this._text.rectTransform.rect.width;
			}
			set
			{
				this._text.rectTransform.SetSizeWithCurrentAnchors(0, value);
			}
		}

		public float Height
		{
			get
			{
				return this._text.rectTransform.rect.height;
			}
			set
			{
				this._text.rectTransform.SetSizeWithCurrentAnchors(1, value);
			}
		}

		public bool HasValue
		{
			get
			{
				return this._text != null;
			}
		}

		[SerializeField]
		private Text _text;
	}
}
