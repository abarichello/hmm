using System;
using JetBrains.Annotations;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnitySprite : ISprite
	{
		public UnitySprite(Sprite sprite)
		{
			this._sprite = sprite;
		}

		public void SetSprite(Sprite sprite)
		{
			this._sprite = sprite;
		}

		public Sprite GetSprite()
		{
			return this._sprite;
		}

		[SerializeField]
		[UsedImplicitly]
		private Sprite _sprite;
	}
}
