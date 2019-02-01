using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class MapProjectedFakeHeight : GameHubBehaviour
	{
		public static MapProjectedFakeHeight.PointData GetPointData(Vector3 position)
		{
			if (MapProjectedFakeHeight.Instance == null || MapProjectedFakeHeight.Instance.terrainInfo == null)
			{
				return default(MapProjectedFakeHeight.PointData);
			}
			Color pixelBilinear = MapProjectedFakeHeight.Instance.terrainInfo.GetPixelBilinear(position.x / MapProjectedFakeHeight.Instance.Size.x * 0.5f + 0.5f, position.z / MapProjectedFakeHeight.Instance.Size.y * 0.5f + 0.5f);
			return new MapProjectedFakeHeight.PointData
			{
				cryspness = pixelBilinear.g,
				groundType = ((pixelBilinear.b <= 0f) ? GroundType.Asphalt : GroundType.Dirt),
				height = pixelBilinear.r
			};
		}

		private void Start()
		{
			MapProjectedFakeHeight.Instance = this;
		}

		private void OnDrawGizmosSelected()
		{
			if (!this.gizmoMaterial || this.gizmoMaterial.mainTexture != this.terrainInfo)
			{
				this.gizmoMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
				this.gizmoMaterial.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
				this.gizmoMaterial.mainTexture = this.terrainInfo;
			}
			else
			{
				this.gizmoMaterial.SetPass(0);
			}
			float y = base.transform.position.y;
			bool fog = RenderSettings.fog;
			RenderSettings.fog = false;
			GL.Begin(7);
			GL.Color(Color.white);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(-this.Size.x, y, -this.Size.y);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(-this.Size.x, y, this.Size.y);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(this.Size.x, y, this.Size.y);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(this.Size.x, y, -this.Size.y);
			GL.End();
			RenderSettings.fog = fog;
			GL.InvalidateState();
		}

		public Vector2 Size;

		public Texture2D terrainInfo;

		private Material gizmoMaterial;

		private static MapProjectedFakeHeight Instance;

		public struct PointData
		{
			public GroundType groundType;

			public float height;

			public float cryspness;
		}
	}
}
