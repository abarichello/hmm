using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class MaterialLocalization : GameHubBehaviour
	{
		private void Start()
		{
			this._meshRenderer = base.GetComponent<MeshRenderer>();
			this.ApplyText();
		}

		private void ApplyText()
		{
			int num = -1;
			int num2 = -1;
			while (++num < this.Textures.Length && num2 == -1)
			{
				if (this.Textures[num].Language == Language.CurrentLanguage)
				{
					num2 = num;
				}
			}
			if (num2 == -1)
			{
				return;
			}
			this._meshRenderer.material.mainTexture = this.Textures[num2].Texture;
		}

		private MeshRenderer _meshRenderer;

		public MaterialLocalization.MaterialTexturePerLanguage[] Textures;

		[Serializable]
		public class MaterialTexturePerLanguage
		{
			public Texture Texture;

			public LanguageCode Language;
		}
	}
}
