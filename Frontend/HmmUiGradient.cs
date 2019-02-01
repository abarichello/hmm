using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/Effects/Gradient")]
	public class HmmUiGradient : BaseMeshEffect
	{
		public void SetAlpha(float topAlpha, float bottomAlpha)
		{
			this._topColor.a = (byte)(topAlpha * 255f);
			this._bottomColor.a = (byte)(bottomAlpha * 255f);
			base.graphic.SetVerticesDirty();
		}

		public void SetColors(Color topColor, Color bottomColor)
		{
			this._topColor = topColor;
			this._bottomColor = bottomColor;
			base.graphic.SetVerticesDirty();
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			int num = vh.currentVertCount;
			if (!this.IsActive() || num == 0)
			{
				return;
			}
			UIVertex vertex = default(UIVertex);
			vh.PopulateUIVertex(ref vertex, 0);
			float y = vertex.position.y;
			vh.PopulateUIVertex(ref vertex, --num);
			float y2 = vertex.position.y;
			float num2 = y - y2;
			for (;;)
			{
				vertex.color = Color32.Lerp(this._bottomColor, this._topColor, (vertex.position.y - y2) / num2);
				if (this._useRootAlpha)
				{
					vertex.color.a = (byte)(base.graphic.color.a * 255f);
				}
				vh.SetUIVertex(vertex, num);
				if (--num < 0)
				{
					break;
				}
				vh.PopulateUIVertex(ref vertex, num);
			}
		}

		[SerializeField]
		private Color32 _topColor = Color.white;

		[SerializeField]
		private Color32 _bottomColor = Color.black;

		[SerializeField]
		private bool _useRootAlpha;
	}
}
