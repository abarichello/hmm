using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class HMMLifebarPanelMeshController : MonoBehaviour
	{
		public MeshFilter meshFilter
		{
			get
			{
				if (this._meshFilter == null)
				{
					this._meshFilter = base.gameObject.GetComponent<MeshFilter>();
				}
				return this._meshFilter;
			}
		}

		public Mesh mesh
		{
			get
			{
				Mesh result;
				if ((result = this.meshFilter.sharedMesh) == null)
				{
					Mesh mesh = new Mesh();
					this.meshFilter.sharedMesh = mesh;
					result = mesh;
				}
				return result;
			}
		}

		private void Awake()
		{
			this.ValidatePanel();
			this._spritesPropertyId = Shader.PropertyToID("_sprites");
		}

		public void CreateMeshes()
		{
			this.ValidatePanel();
			if (this._panel.sprites == null || this._panel.sprites.Length == 0)
			{
				return;
			}
			this.mesh.Clear();
			this.CreateMeshesForSprites();
			this.RecalculateMeshBounds();
			this.mesh.vertices = this.vertices;
			this.mesh.triangles = this.triangles;
			this.mesh.colors32 = this.colors;
			this.mesh.uv = this.uv;
			this.mesh.uv2 = this.uv1;
			this.mesh.MarkDynamic();
			this.mesh.RecalculateNormals();
			this.mesh.RecalculateBounds();
			if (!Application.isPlaying)
			{
				this.UpdateShaderMatrices();
			}
		}

		private void UpdateShaderMatrices()
		{
			Material sharedMaterial = base.GetComponent<MeshRenderer>().sharedMaterial;
			Matrix4x4 zero = Matrix4x4.zero;
			for (int i = 0; i < 4; i++)
			{
				HMMLifebarSpriteContainer hmmlifebarSpriteContainer = this._panel.spriteContainers[i];
				float num = (float)((int)Math.Floor((double)hmmlifebarSpriteContainer.transform.localPosition.x));
				float num2 = (float)((int)Math.Floor((double)hmmlifebarSpriteContainer.transform.localPosition.y));
				int num3 = (int)Math.Floor((double)(25f + (float)(i * 25)));
				hmmlifebarSpriteContainer.InternalWidth = (int)Math.Floor((double)(54f + (float)(i * 9)));
				int internalWidth = hmmlifebarSpriteContainer.InternalWidth;
				int num4 = num3 << 16;
				num4 += internalWidth;
				zero.SetRow(i, new Vector4(num, num2, (float)num4, 0f));
			}
			sharedMaterial.SetMatrix(this._spritesPropertyId, zero);
		}

		public void UpdateSprite(HMMLifebarSprite sprite)
		{
			this.CreateMeshes();
		}

		public void UpdateVertices()
		{
			if (this.vertices == null || this.vertices.Length == 0)
			{
				return;
			}
			this.mesh.vertices = this.vertices;
			this.mesh.colors32 = this.colors;
			if (this.recalculateBounds)
			{
				this.RecalculateMeshBounds();
			}
		}

		public void RecalculateMeshBounds()
		{
			if (this._panel == null)
			{
				return;
			}
			int maxValue = int.MaxValue;
			int maxValue2 = int.MaxValue;
			int minValue = int.MinValue;
			int minValue2 = int.MinValue;
			for (int i = 0; i < this._panel.spriteContainers.Length; i++)
			{
				HMMLifebarSpriteContainer hmmlifebarSpriteContainer = this._panel.spriteContainers[i];
				Vector3 vertice = Vector3.zero;
				for (int j = 0; j < hmmlifebarSpriteContainer.sprites.Length; j++)
				{
					HMMLifebarSprite hmmlifebarSprite = hmmlifebarSpriteContainer.sprites[j];
					for (int k = 0; k < hmmlifebarSprite.spriteMesh.vertices.Length; k++)
					{
						vertice = hmmlifebarSprite.spriteMesh.vertices[k];
						vertice.x += hmmlifebarSpriteContainer.transform.localPosition.x;
						vertice.y += hmmlifebarSpriteContainer.transform.localPosition.y;
						if (this._panel.GetSpriteStyleForName(hmmlifebarSprite.Spritestyle) == HMMLifebarPanel.SpriteStyle.Bordered)
						{
							if (k > 3)
							{
								vertice.x += (float)(hmmlifebarSpriteContainer.InternalWidth + hmmlifebarSprite.width);
							}
						}
						else if (k > 1)
						{
							vertice.x += (float)(hmmlifebarSpriteContainer.InternalWidth + hmmlifebarSprite.width);
						}
						this.CalculateMeshBoundsValues(vertice, ref maxValue, ref maxValue2, ref minValue, ref minValue2);
					}
				}
			}
			this.meshBounds = new Vector4((float)maxValue, (float)maxValue2, (float)minValue, (float)minValue2);
			if (this._panel.spriteContainers == null || this._panel.spriteContainers.Length == 0)
			{
				return;
			}
			for (int l = 0; l < this._panel.spriteContainers.Length; l++)
			{
				HMMLifebarSpriteContainer hmmlifebarSpriteContainer2 = this._panel.spriteContainers[l];
				hmmlifebarSpriteContainer2.RecalculateContainerBounds();
			}
		}

		private void ValidatePanel()
		{
			if (this._panel == null)
			{
				this._panel = base.GetComponent<HMMLifebarPanel>();
			}
		}

		private bool CheckSpriteMeshForSprite(HMMLifebarSprite sprite)
		{
			return sprite.spriteMesh != null && sprite.spriteMesh.vertices != null && sprite.spriteMesh.vertices.Length > 0;
		}

		private HMMLifebarSprite GetLowerDepthOnSpriteList(List<HMMLifebarSprite> spriteList)
		{
			HMMLifebarSprite hmmlifebarSprite = spriteList[0];
			for (int i = 0; i < spriteList.Count; i++)
			{
				if (spriteList[i].depth < hmmlifebarSprite.depth)
				{
					hmmlifebarSprite = spriteList[i];
				}
			}
			spriteList.Remove(hmmlifebarSprite);
			return hmmlifebarSprite;
		}

		private void CreateMeshesForSprites()
		{
			List<int> list = new List<int>();
			List<Vector3> list2 = new List<Vector3>();
			List<Vector2> list3 = new List<Vector2>();
			List<Vector2> list4 = new List<Vector2>();
			List<Color32> list5 = new List<Color32>();
			int num = 0;
			for (int i = 0; i < this._panel.spriteContainers.Length; i++)
			{
				HMMLifebarSpriteContainer hmmlifebarSpriteContainer = this._panel.spriteContainers[i];
				List<HMMLifebarSprite> list6 = new List<HMMLifebarSprite>(hmmlifebarSpriteContainer.sprites);
				while (list6.Count > 0)
				{
					HMMLifebarSprite lowerDepthOnSpriteList = this.GetLowerDepthOnSpriteList(list6);
					HMMLifebarPanel.SpriteData spriteForName = this._panel.GetSpriteForName(lowerDepthOnSpriteList.spriteName);
					if (spriteForName == null)
					{
						string text = this._panel.GetSpriteNames()[0];
						Debug.LogError(string.Format("Sprite '{0}' not found on atlas '{1}'. Replacing with '{2}'.", base.name, this._panel.dataFile.name, text), lowerDepthOnSpriteList.gameObject);
						spriteForName = this._panel.GetSpriteForName(text);
					}
					int containerid = (!(lowerDepthOnSpriteList.parentContainer != null)) ? num : lowerDepthOnSpriteList.parentContainer.Id;
					HMMLifebarPanelMeshController.SpriteMesh spriteMesh;
					switch (this._panel.GetSpriteStyleForName(lowerDepthOnSpriteList.Spritestyle))
					{
					case HMMLifebarPanel.SpriteStyle.Normal:
						spriteMesh = this.CreateSprite(num, containerid, spriteForName, lowerDepthOnSpriteList);
						break;
					case HMMLifebarPanel.SpriteStyle.Tiled:
						spriteMesh = this.CreateTiledSprite(num, containerid, spriteForName, lowerDepthOnSpriteList);
						break;
					case HMMLifebarPanel.SpriteStyle.Filled:
						spriteMesh = this.CreateSprite(num, containerid, spriteForName, lowerDepthOnSpriteList);
						break;
					case HMMLifebarPanel.SpriteStyle.Bordered:
						spriteMesh = this.CreateBorderedSprite(num, containerid, spriteForName, lowerDepthOnSpriteList);
						break;
					default:
						spriteMesh = this.CreateSprite(num, containerid, spriteForName, lowerDepthOnSpriteList);
						break;
					}
					lowerDepthOnSpriteList.spriteMesh = spriteMesh;
					int count = list2.Count;
					list2.AddRange(spriteMesh.vertices);
					for (int j = 0; j < spriteMesh.triangles.Length; j++)
					{
						int num2 = spriteMesh.triangles[j];
						list.Add(num2 + count);
					}
					list3.AddRange(spriteMesh.uv);
					list4.AddRange(spriteMesh.uv1);
					list5.AddRange(spriteMesh.GetColors());
					num++;
				}
			}
			this.vertices = list2.ToArray();
			this.triangles = list.ToArray();
			this.uv = list3.ToArray();
			this.uv1 = list4.ToArray();
			this.colors = list5.ToArray();
		}

		private HMMLifebarPanelMeshController.SpriteMesh CreateSprite(int index, int containerid, HMMLifebarPanel.SpriteData spriteData, HMMLifebarSprite sprite)
		{
			int x = spriteData.x;
			int y = spriteData.y;
			int num = 0;
			int num2 = 0;
			if (sprite.visible)
			{
				num = spriteData.width;
				num2 = spriteData.height;
			}
			int width = sprite.width;
			int height = sprite.height;
			float num3 = (float)x / (float)this._panel.atlasWidth;
			float atlasYOffset = (float)(this._panel.atlasHeight - (y + num2)) / (float)this._panel.atlasHeight;
			float atlasSpriteHeight = (float)(this._panel.atlasHeight - y) / (float)this._panel.atlasHeight;
			float num4 = num3;
			float uv2X = num4 + (float)num / (float)this._panel.atlasWidth;
			return new HMMLifebarPanelMeshController.SpriteMesh(index, containerid, sprite.position, width, height, 0f, atlasYOffset, 1f, atlasSpriteHeight, num4, uv2X, this._panel.GetSpriteStyleForName(sprite.Spritestyle), spriteData.width);
		}

		private HMMLifebarPanelMeshController.SpriteMesh CreateTiledSprite(int index, int containerid, HMMLifebarPanel.SpriteData spriteData, HMMLifebarSprite sprite)
		{
			int x = spriteData.x;
			int y = spriteData.y;
			int num = 0;
			int num2 = 0;
			if (sprite.visible)
			{
				num = spriteData.width;
				num2 = spriteData.height;
			}
			int width = sprite.width;
			int height = sprite.height;
			float num3 = (float)x / (float)this._panel.atlasWidth;
			float atlasYOffset = (float)(this._panel.atlasHeight - (y + num2)) / (float)this._panel.atlasHeight;
			float atlasSpriteWidth = (float)width / (float)num;
			float atlasSpriteHeight = (float)(this._panel.atlasHeight - y) / (float)this._panel.atlasHeight;
			float num4 = num3;
			float uv2X = num4 + (float)num / (float)this._panel.atlasWidth;
			return new HMMLifebarPanelMeshController.SpriteMesh(index, containerid, sprite.position, width, height, 0f, atlasYOffset, atlasSpriteWidth, atlasSpriteHeight, num4, uv2X, this._panel.GetSpriteStyleForName(sprite.Spritestyle), spriteData.width);
		}

		private HMMLifebarPanelMeshController.SpriteMesh CreateBorderedSprite(int index, int containerid, HMMLifebarPanel.SpriteData spriteData, HMMLifebarSprite sprite)
		{
			int x = spriteData.x;
			int y = spriteData.y;
			int num = 0;
			int num2 = 0;
			if (sprite.visible)
			{
				num = spriteData.width;
				num2 = spriteData.height;
			}
			int width = sprite.width;
			int height = sprite.height;
			float num3 = (float)x / (float)this._panel.atlasWidth;
			float atlasYOffset = (float)(this._panel.atlasHeight - (y + num2)) / (float)this._panel.atlasHeight;
			float atlasSpriteHeight = (float)(this._panel.atlasHeight - y) / (float)this._panel.atlasHeight;
			float num4 = num3;
			float uv2X = num4 + (float)num / (float)this._panel.atlasWidth;
			float atlasleftbordersize = (float)sprite.leftBorder / (float)num;
			float atlasrightbordersize = (float)sprite.rightBorder / (float)num;
			return new HMMLifebarPanelMeshController.SpriteMesh(index, containerid, sprite.position, width, height, sprite.leftBorder, sprite.rightBorder, atlasleftbordersize, atlasrightbordersize, 0f, atlasYOffset, 1f, atlasSpriteHeight, num4, uv2X, this._panel.GetSpriteStyleForName(sprite.Spritestyle), spriteData.width);
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

		public int[] triangles;

		public Vector3[] vertices;

		public Vector2[] uv;

		public Vector2[] uv1;

		public Color32[] colors;

		public Vector4 meshBounds;

		public bool recalculateBounds;

		private MeshFilter _meshFilter;

		private HMMLifebarPanel _panel;

		private int _spritesPropertyId = -1;

		[Serializable]
		public class SpriteMesh
		{
			public SpriteMesh(int index, int containerid, Vector3 position, int width, int height, float atlasXOffset, float atlasYOffset, float atlasSpriteWidth, float atlasSpriteHeight, float uv1X, float uv2X, HMMLifebarPanel.SpriteStyle style, int spritedatapixelsize)
			{
				this.verticesAmount = 4;
				this.id = containerid;
				this._spriteDatawidth = spritedatapixelsize;
				int num = (int)Math.Floor((double)position.x);
				int num2 = (int)Math.Floor((double)position.y);
				int num3 = (int)Math.Floor((double)position.z);
				this.vertices = new Vector3[]
				{
					new Vector3((float)num, (float)num2, (float)num3),
					new Vector3((float)num, (float)(height + num2), (float)num3),
					new Vector3((float)(num + width), (float)num2, (float)num3),
					new Vector3((float)(num + width), (float)(num2 + height), (float)num3)
				};
				this.uv = new Vector2[]
				{
					new Vector2(atlasXOffset, atlasYOffset),
					new Vector2(atlasXOffset, atlasSpriteHeight),
					new Vector2(atlasSpriteWidth, atlasYOffset),
					new Vector2(atlasSpriteWidth, atlasSpriteHeight)
				};
				this.uv1 = new Vector2[]
				{
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X)
				};
				this.Style = style;
				this._colors = this.GetColors();
				this.triangles = new int[]
				{
					0,
					1,
					2,
					3,
					2,
					1
				};
			}

			public SpriteMesh(int index, int containerid, Vector3 position, int width, int height, int leftbordersize, int rightbordersize, float atlasleftbordersize, float atlasrightbordersize, float atlasXOffset, float atlasYOffset, float atlasSpriteWidth, float atlasSpriteHeight, float uv1X, float uv2X, HMMLifebarPanel.SpriteStyle style, int spritedatapixelsize)
			{
				this.verticesAmount = 8;
				this.id = containerid;
				this._spriteDatawidth = spritedatapixelsize;
				int num = (int)Math.Floor((double)position.x);
				int num2 = (int)Math.Floor((double)position.y);
				int num3 = (int)Math.Floor((double)position.z);
				this.vertices = new Vector3[]
				{
					new Vector3((float)num, (float)num2, (float)num3),
					new Vector3((float)num, (float)(height + num2), (float)num3),
					new Vector3((float)(num + leftbordersize), (float)num2, (float)num3),
					new Vector3((float)(num + leftbordersize), (float)(height + num2), (float)num3),
					new Vector3((float)(num + leftbordersize + width), (float)num2, (float)num3),
					new Vector3((float)(num + leftbordersize + width), (float)(height + num2), (float)num3),
					new Vector3((float)(num + leftbordersize + width + rightbordersize), (float)num2, (float)num3),
					new Vector3((float)(num + leftbordersize + width + rightbordersize), (float)(num2 + height), (float)num3)
				};
				this.uv = new Vector2[]
				{
					new Vector2(0f, atlasYOffset),
					new Vector2(0f, atlasSpriteHeight),
					new Vector2(atlasleftbordersize, atlasYOffset),
					new Vector2(atlasleftbordersize, atlasSpriteHeight),
					new Vector2(1f - atlasrightbordersize, atlasYOffset),
					new Vector2(1f - atlasrightbordersize, atlasSpriteHeight),
					new Vector2(1f, atlasYOffset),
					new Vector2(1f, atlasSpriteHeight)
				};
				this.uv1 = new Vector2[]
				{
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X),
					new Vector2(uv1X, uv2X)
				};
				this.Style = style;
				this._colors = this.GetColors();
				this.triangles = new int[]
				{
					0,
					1,
					2,
					3,
					2,
					1,
					2,
					3,
					4,
					5,
					4,
					3,
					4,
					5,
					6,
					7,
					6,
					5
				};
			}

			public Color32[] GetColors()
			{
				int num = 0;
				this._colors = new Color32[this.verticesAmount];
				for (int i = 0; i < this.verticesAmount; i++)
				{
					if (this.Style == HMMLifebarPanel.SpriteStyle.Filled)
					{
						this._colors[i] = new Color32(byte.MaxValue, (byte)this.id, byte.MaxValue, 0);
					}
					else if (this.Style == HMMLifebarPanel.SpriteStyle.Tiled)
					{
						this._colors[i] = new Color32(0, (byte)this.id, byte.MaxValue, (byte)this._spriteDatawidth);
					}
					else
					{
						this._colors[i] = new Color32(0, (byte)this.id, byte.MaxValue, 0);
					}
					int num2 = (this.Style != HMMLifebarPanel.SpriteStyle.Bordered) ? 1 : 3;
					if (i > num2)
					{
						this._colors[i].b = byte.MaxValue;
					}
					else
					{
						this._colors[i].b = 0;
					}
					num++;
				}
				return this._colors;
			}

			public void SetColor(Color pValue)
			{
				this.color = pValue;
				this._colors = this.GetColors();
				for (int i = 0; i < this.verticesAmount; i++)
				{
					this._colors[i] = pValue;
				}
			}

			public int verticesAmount = 4;

			private int id = -1;

			public int[] triangles;

			public Vector3[] vertices;

			private Color32[] _colors;

			public Color32 color;

			public Vector2[] uv;

			public Vector2[] uv1;

			private HMMLifebarPanel.SpriteStyle Style;

			private int _spriteDatawidth;
		}
	}
}
