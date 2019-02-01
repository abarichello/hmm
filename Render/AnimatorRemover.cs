using System;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Render
{
	internal class AnimatorRemover : GameHubBehaviour
	{
		private void Awake()
		{
			SceneManager.sceneLoaded += this.OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			Animator[] array = UnityEngine.Object.FindObjectsOfType<Animator>();
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.Destroy(array[i]);
			}
		}

		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= this.OnSceneLoaded;
		}
	}
}
