using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMSpriteContainer : MonoBehaviour
	{
		public bool visible
		{
			get
			{
				return this._visible;
			}
			set
			{
				bool visible = this._visible;
				this._visible = value;
				if (visible != this._visible)
				{
					this.SetSpritesVisibility(this._visible);
				}
			}
		}

		public HMMSprite[] sprites
		{
			get
			{
				return this._sprites;
			}
		}

		public void Init(int pIndex, HMMPanel pHmmPanel)
		{
			this._sprites = base.GetComponentsInChildren<HMMSprite>();
		}

		private void Update()
		{
			if (this.recalculateBoundsEveryFrame)
			{
				this.RecalculateBounds();
			}
		}

		public void RecalculateBounds()
		{
			int maxValue = int.MaxValue;
			int maxValue2 = int.MaxValue;
			int minValue = int.MinValue;
			int minValue2 = int.MinValue;
			for (int i = 0; i < this.sprites.Length; i++)
			{
				HMMSprite hmmsprite = this.sprites[i];
				if (hmmsprite.spriteMesh != null && hmmsprite.spriteMesh.vertices != null)
				{
					for (int j = 0; j < hmmsprite.spriteMesh.vertices.Length; j++)
					{
						Vector3 vertice = hmmsprite.spriteMesh.vertices[j];
						this.CalculateMeshBoundsValues(vertice, ref maxValue, ref maxValue2, ref minValue, ref minValue2);
					}
				}
			}
			this.meshBounds = new Vector4((float)maxValue, (float)maxValue2, (float)minValue, (float)minValue2);
		}

		private void SetSpritesVisibility(bool pVisible)
		{
			for (int i = 0; i < this.sprites.Length; i++)
			{
				HMMSprite hmmsprite = this.sprites[i];
				hmmsprite.visible = pVisible;
			}
		}

		private void CalculateMeshBoundsValues(Vector3 vertice, ref int minX, ref int minY, ref int maxX, ref int maxY)
		{
			if (vertice.x < (float)minX)
			{
				minX = (int)vertice.x;
			}
			if (vertice.y < (float)minY)
			{
				minY = (int)vertice.y;
			}
			if (vertice.x > (float)maxX)
			{
				maxX = (int)vertice.x;
			}
			if (vertice.y > (float)maxY)
			{
				maxY = (int)vertice.y;
			}
		}

		public Vector4 meshBounds;

		public bool recalculateBoundsEveryFrame;

		[SerializeField]
		private bool _visible;

		[SerializeField]
		private HMMSprite[] _sprites;
	}
}
