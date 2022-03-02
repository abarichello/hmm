using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.News.Presenting
{
	public class NewsItemView : MonoBehaviour, INewsItemView
	{
		public IButton Button
		{
			get
			{
				return this._button;
			}
		}

		public ITextureImage TextureImage
		{
			get
			{
				return this._rawImage;
			}
		}

		public IActivatable SpinnerActivatable
		{
			get
			{
				return this._spinnerActivatable;
			}
		}

		public IActivatable ErrorActivatable
		{
			get
			{
				return this._errorActivatable;
			}
		}

		[SerializeField]
		private UnityButton _button;

		[SerializeField]
		private UnityRawImage _rawImage;

		[SerializeField]
		private GameObjectActivatable _spinnerActivatable;

		[SerializeField]
		private GameObjectActivatable _errorActivatable;
	}
}
