using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class HMMPanelMeshController : MonoBehaviour
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
			this.RecalculateBounds();
			this.mesh.vertices = this.vertices;
			this.mesh.triangles = this.triangles;
			this.mesh.colors32 = this.colors;
			this.mesh.uv = this.uv;
			this.mesh.uv2 = this.uv1;
			this.mesh.MarkDynamic();
			this.mesh.RecalculateNormals();
			this.mesh.RecalculateBounds();
		}

		public void UpdateSprite(HMMSprite sprite)
		{
			if (sprite.index >= this.vertices.Length / 4)
			{
				return;
			}
			int num = sprite.index * 4;
			Color32[] array = sprite.spriteMesh.GetColors();
			for (int i = 0; i < 4; i++)
			{
				if (this.CheckSpriteMeshForSprite(sprite))
				{
					int num2 = num + i;
					this.vertices[num2] = sprite.spriteMesh.vertices[i];
					this.colors[num2] = array[i];
				}
			}
			Vector3 position = sprite.position;
			Vector3 vector = this.vertices[num];
			Vector3 vector2 = this.vertices[1 + num];
			Vector3 vector3 = this.vertices[2 + num];
			Vector3 vector4 = this.vertices[3 + num];
			float num3 = vector4.x - vector.x;
			float num4 = vector4.y - vector.y;
			if (!sprite.visible)
			{
				vector2 = vector4;
				vector3 = vector;
			}
			else
			{
				vector = position;
				vector2.x = position.x;
				vector2.y = position.y + num4;
				vector2.z = position.z;
				vector3.x = position.x + num3;
				vector3.y = position.y;
				vector3.z = position.z;
				vector4.x = position.x + num3;
				vector4.y = position.y + num4;
				vector4.z = position.z;
			}
			this.vertices[num] = vector;
			this.vertices[1 + num] = vector2;
			this.vertices[2 + num] = vector3;
			this.vertices[3 + num] = vector4;
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
				this.RecalculateBounds();
			}
		}

		public void RecalculateBounds()
		{
			if (this._panel == null)
			{
				return;
			}
			int maxValue = int.MaxValue;
			int maxValue2 = int.MaxValue;
			int minValue = int.MinValue;
			int minValue2 = int.MinValue;
			for (int i = 0; i < this.vertices.Length; i++)
			{
				Vector3 vertice = this.vertices[i];
				this.CalculateMeshBoundsValues(vertice, ref maxValue, ref maxValue2, ref minValue, ref minValue2);
			}
			this.meshBounds = new Vector4((float)maxValue, (float)maxValue2, (float)minValue, (float)minValue2);
			if (this._panel.spriteContainers == null || this._panel.spriteContainers.Length == 0)
			{
				return;
			}
			for (int j = 0; j < this._panel.spriteContainers.Length; j++)
			{
				HMMSpriteContainer hmmspriteContainer = this._panel.spriteContainers[j];
				hmmspriteContainer.RecalculateBounds();
			}
		}

		private void ValidatePanel()
		{
			if (this._panel == null)
			{
				this._panel = base.GetComponent<HMMPanel>();
			}
		}

		private bool CheckSpriteMeshForSprite(HMMSprite sprite)
		{
			return sprite.spriteMesh != null && sprite.spriteMesh.vertices != null && sprite.spriteMesh.vertices.Length > 0;
		}

		private void CreateMeshesForSprites()
		{
			List<int> list = new List<int>();
			List<Vector3> list2 = new List<Vector3>();
			List<Vector2> list3 = new List<Vector2>();
			List<Vector2> list4 = new List<Vector2>();
			List<Color32> list5 = new List<Color32>();
			for (int i = 0; i < this._panel.sprites.Length; i++)
			{
				HMMSprite hmmsprite = this._panel.sprites[i];
				HMMPanel.SpriteData spriteForName = this._panel.GetSpriteForName(hmmsprite.spriteName);
				if (spriteForName == null)
				{
					string text = this._panel.GetSpriteNames()[0];
					Debug.LogError(string.Format("Sprite '{0}' not found on atlas '{1}'. Replacing with '{2}'.", base.name, this._panel.dataFile.name, text), hmmsprite.gameObject);
					spriteForName = this._panel.GetSpriteForName(text);
				}
				HMMPanelMeshController.SpriteMesh spriteMesh = this.CreateSprite(i, spriteForName, hmmsprite);
				hmmsprite.spriteMesh = spriteMesh;
				list2.AddRange(spriteMesh.vertices);
				list.AddRange(spriteMesh.triangles);
				list3.AddRange(spriteMesh.uv);
				list4.AddRange(spriteMesh.uv1);
				list5.AddRange(spriteMesh.GetColors());
			}
			this.vertices = list2.ToArray();
			this.triangles = list.ToArray();
			this.uv = list3.ToArray();
			this.uv1 = list4.ToArray();
			this.colors = list5.ToArray();
		}

		private HMMPanelMeshController.SpriteMesh CreateSprite(int index, HMMPanel.SpriteData spriteData, HMMSprite sprite)
		{
			int x = spriteData.x;
			int y = spriteData.y;
			int num = 0;
			int num2 = 0;
			if (sprite.visible)
			{
				num = ((this._panel.GetSpriteStyleForName(sprite.Spritestyle) != HMMPanel.SpriteStyle.Tiled) ? spriteData.width : sprite.width);
				num2 = spriteData.height;
			}
			float num3 = (float)x / (float)this._panel.atlasWidth;
			float atlasYOffset = (float)(this._panel.atlasHeight - (y + num2)) / (float)this._panel.atlasHeight;
			float atlasSpriteWidth = num3 + (float)num / (float)this._panel.atlasWidth;
			float atlasSpriteHeight = (float)(this._panel.atlasHeight - y) / (float)this._panel.atlasHeight;
			int width = (!sprite.customSize && this._panel.GetSpriteStyleForName(sprite.Spritestyle) != HMMPanel.SpriteStyle.Tiled) ? spriteData.width : sprite.width;
			int height = (!sprite.customSize) ? spriteData.height : sprite.height;
			sprite.width = width;
			sprite.height = height;
			float atlasTileWidth = (this._panel.GetSpriteStyleForName(sprite.Spritestyle) != HMMPanel.SpriteStyle.Tiled) ? 0f : ((float)spriteData.width / (float)this._panel.atlasWidth);
			return new HMMPanelMeshController.SpriteMesh(index, sprite.position, width, height, num3, atlasYOffset, atlasSpriteWidth, atlasSpriteHeight, atlasTileWidth);
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

		private HMMPanel _panel;

		[Serializable]
		public class SpriteMesh
		{
			public SpriteMesh(int index, Vector3 position, int width, int height, float atlasXOffset, float atlasYOffset, float atlasSpriteWidth, float atlasSpriteHeight, float atlasTileWidth)
			{
				this.vertices = new Vector3[]
				{
					position,
					new Vector3(position.x, (float)height + position.y, position.z),
					new Vector3(position.x + (float)width, position.y, position.z),
					new Vector3(position.x + (float)width, position.y + (float)height, position.z)
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
					new Vector2(atlasXOffset, atlasTileWidth),
					new Vector2(atlasXOffset, atlasTileWidth),
					new Vector2(atlasXOffset, atlasTileWidth),
					new Vector2(atlasXOffset, atlasTileWidth)
				};
				this.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				int num = index * 4;
				this.triangles = new int[]
				{
					num,
					num + 1,
					num + 2,
					num + 3,
					num + 2,
					num + 1
				};
			}

			public Color32[] GetColors()
			{
				if (this._colors == null)
				{
					this._colors = new Color32[4];
					for (int i = 0; i < 4; i++)
					{
						this._colors[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
					}
				}
				return this._colors;
			}

			public void SetColor(Color pValue)
			{
				this.color = pValue;
				this._colors = this.GetColors();
				for (int i = 0; i < 4; i++)
				{
					this._colors[i] = pValue;
				}
			}

			public const int verticesAmount = 4;

			public int[] triangles;

			public Vector3[] vertices;

			private Color32[] _colors;

			public Color32 color;

			public Vector2[] uv;

			public Vector2[] uv1;
		}
	}
}
