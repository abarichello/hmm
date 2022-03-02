using System;
using UnityEngine;

namespace HeavyMetalMachines.Infra.GameObjects
{
	public static class GameObjectExtensions
	{
		public static void ForeachComponentInChildren<TComponentType>(this GameObject gameObject, Action<TComponentType> action) where TComponentType : Component
		{
			TComponentType[] componentsInChildren = gameObject.GetComponentsInChildren<TComponentType>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				action(componentsInChildren[i]);
			}
		}

		public static void DestroySafe(this GameObject gameObject)
		{
			if (null == gameObject)
			{
				return;
			}
			Object.Destroy(gameObject);
		}

		public static void DestroySafe(this Component component)
		{
			if (null == component)
			{
				return;
			}
			Object.Destroy(component.gameObject);
		}
	}
}
