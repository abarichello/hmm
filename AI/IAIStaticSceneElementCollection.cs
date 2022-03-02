using System;

namespace HeavyMetalMachines.AI
{
	public interface IAIStaticSceneElementCollection
	{
		void AddElement(IAIStaticSceneElement sceneElement);

		void RemoveElement(IAIStaticSceneElement sceneElement);
	}
}
