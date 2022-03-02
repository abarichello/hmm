using System;
using HeavyMetalMachines.Utils;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection
{
	[Serializable]
	public struct SceneProjectContext
	{
		[SceneName(false, false)]
		public string SceneName;

		public ProjectContext ProjectContext;
	}
}
