using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	public class AIStaticScene : IAIStaticSceneElementCollection, IAIStaticSceneElementIterator
	{
		public AIStaticScene()
		{
			this._sceneElements = new Dictionary<int, List<IAIStaticSceneElement>>(4);
			this._allElements = new List<IAIStaticSceneElement>(128);
			this._comparison = new Comparison<IAIStaticSceneElement>(this.CompareElements);
		}

		public void CallOnAllElementsInBounds(Vector2 minBound, Vector2 maxBound, Action<IAIStaticSceneElement> action)
		{
			AIStaticScene.CallOnElementsInBounds(minBound, maxBound, action, this._allElements);
		}

		public void CallOnElementsInBounds(Vector2 minBound, Vector2 maxBound, IAIElementKind elementKind, Action<IAIStaticSceneElement> action)
		{
			List<IAIStaticSceneElement> elements;
			if (!this._sceneElements.TryGetValue(elementKind.ID, out elements))
			{
				return;
			}
			AIStaticScene.CallOnElementsInBounds(minBound, maxBound, action, elements);
		}

		private static void CallOnElementsInBounds(Vector2 minBound, Vector2 maxBound, Action<IAIStaticSceneElement> action, List<IAIStaticSceneElement> elements)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				IAIStaticSceneElement iaistaticSceneElement = elements[i];
				if (iaistaticSceneElement.BoundsMin.x > maxBound.x)
				{
					return;
				}
				if (iaistaticSceneElement.BoundsMax.x >= minBound.x && iaistaticSceneElement.BoundsMin.y <= maxBound.y && iaistaticSceneElement.BoundsMax.y >= minBound.y)
				{
					action(iaistaticSceneElement);
				}
			}
		}

		public void AddElement(IAIStaticSceneElement sceneElement)
		{
			List<IAIStaticSceneElement> list;
			if (!this._sceneElements.TryGetValue(sceneElement.Kind.ID, out list))
			{
				list = (this._sceneElements[sceneElement.Kind.ID] = new List<IAIStaticSceneElement>(64));
			}
			if (!list.Contains(sceneElement))
			{
				this._allElements.Add(sceneElement);
				this._allElements.Sort(this._comparison);
				list.Add(sceneElement);
				list.Sort(this._comparison);
			}
		}

		public void RemoveElement(IAIStaticSceneElement sceneElement)
		{
			this._allElements.Remove(sceneElement);
			List<IAIStaticSceneElement> list;
			if (!this._sceneElements.TryGetValue(sceneElement.Kind.ID, out list))
			{
				return;
			}
			list.Remove(sceneElement);
		}

		private int CompareElements(IAIStaticSceneElement a, IAIStaticSceneElement b)
		{
			return a.BoundsMin.x.CompareTo(b.BoundsMin.x);
		}

		private List<IAIStaticSceneElement> _allElements;

		private Dictionary<int, List<IAIStaticSceneElement>> _sceneElements;

		private Comparison<IAIStaticSceneElement> _comparison;
	}
}
