using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class BasicExtensions
	{
		public static string GetFullPath(this GameObject thisGameObject)
		{
			return thisGameObject.transform.GetFullPath();
		}

		public static string GetFullPath(this Transform transform)
		{
			string text = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				text = transform.name + "/" + text;
			}
			return text;
		}
	}
}
