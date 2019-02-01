using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ProfileHelper : MonoBehaviour
	{
		private void PrintCounts()
		{
			GUI.color = Color.red;
			GUILayout.Label("All " + Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)).Length, new GUILayoutOption[0]);
			GUILayout.Label("Textures " + Resources.FindObjectsOfTypeAll(typeof(Texture)).Length, new GUILayoutOption[0]);
			GUILayout.Label("AudioClips " + Resources.FindObjectsOfTypeAll(typeof(AudioClip)).Length, new GUILayoutOption[0]);
			GUILayout.Label("Meshes " + Resources.FindObjectsOfTypeAll(typeof(Mesh)).Length, new GUILayoutOption[0]);
			GUILayout.Label("Materials " + Resources.FindObjectsOfTypeAll(typeof(Material)).Length, new GUILayoutOption[0]);
			GUILayout.Label("GameObjects " + Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length, new GUILayoutOption[0]);
			GUILayout.Label("Components " + Resources.FindObjectsOfTypeAll(typeof(Component)).Length, new GUILayoutOption[0]);
		}

		private void PrintAllUsedMaterials()
		{
			this.PrintXXX<Texture2D>(delegate(Texture2D sceneObject)
			{
			});
			this.PrintXXX<Sprite>(delegate(Sprite sceneObject)
			{
			});
		}

		private void PrintXXX<T>(Action<T> action)
		{
			List<T> allObjectsInScene = this.GetAllObjectsInScene<T>();
			if (allObjectsInScene != null)
			{
				ProfileHelper.Log.WarnFormat("Found {0} items of type {1}", new object[]
				{
					allObjectsInScene.Count,
					typeof(T)
				});
				for (int i = 0; i < allObjectsInScene.Count; i++)
				{
					T obj = allObjectsInScene[i];
					action(obj);
				}
			}
		}

		private List<T> GetAllObjectsInScene<T>()
		{
			List<T> list = new List<T>();
			foreach (T item in Resources.FindObjectsOfTypeAll(typeof(T)) as T[])
			{
				list.Add(item);
			}
			return list;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ProfileHelper));
	}
}
