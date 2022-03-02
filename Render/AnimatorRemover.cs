using System;
using Pocketverse;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Render
{
	internal class AnimatorRemover : GameHubBehaviour
	{
		private void Awake()
		{
			SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
		}

		private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			Animator[] array = Object.FindObjectsOfType<Animator>();
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy(array[i]);
			}
		}

		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
		}
	}
}
