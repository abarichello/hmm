using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class BaseHintContent
	{
		public BaseHintContent(string textContent, string newLineTextContent, float timeoutSeconds, bool useDefaultStyle = true, Sprite ui2DSprite = null, string ownerId = "SystemMessage")
		{
			this._textContent = textContent;
			this._newLinetextContent = newLineTextContent;
			this._timeoutSeconds = timeoutSeconds;
			this._Sprite = ui2DSprite;
			this._defaultStyle = useDefaultStyle;
			this._ownerId = ((!string.IsNullOrEmpty(ownerId)) ? ownerId : "SystemMessage");
		}

		public BaseHintContent(string textContent, float timeoutSeconds, bool useDefaultStyle = true, Sprite ui2DSprite = null, string ownerId = "SystemMessage")
		{
			this._textContent = textContent;
			this._newLinetextContent = string.Empty;
			this._timeoutSeconds = timeoutSeconds;
			this._Sprite = ui2DSprite;
			this._defaultStyle = useDefaultStyle;
			this._ownerId = ((!string.IsNullOrEmpty(ownerId)) ? ownerId : "SystemMessage");
		}

		public string OwnerId
		{
			get
			{
				return this._ownerId;
			}
		}

		public float TimeoutSeconds
		{
			get
			{
				return this._timeoutSeconds;
			}
		}

		public string TextContent
		{
			get
			{
				return this._textContent;
			}
		}

		public string NewLineTextContent
		{
			get
			{
				return this._newLinetextContent;
			}
		}

		public Sprite Sprite
		{
			get
			{
				return this._Sprite;
			}
		}

		public bool UseDefaultStyle
		{
			get
			{
				return this._defaultStyle;
			}
		}

		public float GetTimeoutSeconds()
		{
			return this._timeoutSeconds;
		}

		protected string _ownerId;

		protected float _timeoutSeconds;

		protected string _textContent;

		protected string _newLinetextContent;

		protected Sprite _Sprite;

		protected bool _defaultStyle;
	}
}
