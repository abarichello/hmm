using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMLifebarSpriteContainer : MonoBehaviour
	{
		public int InternalWidth
		{
			get
			{
				return this._internalWidth;
			}
			set
			{
				this._internalWidth = value;
			}
		}

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

		public HMMLifebarSprite[] sprites
		{
			get
			{
				return this._sprites;
			}
		}

		public void Init(int pIndex, HMMLifebarPanel pHMMLifebarPanel)
		{
			this.Id = pIndex;
			this._sprites = base.GetComponentsInChildren<HMMLifebarSprite>();
			this.parentPanel = base.GetComponentInParent<HMMLifebarPanel>();
		}

		private void Update()
		{
			if (this.recalculateBoundsEveryFrame)
			{
				this.RecalculateContainerBounds();
			}
		}

		public void RecalculateContainerBounds()
		{
			int maxValue = int.MaxValue;
			int maxValue2 = int.MaxValue;
			int minValue = int.MinValue;
			int minValue2 = int.MinValue;
			if (this.parentPanel == null)
			{
				this.parentPanel = base.GetComponentInParent<HMMLifebarPanel>();
			}
			Vector3 vertice = Vector3.zero;
			for (int i = 0; i < this.sprites.Length; i++)
			{
				HMMLifebarSprite hmmlifebarSprite = this.sprites[i];
				for (int j = 0; j < hmmlifebarSprite.spriteMesh.vertices.Length; j++)
				{
					vertice = hmmlifebarSprite.spriteMesh.vertices[j];
					vertice.x += base.transform.localPosition.x;
					vertice.y += base.transform.localPosition.y;
					if (this.parentPanel.GetSpriteStyleForName(hmmlifebarSprite.Spritestyle) == HMMLifebarPanel.SpriteStyle.Bordered)
					{
						if (j > 3)
						{
							vertice.x += (float)(this.InternalWidth + hmmlifebarSprite.width);
						}
					}
					else if (j > 1)
					{
						vertice.x += (float)(this.InternalWidth + hmmlifebarSprite.width);
					}
					this.CalculateMeshBoundsValues(vertice, ref maxValue, ref maxValue2, ref minValue, ref minValue2);
				}
			}
			this.meshBounds = new Vector4((float)maxValue, (float)maxValue2, (float)minValue, (float)minValue2);
		}

		private void SetSpritesVisibility(bool pVisible)
		{
			for (int i = 0; i < this.sprites.Length; i++)
			{
				HMMLifebarSprite hmmlifebarSprite = this.sprites[i];
				hmmlifebarSprite.visible = pVisible;
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

		public int Id;

		public Vector4 meshBounds;

		public bool recalculateBoundsEveryFrame;

		private int _internalWidth;

		[SerializeField]
		private bool _visible;

		[SerializeField]
		private HMMLifebarSprite[] _sprites;

		private HMMLifebarPanel parentPanel;
	}
}
