using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting
{
	public static class RectTransformExtensions
	{
		public static Vector2 LocalToScreenPosition(this RectTransform transform)
		{
			Vector3[] array = new Vector3[4];
			transform.GetWorldCorners(array);
			Rect rect;
			rect..ctor(array[0].x, array[0].y, array[2].x - array[0].x, array[2].y - array[0].y);
			return rect.center;
		}

		public static Vector2 ScreenToLocalPosition(this RectTransform transform, Vector2 screenPosition)
		{
			Vector2 result;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, screenPosition, null, ref result);
			return result;
		}
	}
}
