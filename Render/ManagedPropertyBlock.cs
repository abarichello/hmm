using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class ManagedPropertyBlock
	{
		public ManagedPropertyBlock()
		{
			this._properties = new Dictionary<string, object>();
		}

		~ManagedPropertyBlock()
		{
			this._properties.Clear();
			this._properties = null;
		}

		public void Clear()
		{
			this._properties.Clear();
		}

		public bool HasProperty(string name)
		{
			return this._properties.ContainsKey(name);
		}

		public void SetProperty(string name, object value)
		{
			this._properties[name] = value;
		}

		public object GetProperty(string name)
		{
			return this._properties[name];
		}

		public void CopyTo(MaterialPropertyBlock output)
		{
			Dictionary<string, object>.Enumerator enumerator = this._properties.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, object> keyValuePair = enumerator.Current;
				if (keyValuePair.Value is Vector3)
				{
					output.SetVector(keyValuePair.Key, (Vector3)keyValuePair.Value);
				}
				else if (keyValuePair.Value is Vector4)
				{
					output.SetVector(keyValuePair.Key, (Vector4)keyValuePair.Value);
				}
				else if (keyValuePair.Value is Matrix4x4)
				{
					output.SetMatrix(keyValuePair.Key, (Matrix4x4)keyValuePair.Value);
				}
				else if (keyValuePair.Value is Color)
				{
					output.SetColor(keyValuePair.Key, (Color)keyValuePair.Value);
				}
				else if (keyValuePair.Value is float)
				{
					output.SetFloat(keyValuePair.Key, (float)keyValuePair.Value);
				}
				else if (keyValuePair.Value is Texture)
				{
					output.SetTexture(keyValuePair.Key, (Texture)keyValuePair.Value);
				}
			}
			enumerator.Dispose();
		}

		private Dictionary<string, object> _properties;
	}
}
