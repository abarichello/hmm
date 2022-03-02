using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public interface IUIWindowFactory
	{
		void LoadWindow(string windowName, Transform parentTransform, Action<UIWindow> onLoadCallback);
	}
}
