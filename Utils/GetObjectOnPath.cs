using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class GetObjectOnPath : MonoBehaviour
	{
		private void Awake()
		{
			if (string.IsNullOrEmpty(this.Path))
			{
				return;
			}
			GameObject gameObject = GameObject.Find(this.Path);
			AttachScript component = base.GetComponent<AttachScript>();
			if (component)
			{
				component.Target = gameObject.transform;
			}
			else
			{
				GetObjectOnPath.Log.ErrorFormat("Could not get GameObject on path: {0}. GameObjectName: {1}", new object[]
				{
					this.Path,
					base.gameObject.name
				});
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GetObjectOnPath));

		public string Path;
	}
}
