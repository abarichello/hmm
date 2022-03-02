using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMUI2DPolygonSprite : UIWidget
	{
		public override Material material
		{
			get
			{
				if (this.mMaterial == null)
				{
					this.mMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
				}
				this.mMaterial.SetTexture("_MainTex", this.mTexture);
				return this.mMaterial;
			}
		}

		protected override void OnInit()
		{
			base.OnInit();
			this.UpdatePivot();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (this.Interpolate)
			{
				this.mChanged = true;
			}
		}

		public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
		{
			base.OnFill(verts, uvs, cols);
			int i = 0;
			Vector2 vector = default(Vector2);
			int num = this.Vertices.Length;
			while (i < num)
			{
				Vector2 vector2 = this.Vertices[i];
				if (this.Interpolate)
				{
					this._interpolatedVertices[i] = Vector3.Lerp(this._interpolatedVertices[i], this.Vertices[i], (!Application.isPlaying) ? 1f : Time.unscaledDeltaTime);
					vector2 = this._interpolatedVertices[i];
				}
				vector..ctor(this._pivotPositionModifierX + this.localSize.x * vector2.x, this._pivotPositionModifierY + this.localSize.y * vector2.y);
				verts.Add(vector);
				i++;
			}
			if (num % 4 != 0)
			{
				int num2 = num + (4 - num % 4);
				while (i < num2)
				{
					verts.Add(vector);
					i++;
				}
			}
			if (this.Interpolate)
			{
				bool interpolate = false;
				for (int j = 0; j < this.Vertices.Length; j++)
				{
					if (this._interpolatedVertices[j] != this.Vertices[j])
					{
						interpolate = true;
						break;
					}
				}
				this.Interpolate = interpolate;
			}
			int k = 0;
			Vector2 item = default(Vector2);
			int num3 = this.Uvs.Length;
			while (k < num3)
			{
				item = this.Uvs[k];
				uvs.Add(item);
				k++;
			}
			if (num3 % 4 != 0)
			{
				int num4 = num3 + (4 - num3 % 4);
				while (k < num4)
				{
					uvs.Add(item);
					k++;
				}
			}
			Color mColor = this.mColor;
			mColor.a = this.finalAlpha;
			for (int l = 0; l < verts.size; l++)
			{
				cols.Add(mColor);
			}
			for (int m = 0; m < this.VerticeSprites.Length; m++)
			{
				UI2DSprite ui2DSprite = this.VerticeSprites[m];
				if (ui2DSprite != null)
				{
					ui2DSprite.transform.localPosition = verts[m];
				}
			}
			if (this.onPostFill != null)
			{
				this.onPostFill(this, verts.size, verts, uvs, cols);
			}
		}

		public void InterpolateVertices(float[] verticeValues, bool minToValue)
		{
			this.Interpolate = true;
			if (verticeValues == null || verticeValues.Length != this.Vertices.Length)
			{
				Debug.LogError(string.Format("HMMUI2DPolygonSprite.InterpolateVertices: Wrong values length. Vert:[{0}], Values:[{1}]", this.Vertices.Length, (verticeValues == null) ? -1 : verticeValues.Length));
				return;
			}
			for (int i = 0; i < verticeValues.Length; i++)
			{
				this._interpolatedVertices[i] = ((!minToValue) ? this.MaxPoints[i] : this.MinPoints[i]);
				this.Vertices[i] = Vector2.Lerp(this.MinPoints[i], this.MaxPoints[i], verticeValues[i]);
			}
		}

		public void UpdatePivot()
		{
			float num = this.localSize.x * 0.5f;
			float num2 = this.localSize.y * 0.5f;
			switch (this.mPivot)
			{
			case UIWidget.Pivot.TopLeft:
				this._pivotPositionModifierX = 0f;
				this._pivotPositionModifierY = -this.localSize.y;
				break;
			case UIWidget.Pivot.Top:
				this._pivotPositionModifierX = -num;
				this._pivotPositionModifierY = -this.localSize.y;
				break;
			case UIWidget.Pivot.TopRight:
				this._pivotPositionModifierX = -this.localSize.x;
				this._pivotPositionModifierY = -this.localSize.y;
				break;
			case UIWidget.Pivot.Left:
				this._pivotPositionModifierX = 0f;
				this._pivotPositionModifierY = -num2;
				break;
			case UIWidget.Pivot.Center:
				this._pivotPositionModifierX = -num;
				this._pivotPositionModifierY = -num2;
				break;
			case UIWidget.Pivot.Right:
				this._pivotPositionModifierX = -this.localSize.x;
				this._pivotPositionModifierY = -num2;
				break;
			case UIWidget.Pivot.BottomLeft:
				this._pivotPositionModifierX = 0f;
				this._pivotPositionModifierY = 0f;
				break;
			case UIWidget.Pivot.Bottom:
				this._pivotPositionModifierX = -num;
				this._pivotPositionModifierY = 0f;
				break;
			case UIWidget.Pivot.BottomRight:
				this._pivotPositionModifierX = -this.localSize.x;
				this._pivotPositionModifierY = 0f;
				break;
			}
		}

		[HideInInspector]
		[SerializeField]
		protected Texture mTexture;

		[HideInInspector]
		[SerializeField]
		[Range(3f, 12f)]
		protected int mNumVertices = 3;

		private Material mMaterial;

		public Vector2[] Vertices = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0.5f, 1f)
		};

		public Vector2[] Uvs = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0.5f, 1f)
		};

		public UI2DSprite[] VerticeSprites = new UI2DSprite[3];

		public Vector2[] MaxPoints = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0.5f, 1f)
		};

		public Vector2[] MinPoints = new Vector2[]
		{
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, 0.5f)
		};

		public bool Interpolate;

		private readonly Vector2[] _interpolatedVertices = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0.5f, 1f)
		};

		private float _pivotPositionModifierX;

		private float _pivotPositionModifierY;
	}
}
