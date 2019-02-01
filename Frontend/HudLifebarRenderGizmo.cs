using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarRenderGizmo : MonoBehaviour
	{
		public void Start()
		{
		}

		public void OnPostRender()
		{
			GL.LoadPixelMatrix(0f, (float)Screen.width, 0f, (float)Screen.height);
			GL.Begin(1);
			this.GizmoLineMat.SetPass(0);
			GL.Color(Color.red);
			Vector2 vector = new Vector2(1f, 1f);
			Vector2 vector2 = new Vector2((float)Screen.width, 1f);
			Vector2 vector3 = new Vector2((float)Screen.width, (float)Screen.height);
			Vector2 vector4 = new Vector2(1f, (float)Screen.height);
			GL.Vertex3(vector.x, vector.y, 0f);
			GL.Vertex3(vector2.x, vector2.y, 0f);
			GL.Vertex3(vector2.x, vector2.y, 0f);
			GL.Vertex3(vector3.x, vector3.y, 0f);
			GL.Vertex3(vector3.x, vector3.y, 0f);
			GL.Vertex3(vector4.x, vector4.y, 0f);
			GL.Vertex3(vector4.x, vector4.y, 0f);
			GL.Vertex3(vector.x, vector.y, 0f);
			Vector2 vector5 = this.WorldToScreenPointPosition;
			vector = new Vector2(vector5.x + 1f, vector5.y + 1f);
			vector2 = new Vector2(vector5.x + 2f, vector5.y + 1f);
			vector3 = new Vector2(vector5.x + 2f, vector5.y + 2f);
			vector4 = new Vector2(vector5.x + 1f, vector5.y + 2f);
			GL.Vertex3(vector.x, vector.y, 0f);
			GL.Vertex3(vector2.x, vector2.y, 0f);
			GL.Vertex3(vector2.x, vector2.y, 0f);
			GL.Vertex3(vector3.x, vector3.y, 0f);
			GL.Vertex3(vector3.x, vector3.y, 0f);
			GL.Vertex3(vector4.x, vector4.y, 0f);
			GL.Vertex3(vector4.x, vector4.y, 0f);
			GL.Vertex3(vector.x, vector.y, 0f);
			GL.Color(Color.green);
			vector5 = this.TransformedPosition;
			vector = new Vector2(vector5.x + 1f, vector5.y + 1f);
			vector2 = new Vector2(vector5.x + 2f, vector5.y + 1f);
			vector3 = new Vector2(vector5.x + 2f, vector5.y + 2f);
			vector4 = new Vector2(vector5.x + 1f, vector5.y + 2f);
			GL.Vertex3(vector.x, vector.y, 0f);
			GL.Vertex3(vector2.x, vector2.y, 0f);
			GL.Vertex3(vector2.x, vector2.y, 0f);
			GL.Vertex3(vector3.x, vector3.y, 0f);
			GL.Vertex3(vector3.x, vector3.y, 0f);
			GL.Vertex3(vector4.x, vector4.y, 0f);
			GL.Vertex3(vector4.x, vector4.y, 0f);
			GL.Vertex3(vector.x, vector.y, 0f);
			GL.End();
		}

		public Material GizmoLineMat;

		public Vector2 WorldToScreenPointPosition;

		public Vector2 TransformedPosition;
	}
}
