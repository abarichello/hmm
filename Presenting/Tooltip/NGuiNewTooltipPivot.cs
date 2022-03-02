using System;
using Hoplon.Math;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Tooltip
{
	public class NGuiNewTooltipPivot : MonoBehaviour, ITooltipPivot
	{
		public Vector2 Position
		{
			get
			{
				Camera mainCamera = UICamera.mainCamera;
				if (mainCamera == null)
				{
					return Vector2.Zero;
				}
				Vector3 vector = mainCamera.WorldToScreenPoint(base.gameObject.transform.position);
				Vector2 result = default(Vector2);
				result.x = vector.x;
				result.y = vector.y;
				return result;
			}
		}
	}
}
