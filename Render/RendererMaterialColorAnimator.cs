using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class RendererMaterialColorAnimator : MonoBehaviour
	{
		public void Awake()
		{
			this._renderer = base.GetComponent<Renderer>();
			this.materialPropertyBlock = new MaterialPropertyBlock();
			if (this._renderer == null)
			{
				return;
			}
			this.materialPropertyBlock.SetColor(this.PropertyName, this.Color);
		}

		public void Update()
		{
			if (this._renderer == null || this.oldColor == this.Color)
			{
				return;
			}
			this.materialPropertyBlock.SetColor(this.PropertyName, this.Color);
			this._renderer.SetPropertyBlock(this.materialPropertyBlock);
		}

		public string PropertyName;

		private MaterialPropertyBlock materialPropertyBlock;

		public Color Color;

		private Color oldColor = default(Color);

		private Renderer _renderer;

		public bool WorkWithInstance = true;
	}
}
