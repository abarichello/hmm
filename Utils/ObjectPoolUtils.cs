using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Utils
{
	public class ObjectPoolUtils
	{
		public static void CreateObjectPool<T>(T objectReference, out T[] objectArray, int quantity, DiContainer container = null) where T : MonoBehaviour
		{
			ObjectPoolUtils.CreateInjectedObjectPool<T>(objectReference, out objectArray, quantity, container, 1, null);
		}

		public static void CreateInjectedObjectPool<T>(T objectReference, out T[] objectArray, int quantity, DiContainer container = null, int startIndex = 1, Transform parent = null) where T : MonoBehaviour
		{
			ObjectPoolUtils.PrepareObjectReference<T>(objectReference);
			objectArray = new T[quantity];
			objectArray[0] = objectReference;
			ObjectPoolUtils.InstantiateObjects<T>(objectReference, objectArray, startIndex, container, parent);
		}

		public static void ExpandObjectPool<T>(T objectReference, out T[] objectArray, int quantity, DiContainer container = null) where T : MonoBehaviour
		{
			ObjectPoolUtils.PrepareObjectReference<T>(objectReference);
			objectArray = new T[quantity];
			ObjectPoolUtils.InstantiateObjects<T>(objectReference, objectArray, 0, container, null);
		}

		private static void PrepareObjectReference<T>(T objectReference) where T : MonoBehaviour
		{
			objectReference.gameObject.SetActive(false);
			objectReference.transform.localPosition = Vector3.zero;
		}

		private static void InstantiateObjects<T>(T objectReference, T[] objectArray, int startIndex, DiContainer container = null, Transform parent = null) where T : MonoBehaviour
		{
			for (int i = startIndex; i < objectArray.Length; i++)
			{
				T t;
				if (container == null)
				{
					t = Object.Instantiate<T>(objectReference, Vector3.zero, Quaternion.identity, objectReference.transform.parent);
					t.transform.localPosition = Vector3.zero;
					t.transform.localScale = objectReference.transform.localScale;
					t.transform.localRotation = objectReference.transform.localRotation;
				}
				else if (parent == null)
				{
					t = container.InstantiatePrefab(objectReference, Vector3.zero, Quaternion.identity, objectReference.transform.parent).GetComponent<T>();
					t.transform.localPosition = Vector3.zero;
					t.transform.localScale = objectReference.transform.localScale;
					t.transform.localRotation = objectReference.transform.localRotation;
				}
				else
				{
					t = container.InstantiatePrefab(objectReference, Vector3.zero, Quaternion.identity, parent).GetComponent<T>();
				}
				t.gameObject.SetActive(false);
				objectArray[i] = t;
			}
		}

		public static bool TryToGetFreeObject<T>(T[] objectArray, ref T freeObject) where T : MonoBehaviour
		{
			foreach (T t in objectArray)
			{
				if (!t.gameObject.activeSelf)
				{
					freeObject = t;
					return true;
				}
			}
			return false;
		}
	}
}
