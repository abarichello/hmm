using System;
using JetBrains.Annotations;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	public class UnityTexture : ITexture
	{
		public UnityTexture(Texture texture)
		{
			this._texture = texture;
		}

		public Texture GetTexture()
		{
			return this._texture;
		}

		[SerializeField]
		[UsedImplicitly]
		private Texture _texture;
	}
}
