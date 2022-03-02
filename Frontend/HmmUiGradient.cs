using System;
using HeavyMetalMachines.Frontend.UnityUI;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/Effects/Gradient")]
	public class HmmUiGradient : BaseMeshEffect, IUiGradient
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
			UIVertex uivertex = default(UIVertex);
			vh.PopulateUIVertex(ref uivertex, 0);
			float y = uivertex.position.y;
			vh.PopulateUIVertex(ref uivertex, --num);
			float y2 = uivertex.position.y;
			float num2 = y - y2;
			for (;;)
			{
				uivertex.color = Color32.Lerp(this._bottomColor, this._topColor, (uivertex.position.y - y2) / num2);
				if (this._useRootAlpha)
				{
					uivertex.color.a = (byte)(base.graphic.color.a * 255f);
				}
				vh.SetUIVertex(uivertex, num);
				if (--num < 0)
				{
					break;
				}
				vh.PopulateUIVertex(ref uivertex, num);
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
