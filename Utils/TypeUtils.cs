using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class TypeUtils
	{
		public static List<Type> GetSubtypes<T>() where T : class
		{
			List<Type> list = new List<Type>();
			foreach (Assembly assembly in TypeUtils.GetValidAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)))
					{
						list.Add(type);
					}
				}
			}
			return list;
		}

		public static Type GetType(string typeString)
		{
			Type type = Type.GetType(typeString, false, true);
			if (type != null)
			{
				return type;
			}
			foreach (Assembly assembly in TypeUtils.GetValidAssemblies())
			{
				foreach (Type type2 in assembly.GetTypes())
				{
					if (type2.IsClass)
					{
						string text = type2.Name;
						if (type2.IsGenericType)
						{
							int num = type2.Name.IndexOf('`');
							if (num != -1)
							{
								text = type2.Name.Remove(type2.Name.IndexOf('`'));
							}
						}
						if (text.Equals(typeString, StringComparison.InvariantCultureIgnoreCase))
						{
							return type2;
						}
					}
				}
			}
			return null;
		}

		public static List<KeyValuePair<T, string>> GetTypeRefsInObj<T>(object obj, bool onlyInsideArray)
		{
			List<KeyValuePair<T, string>> list = new List<KeyValuePair<T, string>>();
			foreach (FieldInfo fieldInfo in obj.GetType().GetFields())
			{
				object value = fieldInfo.GetValue(obj);
				if (fieldInfo.FieldType.IsArray)
				{
					Array array = value as Array;
					if (array != null)
					{
						IEnumerator enumerator = array.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object obj2 = enumerator.Current;
								if (obj2 != null && obj2.GetType() == typeof(T))
								{
									T key = (T)((object)obj2);
									list.Add(new KeyValuePair<T, string>(key, fieldInfo.Name));
								}
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
					}
				}
				else if (!onlyInsideArray)
				{
				}
			}
			return list;
		}

		private static Assembly[] GetValidAssemblies()
		{
			List<Assembly> list = new List<Assembly>();
			string[] array = new string[]
			{
				"Assembly",
				"SharedUtils"
			};
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				bool flag = false;
				foreach (string value in array)
				{
					if (assembly.FullName.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
					{
						flag = true;
					}
				}
				if (flag)
				{
					list.Add(assembly);
				}
			}
			return list.ToArray();
		}

		public static FieldInfo[] GetCustomSerializationFields(object obj)
		{
			Type type = obj.GetType();
			FieldInfo[] fields = type.GetFields(TypeUtils.flags);
			return (from f in fields
			where !f.IsDefined(typeof(NonSerializedAttribute), true) && (!f.IsPrivate || (f.IsPrivate && f.IsDefined(typeof(SerializeField), false))) && !f.FieldType.IsSubclassOf(typeof(MonoBehaviour)) && (f.FieldType.IsAbstract || (f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<, >)))
			select f).ToArray<FieldInfo>();
		}

		private static BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
	}
}
