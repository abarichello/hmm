using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting
{
	public class ImageSetter : MonoBehaviour
	{
		public void Set(Sprite sprite)
		{
			this._image.sprite = sprite;
		}

		[SerializeField]
		private Image _image;
	}
}
