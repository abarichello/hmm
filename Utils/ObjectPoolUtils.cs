using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class ObjectPoolUtils
	{
		public static void CreateObjectPool<T>(T objectReference, out T[] objectArray, int quantity) where T : MonoBehaviour
		{
			objectReference.gameObject.SetActive(false);
			objectReference.transform.localPosition = Vector3.zero;
			objectArray = new T[quantity];
			objectArray[0] = objectReference;
			for (int i = 1; i < objectArray.Length; i++)
			{
				T t = UnityEngine.Object.Instantiate<T>(objectReference, Vector3.zero, Quaternion.identity, objectReference.transform.parent);
				t.transform.localPosition = Vector3.zero;
				t.transform.localScale = objectReference.transform.localScale;
				t.transform.localRotation = objectReference.transform.localRotation;
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
