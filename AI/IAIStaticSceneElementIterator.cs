using System;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	public interface IAIStaticSceneElementIterator
	{
		void CallOnAllElementsInBounds(Vector2 minBound, Vector2 maxBound, Action<IAIStaticSceneElement> action);

		void CallOnElementsInBounds(Vector2 minBound, Vector2 maxBound, IAIElementKind elementKind, Action<IAIStaticSceneElement> action);
	}
}
