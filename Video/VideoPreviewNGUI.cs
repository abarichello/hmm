using System;
using UnityEngine;

namespace HeavyMetalMachines.Video
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(UITexture))]
	public sealed class VideoPreviewNGUI : BaseVideoPreview
	{
		protected override void SetupTexture(Texture texture)
		{
			this._uiTexture.mainTexture = texture;
			this._borderSprite.enabled = (texture != null);
		}

		protected override void InvokeOnVideoLoaded()
		{
			this._player.enabled = true;
			base.InvokeOnVideoLoaded();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			this._player.enabled = false;
		}

		private void OnValidate()
		{
			this._uiTexture = base.GetComponent<UITexture>();
		}

		[SerializeField]
		private UI2DSprite _borderSprite;

		[SerializeField]
		[HideInInspector]
		private UITexture _uiTexture;
	}
}
