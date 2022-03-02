using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection
{
	public class RedirectProjectContext : ProjectContext
	{
		protected override void GetInjectableMonoBehaviours(List<MonoBehaviour> monoBehaviours)
		{
			ProjectContext projectContext = this.FindProjectContext(SceneManager.GetActiveScene().name);
			if (projectContext == null)
			{
				return;
			}
			ProjectContext projectContext2 = Object.Instantiate<ProjectContext>(projectContext);
			base.Installers = projectContext2.Installers;
			base.InstallerPrefabs = projectContext2.InstallerPrefabs;
			base.ScriptableObjectInstallers = projectContext2.ScriptableObjectInstallers;
			base.GetInjectableMonoBehaviours(monoBehaviours);
		}

		private ProjectContext FindProjectContext(string sceneName)
		{
			if (this._scenesProjectContexts.All((SceneProjectContext spc) => spc.SceneName != sceneName))
			{
				Debug.LogWarning(string.Format("Could not find project context for scene {0}. Make sure it is correctly configured in the file Resources/ProjectContext.", sceneName));
				return null;
			}
			return this._scenesProjectContexts.First((SceneProjectContext spc) => spc.SceneName == sceneName).ProjectContext;
		}

		[SerializeField]
		private List<SceneProjectContext> _scenesProjectContexts;
	}
}
