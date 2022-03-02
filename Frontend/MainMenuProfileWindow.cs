using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public abstract class MainMenuProfileWindow : StateGuiController
	{
		public abstract void OnLoading();

		public abstract void OnUnloading();

		public abstract void UpdateData();

		public abstract void SetWindowVisibility(bool visible);

		public abstract void OnPreBackToMainMenu();

		public abstract void OnBackToMainMenu();

		[SerializeField]
		protected Animation ScreenAlphaAnimation;

		[NonSerialized]
		public MainMenuProfileController MainMenuProfileController;
	}
}
