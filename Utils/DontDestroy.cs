using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class DontDestroy : MonoBehaviour
	{
		private void Awake()
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
