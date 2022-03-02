using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Video
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RawImage))]
	public sealed class VideoPreviewUnityUI : BaseVideoPreview
	{
		protected override void SetupTexture(Texture texture)
		{
			if (texture)
			{
				this._image.enabled = true;
				this._image.texture = texture;
			}
			else
			{
				this._image.enabled = false;
			}
		}

		private void OnValidate()
		{
			this._image = base.GetComponent<RawImage>();
		}

		[SerializeField]
		[HideInInspector]
		private RawImage _image;
	}
}
