using System;
using UnityEngine;

namespace HeavyMetalMachines.DependencyInjection
{
	public interface IInjectionInstantiator
	{
		T Instantiate<T>(T prefab) where T : Object;
	}
}
