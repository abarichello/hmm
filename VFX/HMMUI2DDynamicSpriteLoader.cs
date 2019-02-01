using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(HMMUI2DDynamicSprite))]
	public class HMMUI2DDynamicSpriteLoader : MonoBehaviour
	{
		public void OnEnable()
		{
			HMMUI2DDynamicSprite component = base.GetComponent<HMMUI2DDynamicSprite>();
			if (component.GetAssetName() != this.SpriteName)
			{
				component.SpriteName = this.SpriteName;
			}
		}

		public void OnDisable()
		{
			if (this.UnloadOnDisable)
			{
				base.GetComponent<HMMUI2DDynamicSprite>().ClearSprite();
			}
		}

		public string SpriteName;

		public bool UnloadOnDisable = true;
	}
}
